using Serilog;

namespace SyncFolders;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Logging.AddSerilog();

        // Check if all args are given:
        // input path, folder path, sync interval, log path
        if (args.Length <= 2)
        {
            Log.Error("Not enough arguments. Please provide input folder path, output folder path, synchronization interval and log file path, in that order.");
            return;
        }

        int interval;
        if (!int.TryParse(args[2], out interval))
        {
            Log.Error("Please enter a numeric argument for interval in minutes.");
            return;
        }

        var sourceDirectory = new DirectoryInfo(args[0]);
        var replicaDirectory = new DirectoryInfo(args[1]);
        var logDirectory = new DirectoryInfo(args[3]);

        if (!sourceDirectory.Exists)
        {
            Log.Error($"Input directory not found: {sourceDirectory.FullName}");
            return;
        }

        if (!replicaDirectory.Exists)
        {
            Log.Error($"Input directory not found: {replicaDirectory.FullName}");
            return;
        }

        if( !logDirectory.Exists)
        {
            Log.Error($"Input directory not found: {logDirectory.FullName}");
            return;
        }

        var arguments = new Arguments
        {
            SourcePath = args[0],
            ReplicaPath = args[1],
            Interval = interval,
            LogPath = args[3]
        };

        builder.Services.AddSingleton(arguments);
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}

// For the Veeam Test task
// Made with love by Radu