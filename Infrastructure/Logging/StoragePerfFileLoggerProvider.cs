using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DocuArchi.Api.Infrastructure.Logging
{
    public sealed class StoragePerfFileLoggerProvider : ILoggerProvider
    {
        private readonly string _logPath;
        private readonly ConcurrentDictionary<string, StoragePerfFileLogger> _loggers = new();

        public StoragePerfFileLoggerProvider(string logDirectory)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                throw new ArgumentException("Log directory requerido", nameof(logDirectory));
            }

            Directory.CreateDirectory(logDirectory);
            _logPath = Path.Combine(
                logDirectory,
                $"storage-perf-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log");
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, category => new StoragePerfFileLogger(category, _logPath));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        private sealed class StoragePerfFileLogger : ILogger
        {
            private static readonly object Sync = new();
            private readonly string _categoryName;
            private readonly string _logPath;

            public StoragePerfFileLogger(string categoryName, string logPath)
            {
                _categoryName = categoryName;
                _logPath = logPath;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull
            {
                return NullScope.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel >= LogLevel.Information;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel) || formatter == null)
                {
                    return;
                }

                var message = formatter(state, exception);
                if (string.IsNullOrWhiteSpace(message) ||
                    (!message.Contains("StoragePerf", StringComparison.Ordinal) &&
                     !message.Contains("StoragePerfError", StringComparison.Ordinal)))
                {
                    return;
                }

                var line = string.Create(
                    CultureInfo.InvariantCulture,
                    $"{DateTimeOffset.UtcNow:O} [{logLevel}] {_categoryName} {message}");

                lock (Sync)
                {
                    File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
