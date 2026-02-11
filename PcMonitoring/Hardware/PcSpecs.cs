namespace HardwareMonitor.Hardware;

public record PcSpecs(
    string MotherBoard,
    string Cpu,
    string Gpu,
    string Ram,
    IReadOnlyList<string> Disks
);