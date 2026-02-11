using LibreHardwareMonitor.Hardware;
using System.Management;

namespace HardwareMonitor.Hardware;

public class HardwareReader
{
    public PcSpecs ReadPcSpec()
    {
        var computer = CreateComputer();
        computer.Open();

        string motherBoard = "Unknown";
        string cpu = "Unknown";
        string gpu = "Unknown";
        string ram = "Unknown";

        foreach (var hardware in computer.Hardware)
        {
            hardware.Update();

            switch (hardware.HardwareType)
            {
                case HardwareType.Motherboard:
                    motherBoard = hardware.Name;
                    break;

                case HardwareType.Cpu:
                    cpu = hardware.Name;
                    break;

                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    gpu = hardware.Name;
                    break;

                case HardwareType.Memory:
                    ram = ReadRam(hardware);
                    break;
            }
        }

        computer.Close();

        var disks = ReadDisk();

        return new PcSpecs(motherBoard, cpu, gpu, ram, disks);
    }


    private static Computer CreateComputer() => new()
    {
        IsMotherboardEnabled = true,
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true,
        IsStorageEnabled = true
    };

    private static string ReadRam(IHardware memory)
    {
        float used = 0;
        float available = 0;

        foreach (var sensor in memory.Sensors)
        {
            if (sensor.SensorType != SensorType.Data || !sensor.Value.HasValue)
                continue;

            if (sensor.Name == "Memory Used")
                used = sensor.Value.Value;

            if (sensor.Name == "Memory Available")
                available = sensor.Value.Value;
        }

        var total = used + available;

        return total > 0
            ? $"{Math.Round(total)} GB"
            : "Unknown";
    }
    private static List<string> ReadDisk()
    {
        var disks = new List<string>();
        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

        foreach (ManagementObject disk in searcher.Get())
        {
            string name = disk["Model"]?.ToString() ?? "Unknown";
            ulong size = (ulong)(disk["Size"] ?? 0);
            disks.Add($"{name}: {Math.Round(size / 1024.0 / 1024.0 / 1024.0, 2)} GB");
        }

        return disks;
    }
}