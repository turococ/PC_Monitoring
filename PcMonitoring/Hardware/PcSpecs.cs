namespace HardwareMonitor.Hardware;

public record PcSpecs(
    string MotherBoard,
    string Cpu,
    string Gpu,
    string Ram,
    string SSD,
    string HDD
);