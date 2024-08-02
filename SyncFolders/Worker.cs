using Serilog;
using ILogger = Serilog.ILogger;

namespace SyncFolders;

public class Worker : BackgroundService
{
    private static readonly ILogger _logger = Log.ForContext<Worker>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.Information("Worker running at {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
