namespace HardwareMonitor.Hardware;

public record PcSpecs(
    string Motherboard,
    string Cpu,
    string Gpu,
    string RamTotal,
    IReadOnlyList<string> Disks
);