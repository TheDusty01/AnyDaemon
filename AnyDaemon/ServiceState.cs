namespace AnyDaemon;

public enum ServiceState
{
    // SERVICE_STOPPED
    // 0x00000001
    // The service is not running.
    Stopped = 1,

    // SERVICE_START_PENDING
    // 0x00000002
    // The service is starting.
    StartPending = 2,

    // SERVICE_STOP_PENDING
    // 0x00000003
    // The service is stopping.
    StopPending = 3,

    // SERVICE_RUNNING
    // 0x00000004
    // The service is running.
    Running = 4,

    // SERVICE_CONTINUE_PENDING
    // 0x00000005
    // The service continue is pending.
    ContinuePending = 5,

    // SERVICE_PAUSE_PENDING
    // 0x00000006
    // The service pause is pending.
    PausePending = 6,

    // SERVICE_PAUSED
    // 0x00000007
    // The service is paused.
    Paused = 7,
}
