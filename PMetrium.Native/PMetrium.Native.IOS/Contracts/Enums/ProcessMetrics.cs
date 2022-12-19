namespace TestXml.Contracts.Enums;

public enum ProcessMetrics
{
    StartTime = 0,
    Duration,
    Process = 2,
    ResponsibleProcess,
    ProcessID,
    UserName,
    Cpu = 6,
    CpuTime,
    Threads,
    Ports,
    Memory = 10,
    RealMem,
    RealPrivateMem,
    RealSharedMem,
    Kind,
    SuddenTermination,
    Sandbox,
    Restricted,
    IdleWakeUps,
    AppNap,
    PurgeableMem,
    CompressedMem,
    DiskWrites,
    DiskReads,
    PreventingSleep
}