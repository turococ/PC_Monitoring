namespace HardwareMonitor.Hardware
{
    public class HardwareMetrics
    {
        public float? CpuLoad { get; set; }
        public float? CpuTemp { get; set; }
        public float? GpuLoad { get; set; }
        public float? GpuTemp { get; set; }
        public float? RamUsedPercent { get; set; }
        public float? RamUsedGB { get; set; }
        public float? RamTotalGB { get; set; }
    }
    public class HardwareInfo
    {
        public string Motherboard { get; set; } = "Unknown";
        public string Cpu { get; set; } = "Unknown";
        public string Gpu { get; set; } = "Unknown";
        public string RamTotal { get; set; } = "Unknown";
    }
}