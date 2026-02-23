using LibreHardwareMonitor.Hardware;
using System.Management;
using System.Collections.Generic;

namespace HardwareMonitor.Hardware;

public class HardwareReader : IDisposable
{
    private readonly Computer _computer;
    private readonly Visitor _visitor = new();

    public HardwareReader()
    {
        _computer = CreateComputer();
        _computer.Open();
        _computer.Accept(_visitor);
    }

    public PcSpecs ReadPcSpec()
    {
        _computer.Traverse(_visitor);
        var info = _visitor.Info;
        var disks = ReadDisks();
        return new PcSpecs(info.Motherboard, info.Cpu, info.Gpu, info.RamTotal, disks);
    }

    public HardwareMetrics GetCurrentMetrics()
    {
        _computer.Traverse(_visitor);
        return _visitor.Metrics;
    }

    private static Computer CreateComputer() => new()
    {
        IsMotherboardEnabled = true,
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true,
        IsStorageEnabled = true,
        IsControllerEnabled = false,
        IsNetworkEnabled = false
    };

    private static List<string> ReadDisks()
    {
        var disks = new List<string>();
        try
        {
            var searcher = new System.Management.ManagementObjectSearcher(
                "SELECT Model, Size FROM Win32_DiskDrive");
            foreach (System.Management.ManagementObject disk in searcher.Get())
            {
                var name = disk["Model"]?.ToString() ?? "Unknown";
                var size = (ulong)(disk["Size"] ?? 0);
                var gb = (float)Math.Round(size / 1024.0 / 1024.0 / 1024.0, 1);
                disks.Add($"{name} — {gb} GB");
            }
        }
        catch { disks.Add("Не удалось получить информацию о дисках"); }
        return disks;
    }

    public void Dispose() => _computer?.Close();
}