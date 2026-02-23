using LibreHardwareMonitor.Hardware;

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
}