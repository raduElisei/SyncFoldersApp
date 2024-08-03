using Serilog;

namespace SyncFolders;

public class Program
{
    public static void Main(string[] args)
    {
        // Check if all args are given:
        // input path, folder path, sync interval, log path
        if (args.Length < 4)
        {
            Console.WriteLine("Not enough arguments. Please provide input folder path, output folder path, synchronization interval and log file path, in that order.");
            return;
        }

        int interval;
        if (!int.TryParse(args[2], out interval))
        {
            Console.WriteLine("Please enter a numeric argument for interval in minutes.");
            return;
        }

        var sourceDirectory = new DirectoryInfo(args[0]);
        var replicaDirectory = new DirectoryInfo(args[1]);
        var logDirectory = new DirectoryInfo(args[3]);

        if (!sourceDirectory.Exists)
        {
            Console.WriteLine($"Source directory not found: {sourceDirectory.FullName}");
            return;
        }

        if (!replicaDirectory.Exists)
        {
            Console.WriteLine($"Replica directory not found: {replicaDirectory.FullName}");
            return;
        }

        if( !logDirectory.Exists)
        {
            Console.WriteLine($"Log directory not found: {logDirectory.FullName}");
            return;
        }

        var arguments = new Arguments
        {
            SourcePath = args[0],
            ReplicaPath = args[1],
            Interval = interval,
            LogPath = args[3]
        };

        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.File(arguments.LogPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Logging.AddSerilog();

        builder.Services.AddSingleton(arguments);
        builder.Services.AddHostedService(sp =>
        {
            return new Worker(arguments);
        });

        var host = builder.Build();
        host.Run();
    }
}

// For the Veeam Test task
// Made with love by Radu