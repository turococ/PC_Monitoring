using HardwareMonitor.Hardware;

public class PcSpecsViewModel
{
    public string MotherBoard { get; }
    public string Cpu { get; }
    public string Gpu { get; }
    public string Ram { get; }
    public IReadOnlyList<string> Disks { get; }

    public PcSpecsViewModel(PcSpecs specs)
    {
        MotherBoard = $"MotherBoard: {specs.MotherBoard}";
        Cpu = $"CPU: {specs.Cpu}";
        Gpu = $"GPU: {specs.Gpu}";
        Ram = $"RAM: {specs.Ram}";
        Disks = specs.Disks;
    }
}
