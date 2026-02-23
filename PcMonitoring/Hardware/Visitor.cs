using LibreHardwareMonitor.Hardware;
using System;
using System.Linq;

namespace HardwareMonitor.Hardware
{
    public class Visitor : IVisitor
    {
        public HardwareInfo Info { get; } = new();
        public HardwareMetrics Metrics { get; } = new();

        public void VisitComputer(IComputer computer) => computer.Traverse(this);

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();

            switch (hardware.HardwareType)
            {
                case HardwareType.Motherboard:
                    Info.Motherboard = hardware.Name;
                    break;
                case HardwareType.Cpu:
                    Info.Cpu = hardware.Name;
                    ExtractCpuMetrics(hardware);
                    break;
                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    Info.Gpu = hardware.Name;
                    ExtractGpuMetrics(hardware);
                    break;
                case HardwareType.Memory:
                    ExtractRamMetrics(hardware);
                    break;
            }

            foreach (var sub in hardware.SubHardware)
                sub.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }

        private void ExtractCpuMetrics(IHardware cpu)
        {
            foreach (var s in cpu.Sensors)
            {
                if (s.SensorType == SensorType.Load && s.Name == "CPU Total")
                    Metrics.CpuLoad = s.Value;
                if (s.SensorType == SensorType.Temperature && s.Name == "CPU Package")
                    Metrics.CpuTemp = s.Value;
            }
        }

        private void ExtractGpuMetrics(IHardware gpu)
        {
            foreach (var s in gpu.Sensors)
            {
                if (s.SensorType == SensorType.Load && s.Name.Contains("GPU Core"))
                    Metrics.GpuLoad = s.Value;
                if (s.SensorType == SensorType.Temperature && s.Name.Contains("GPU Core"))
                    Metrics.GpuTemp = s.Value;
            }
        }

        private void ExtractRamMetrics(IHardware memory)
        {
            float used = 0, available = 0;
            foreach (var s in memory.Sensors)
            {
                if (s.SensorType != SensorType.Data || !s.Value.HasValue) continue;
                if (s.Name == "Memory Used") used = s.Value.Value;
                if (s.Name == "Memory Available") available = s.Value.Value;
            }

            var total = used + available;
            if (total > 0)
            {
                Metrics.RamUsedGB = (float)Math.Round(used, 1);
                Metrics.RamTotalGB = (float)Math.Round(total, 1);
                Metrics.RamUsedPercent = (float)Math.Round((used / total) * 100, 1);
                Info.RamTotal = $"{Math.Round(total)} GB";
            }
        }
    }

    public class HardwareInfo
    {
        public string Motherboard { get; set; } = "Unknown";
        public string Cpu { get; set; } = "Unknown";
        public string Gpu { get; set; } = "Unknown";
        public string RamTotal { get; set; } = "Unknown";
    }
}