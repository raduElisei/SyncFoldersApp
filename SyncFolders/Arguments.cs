namespace SyncFolders;

public class Arguments
{
    public string SourcePath { get; set; } = string.Empty;
    public string ReplicaPath { get; set; } = string.Empty;
    public int Interval { get; set; }
    public string LogPath { get; set; } = string.Empty;
}
