using MiApp.Services.Service.DocumentProcessing.Core;
using Microsoft.Extensions.Options;

namespace DocuArchi.Api.Infrastructure.HostedServices
{
    public sealed class OfficeWorkerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OfficeWorkerHostedService> _logger;
        private readonly DocumentProcessingOfficeWorkerOptions _options;

        public OfficeWorkerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<DocumentProcessingOfficeWorkerOptions>? options,
            ILogger<OfficeWorkerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options?.Value ?? new DocumentProcessingOfficeWorkerOptions();
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
            if (string.IsNullOrWhiteSpace(_options.DefaultDbAlias))
            {
                _logger.LogWarning("office_worker_hosted_skipped errorCode={ErrorCode}", "OFFICE_WORKER_ALIAS_REQUIRED");
                return;
            }

            var maxBatch = Math.Max(1, _options.MaxBatchSize);
            for (var index = 0; index < maxBatch && !stoppingToken.IsCancellationRequested; index++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var coordinator = scope.ServiceProvider.GetRequiredService<IDocumentProcessingOfficeWorkerCoordinator>();
                    var result = await coordinator.ExecuteNextAsync(
                        _options.DefaultDbAlias,
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
                _logger.LogError(exception, message, args);
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
