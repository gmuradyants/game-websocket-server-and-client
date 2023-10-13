using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Game.Shared.Logging;

public static class LoggerProvider
{
    private const string OutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:l}{NewLine}{Exception}";

    public static Logger CreateLogger(string? basePathToWriteLogs = null, LogEventLevel minimumMicrosoftLevelToOverride = LogEventLevel.Warning)
    {
        if (string.IsNullOrEmpty(basePathToWriteLogs)) { basePathToWriteLogs = AppDomain.CurrentDomain.BaseDirectory; }

        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", minimumMicrosoftLevelToOverride)
            .WriteTo.Logger(
                x => x.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Verbose)
                    .WriteTo.File(
                        Path.Combine(basePathToWriteLogs, "logs/verbose/verbose.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: OutputTemplate))
            .WriteTo.Logger(
                x => x.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                    .WriteTo.File(
                        Path.Combine(basePathToWriteLogs, "logs/info/info.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: OutputTemplate))
            .WriteTo.Logger(
                x => x.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.File(
                        Path.Combine(basePathToWriteLogs, "logs/Warning/Warning.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: OutputTemplate))
            .WriteTo.Logger(
                x => x.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File(
                        Path.Combine(basePathToWriteLogs, "logs/error/error.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: OutputTemplate))
            .WriteTo.Logger(
                x => x.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                    .WriteTo.File(
                        Path.Combine(basePathToWriteLogs, "logs/fatal/fatal.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: OutputTemplate))
            .WriteTo.Console().CreateLogger();
    }
}