namespace AnyDaemon;

public class DaemonDescriptor
{
    public required string ExecutablePath { get; set; }
    public required string WorkingDir { get; set; }
    public string? Arguments { get; set; }
    public required string Name { get; set; }
    public ServiceStartType StartType { get; set; } = ServiceStartType.Delayed;

    public required string DisplayName { get; set; }
}
