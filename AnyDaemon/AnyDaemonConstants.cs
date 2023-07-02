using System.Text.Json;

namespace AnyDaemon;

public static class AnyDaemonConstants
{
    public static readonly JsonSerializerOptions IntendedJsonOptions = new()
    {
        WriteIndented = true,
    };
}