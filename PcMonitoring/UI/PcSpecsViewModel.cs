using HardwareMonitor.Hardware;

namespace PcMonitoring.UI;

public class PcSpecsViewModel
{
    public string MotherBoard { get; }
    public string Cpu { get; }
    public string Gpu { get; }
    public string Ram { get; }
    public string SSD { get; }
    public string HDD { get; }

    public PcSpecsViewModel(PcSpecs specs)
    {
        MotherBoard = $"MotherBoard: {specs.MotherBoard}";
        Cpu = $"CPU: {specs.Cpu}";
        Gpu = $"GPU: {specs.Gpu}";
        Ram = $"RAM: {specs.Ram}";
        SSD = $"SSD: {specs.SSD}";
        HDD = $"HDD: {specs.HDD}";
    }
}
