using MiApp.Services.Service.DocumentProcessing.Core;
using Microsoft.Extensions.Options;

namespace DocuArchi.Api.Infrastructure.HostedServices
{
    public sealed class OcrWorkerHostedService : BackgroundService
    {
        private long _recoveryCursor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OcrWorkerHostedService> _logger;
        private readonly DocumentProcessingOcrWorkerOptions _options;

        public OcrWorkerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<DocumentProcessingOcrWorkerOptions>? options,
            ILogger<OcrWorkerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options?.Value ?? new DocumentProcessingOcrWorkerOptions();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("docproc_ocr_worker_hosted_disabled");
                return;
            }

            _logger.LogInformation(
                "docproc_ocr_worker_hosted_started defaultDbAlias={DefaultDbAlias} pollIntervalSeconds={PollIntervalSeconds} maxBatchSize={MaxBatchSize}",
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
                _logger.LogDebug("docproc_ocr_worker_hosted_stopped");
            }
            catch (Exception ex)
            {
                SafeLogError(ex, "docproc_ocr_worker_hosted_failed errorCode={ErrorCode}", "DOCPROC_OCR_WORKER_HOSTED_FAILED");
            }
        }

        private async Task RunBatchAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_options.DefaultDbAlias))
            {
                _logger.LogWarning("docproc_ocr_worker_hosted_skipped errorCode={ErrorCode}", "DOCPROC_OCR_WORKER_ALIAS_REQUIRED");
                return;
            }

            try {
                using var recoveryScope = _scopeFactory.CreateScope();
                var recovery = recoveryScope.ServiceProvider.GetService<MiApp.Services.Service.DocumentProcessing.IntegracionAlmacenamientoContextoSolicitud.IStorageProcessingAdmissionService>();
                if (recovery is not null) {
                    var page = await recovery.ReconcileAsync(_options.DefaultDbAlias, _recoveryCursor, stoppingToken);
                    _recoveryCursor = page.NextCursor;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch { _logger.LogWarning("StorageProcessing reconciliation remains pending errorCode={ErrorCode}", "STORAGE_PROCESSING_RECOVERY_PENDING"); }
            var maxBatch = Math.Max(1, _options.MaxBatchSize);
            for (var index = 0; index < maxBatch && !stoppingToken.IsCancellationRequested; index++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var coordinator = scope.ServiceProvider.GetRequiredService<IDocumentProcessingOcrWorkerCoordinator>();
                    var result = await coordinator.ExecuteNextAsync(
                        _options.DefaultDbAlias,
                        string.IsNullOrWhiteSpace(_options.WorkerUser) ? "ocr-worker" : _options.WorkerUser.Trim(),
                        stoppingToken);

                    if (result.FinalStatus == "no_job")
                    {
                        return;
                    }

                    _logger.LogInformation(
                        "docproc_ocr_worker_hosted_iteration docprocJobId={DocprocJobId} ocrJobId={OcrJobId} finalStatus={FinalStatus} success={Success} errorCode={ErrorCode}",
                        result.DocprocJobId,
                        result.OcrJobId,
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
                    SafeLogError(ex, "docproc_ocr_worker_hosted_iteration_failed errorCode={ErrorCode}", "DOCPROC_OCR_WORKER_HOSTED_FAILED");
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
                // Logging providers must not terminate the OCR background worker.
            }
        }
    }

    public sealed class DocumentProcessingOcrWorkerOptions
    {
        public bool Enabled { get; set; }
        public string DefaultDbAlias { get; set; } = string.Empty;
        public string WorkerUser { get; set; } = "ocr-worker";
        public int PollIntervalSeconds { get; set; } = 10;
        public int MaxBatchSize { get; set; } = 1;
    }
}
