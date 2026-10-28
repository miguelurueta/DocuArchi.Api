using MiApp.Services.Service.DocumentProcessing.Core;
using Microsoft.Extensions.Options;
using MiApp.Models.Models.DocumentProcessing.IntegracionAlmacenamientoContextoSolicitud;
using MiApp.Services.Service.DocumentProcessing.IntegracionAlmacenamientoContextoSolicitud;

namespace DocuArchi.Api.Infrastructure.HostedServices
{
    public sealed class OfficeWorkerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OfficeWorkerHostedService> _logger;
        private readonly DocumentProcessingOfficeWorkerOptions _options;
        private readonly StorageProcessingOptions _integration;
        private readonly StorageProcessingDispatchOptions _dispatch;
        private readonly Dictionary<string, long> _recoveryCursors = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> _dispatchCursors = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> _backfillCursors = new(StringComparer.OrdinalIgnoreCase);

        public OfficeWorkerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<DocumentProcessingOfficeWorkerOptions>? options,
            ILogger<OfficeWorkerHostedService> logger,
            IOptions<StorageProcessingOptions>? integration = null,
            IOptions<StorageProcessingDispatchOptions>? dispatch = null)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options?.Value ?? new DocumentProcessingOfficeWorkerOptions();
            _integration = integration?.Value ?? new();
            _dispatch = dispatch?.Value ?? new();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("office_worker_hosted_disabled");
                return;
            }

            _logger.LogInformation(
                "office_worker_hosted_started defaultDbAlias={DefaultDbAlias} pollIntervalSeconds={PollIntervalSeconds} maxBatchSize={MaxBatchSize}",
                _options.DefaultDbAlias,
                _options.PollIntervalSeconds,
                _options.MaxBatchSize);

            try
            {
                using var timer = new PeriodicTimer(GetInterval());
                await RunBatchAsync(stoppingToken);
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunBatchAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("office_worker_hosted_stopped");
            }
            catch (Exception ex)
            {
                SafeLogError(ex, "office_worker_hosted_failed errorCode={ErrorCode}", "OFFICE_WORKER_HOSTED_FAILED");
            }
        }

        private async Task RunBatchAsync(CancellationToken stoppingToken)
        {
            foreach (var alias in StorageProcessingWorkerAliases.Resolve(_scopeFactory, _options.DefaultDbAlias, _integration, _dispatch)) {
                stoppingToken.ThrowIfCancellationRequested();
                await RunAliasAsync(alias, stoppingToken);
            }
        }

        private async Task RunAliasAsync(string alias, CancellationToken stoppingToken)
        {
            try {
                using var integrationScope = _scopeFactory.CreateScope();
                var recovery = integrationScope.ServiceProvider.GetService<IStorageProcessingAdmissionService>();
                if (recovery is not null && _integration.Enabled && _dispatch.Enabled)
                    _recoveryCursors[alias] = (await recovery.ReconcileAsync(alias, _recoveryCursors.GetValueOrDefault(alias), stoppingToken)).NextCursor;
                var dispatcher = integrationScope.ServiceProvider.GetService<IStorageProcessingDispatcher>();
                if (dispatcher is not null) _dispatchCursors[alias] = await dispatcher.RunPageAsync(alias, _dispatchCursors.GetValueOrDefault(alias), stoppingToken);
                var backfill = integrationScope.ServiceProvider.GetService<IStorageProcessingBackfillService>();
                if (backfill is not null) _backfillCursors[alias] = await backfill.RunPageAsync(alias, _backfillCursors.GetValueOrDefault(alias), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch { _logger.LogWarning("StorageProcessing office reconciliation pending errorCode={ErrorCode}", "STORAGE_PROCESSING_RECOVERY_PENDING"); }
            var maxBatch = Math.Max(1, _options.MaxBatchSize);
            for (var index = 0; index < maxBatch && !stoppingToken.IsCancellationRequested; index++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var coordinator = scope.ServiceProvider.GetRequiredService<IDocumentProcessingOfficeWorkerCoordinator>();
                    var result = await coordinator.ExecuteNextAsync(
                        alias,
                        string.IsNullOrWhiteSpace(_options.WorkerUser) ? "office-worker" : _options.WorkerUser.Trim(),
                        stoppingToken);

                    if (result.FinalStatus == "no_job")
                    {
                        return;
                    }

                    _logger.LogInformation(
                        "office_worker_hosted_iteration docprocJobId={DocprocJobId} officeJobId={OfficeJobId} finalStatus={FinalStatus} success={Success} errorCode={ErrorCode}",
                        result.DocprocJobId,
                        result.OfficeJobId,
                        result.FinalStatus,
                        result.Success,
                        result.ErrorCode);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    SafeLogError(ex, "office_worker_hosted_iteration_failed errorCode={ErrorCode}", "OFFICE_WORKER_HOSTED_FAILED");
                }
            }
        }

        private TimeSpan GetInterval()
        {
            return TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds));
        }

        private void SafeLogError(Exception exception, string message, params object[] args)
        {
            try
            {
                _logger.LogError(message, args);
            }
            catch
            {
                // Logging providers must not terminate the Office background worker.
            }
        }
    }

    public sealed class DocumentProcessingOfficeWorkerOptions
    {
        public bool Enabled { get; set; }
        public string DefaultDbAlias { get; set; } = string.Empty;
        public string WorkerUser { get; set; } = "office-worker";
        public int PollIntervalSeconds { get; set; } = 10;
        public int MaxBatchSize { get; set; } = 1;
    }
}
