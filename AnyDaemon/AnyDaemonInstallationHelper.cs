namespace AnyDaemon;

public static class AnyDaemonInstallationHelper
{
    public const string ServiceRunnerExecutableName = "AnyDaemon.Service.exe";
    public const string AnyDaemonHomeVariable = "ANYDAEMON_HOME";
    public const string ServiceNameArgument = "name";

    public static readonly string AnyDaemonSavedServiceConfigPath = $"UserServices{Path.DirectorySeparatorChar}";

    public static bool IsInstalled => GetInstallationPath() is not null;

    public static string? GetInstallationPath()
    {
        return Environment.GetEnvironmentVariable(AnyDaemonHomeVariable);
    }

    public static string GetConfigPath(string serviceName)
    {
        return Path.Combine(
            GetInstallationPath()!,
            AnyDaemonSavedServiceConfigPath,
            $"{serviceName}.json"
        );
    }
}
