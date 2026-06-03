using MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload;
using MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload;
using Microsoft.Extensions.Options;

namespace DocuArchi.Api.Infrastructure.HostedServices
{
    public sealed class StorageUploadCleanupHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StorageUploadCleanupHostedService> _logger;
        private readonly StorageUploadOptions _options;

        public StorageUploadCleanupHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<StorageUploadOptions>? options,
            ILogger<StorageUploadCleanupHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options?.Value ?? new StorageUploadOptions();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunCleanupAsync(stoppingToken);

            using var timer = new PeriodicTimer(GetInterval());
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunCleanupAsync(stoppingToken);
            }
        }

        private async Task RunCleanupAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cleanup = scope.ServiceProvider.GetRequiredService<IStorageUploadCleanupService>();
                var removed = await cleanup.CleanupExpiredAsync();
                if (removed > 0)
                {
                    _logger.LogInformation("Limpieza temporales upload removio {Removed} directorios", removed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No fue posible ejecutar limpieza periodica de temporales upload");
            }
        }

        private TimeSpan GetInterval()
        {
            var minutes = _options.CleanupIntervalMinutes <= 0 ? 60 : _options.CleanupIntervalMinutes;
            return TimeSpan.FromMinutes(minutes);
        }
    }
}
