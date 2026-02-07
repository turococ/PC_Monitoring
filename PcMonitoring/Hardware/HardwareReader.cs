using LibreHardwareMonitor.Hardware;

namespace HardwareMonitor.Hardware;

public class HardwareReader
{
    public PcSpecs ReadPcSpec()
    {
        var computer = CreateComputer();
        computer.Open();

        string MotherBoard = "Unknown";
        string cpu = "Unknown";
        string gpu = "Unknown";
        string ram = "Unknown";
        string ssd = "Unknown";
        string hdd = "Unknown";

        foreach (var hardware in computer.Hardware)
        {
            hardware.Update();

            switch (hardware.HardwareType)
            {
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

                case HardwareType.Storage:
                    ssd = ReadSSD(hardware);
                    //hdd = ReadHDD(hardware);
                    break;
            }
        }

        return new PcSpecs(MotherBoard, cpu, gpu, ram, ssd, hdd);
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

    private static string ReadSSD(IHardware storage)
    {
        float total = 0;

        storage.Update();

        foreach (var sensor in storage.Sensors)
        {
            if (sensor.SensorType != SensorType.Data || !sensor.Value.HasValue)
                continue;

            if (sensor.Name.Contains("Total", StringComparison.OrdinalIgnoreCase))
            {
                total = sensor.Value.Value;
                break;
            }
        }

        return total > 0
            ? $"{Math.Round(total)} GB" : "Unknown";
    }
}