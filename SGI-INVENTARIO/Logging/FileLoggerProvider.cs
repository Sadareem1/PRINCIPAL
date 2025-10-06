using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace SGI_INVENTARIO.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(_filePath, name));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }

    private class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly string _categoryName;
        private static readonly object _lock = new();

        public FileLogger(string filePath, string categoryName)
        {
            _filePath = filePath;
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            var line = $"{DateTime.UtcNow:O} [{logLevel}] {_categoryName}: {message}";
            if (exception is not null)
            {
                line += Environment.NewLine + exception;
            }

            lock (_lock)
            {
                File.AppendAllText(_filePath, line + Environment.NewLine);
            }
        }
    }
}
