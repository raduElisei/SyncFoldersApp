using Serilog;
using System.IO;
using System.Threading;
using System.Timers;
using ILogger = Serilog.ILogger;

namespace SyncFolders;

public class Worker : BackgroundService, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<Worker>();
    private readonly Arguments _arguments;
    private System.Timers.Timer _timer;
    private FileSystemWatcher _fileSystemWatcher;

    public Worker(Arguments arguments)
    {
        _arguments = arguments;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Worker start at {time}", DateTimeOffset.Now);

        _timer = new System.Timers.Timer(_arguments.Interval * 60 * 1000);
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;
        _timer.Enabled = true;

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Worker stopping at: {time}", DateTimeOffset.Now);
        _fileSystemWatcher.Dispose();
        return base.StopAsync(cancellationToken);
    }

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        if (_fileSystemWatcher != null)
        {
            return;
        }

        _fileSystemWatcher = new FileSystemWatcher(_arguments.SourcePath)
        {
            NotifyFilter =
                NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastWrite |
                NotifyFilters.Security |
                NotifyFilters.Size,
            Filter = "*.*",
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };

        _fileSystemWatcher.Changed += OnChanged;
        _fileSystemWatcher.Created += OnCreated;
        _fileSystemWatcher.Deleted += OnDeleted;
        _fileSystemWatcher.Renamed += OnRenamed;
        _fileSystemWatcher.Error += OnError;

        Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(t =>
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Dispose();
            _logger.Information("FileSystemWatcher deactivated.");
        });
    }

    #region FileSystemWatcher Event Handlers
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.Information($"File changed: {e.FullPath}");
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _logger.Information($"File created: {e.FullPath}");
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.Information($"File deleted: {e.FullPath}");
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _logger.Information($"File renamed from {e.OldFullPath} to {e.FullPath}");
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        _logger.Error(e.GetException(), "File system watcher error");
    }

    #endregion
}
