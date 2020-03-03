using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Nuget = NuGet.Common;

namespace DepGraph.Logging
{
    public class NugetLoggerWrapper : Nuget.ILogger
    {
        private readonly ILogger _logger;

        public NugetLoggerWrapper(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Log(Nuget.LogLevel level, string data)
        {
            switch (level)
            {
                case Nuget.LogLevel.Debug:
                    LogDebug(data);
                    return;
                case Nuget.LogLevel.Error:
                    LogError(data);
                    return;
                case Nuget.LogLevel.Information:
                    LogInformation(data);
                    return;
                case Nuget.LogLevel.Minimal:
                    LogMinimal(data);
                    return;
                case Nuget.LogLevel.Verbose:
                    LogVerbose(data);
                    return;
                case Nuget.LogLevel.Warning:
                    LogWarning(data);
                    return;
            }
        }

        public void Log(Nuget.ILogMessage message)
        {
            if (message == null) return;

            Log(message.Level, message.Message);
        }
        
        public Task LogAsync(Nuget.LogLevel level, string data)
        {
            Log(level, data);
            return Task.CompletedTask;
        }

        public Task LogAsync(Nuget.ILogMessage message)
        {
            Log(message);
            return Task.CompletedTask;
        }

        public void LogDebug(string data) => _logger.LogDebug(data);

        public void LogError(string data) => _logger.LogError(data);

        public void LogInformation(string data) => _logger.LogInformation(data);

        public void LogInformationSummary(string data) => LogInformation(data);

        public void LogMinimal(string data) => LogInformation(data);

        public void LogVerbose(string data) => _logger.LogTrace(data);

        public void LogWarning(string data) => _logger.LogWarning(data);
    }
}
