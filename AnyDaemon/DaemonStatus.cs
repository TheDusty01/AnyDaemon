﻿namespace AnyDaemon;

public class DaemonStatus
{
    //typedef struct _SERVICE_STATUS
    //{
    //    DWORD dwServiceType;
    //    DWORD dwCurrentState;
    //    DWORD dwControlsAccepted;
    //    DWORD dwWin32ExitCode;
    //    DWORD dwServiceSpecificExitCode;
    //    DWORD dwCheckPoint;
    //    DWORD dwWaitHint;
    //}
    //SERVICE_STATUS, *LPSERVICE_STATUS;

    public required ServiceState State { get; set; }
}
