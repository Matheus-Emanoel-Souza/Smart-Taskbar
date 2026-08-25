using Serilog;
using Serilog.Events;

namespace SmartTaskbar.Infrastructure.Logging;

/// <summary>
/// Configura o logger global. Nível padrão Information, arquivo com buffer (menos I/O
/// síncrono) e retenção de 7 dias — o app fica aberto o tempo todo, então log sem rotação
/// cresceria indefinidamente.
/// </summary>
public static class LoggerSetup
{
    public static ILogger CreateLogger(string dataDirectory)
    {
        var logPath = Path.Combine(dataDirectory, "logs", "smarttaskbar-.log");

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                buffered: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
