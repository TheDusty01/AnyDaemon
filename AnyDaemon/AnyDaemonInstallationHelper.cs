namespace AnyDaemon;

public static class AnyDaemonInstallationHelper
{
    public const string ServiceRunnerExecutableName = "AnyDaemon.Service.exe";
    public const string AnyDaemonHomeVariable = "ANYDAEMON_HOME";
    public const string ServiceNameArgument = "name";

    private const string SavedServicesDirectoryName = $"UserServices";

    public static bool IsInstalled => GetInstallationDir() is not null;

    public static string? GetInstallationDir()
    {
        return Environment.GetEnvironmentVariable(AnyDaemonHomeVariable);
    }

    public static string? GetInstallationPath()
    {
        var installationDir = GetInstallationDir();
        if (installationDir is null)
            return null;

        return Path.Combine(installationDir, ServiceRunnerExecutableName);
    }

    public static string GetConfigPath(string serviceName)
    {
        var path = GetInstallationDir() ?? throw new AnyDaemonNotInstalledException();

        return Path.Combine(
            path,
            SavedServicesDirectoryName,
            $"{serviceName}.json"
        );
    }

    public static void ThrowIfNotInstalled()
    {
        if (!IsInstalled)
            throw new AnyDaemonNotInstalledException();
    }
}
