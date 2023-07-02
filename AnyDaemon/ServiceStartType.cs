using FastEnumUtility;

namespace AnyDaemon;

public enum ServiceStartType
{
    [Label("auto")]
    Auto,
    [Label("delayed-auto")]
    Delayed,
    [Label("demand")]
    Demand,
    [Label("disabled")]
    Disabled
}
