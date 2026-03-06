using HardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Management;

namespace HardwareMonitor.Hardware
{
    public class HardwareReader : IDisposable
    {
        private readonly Computer _computer;

        public HardwareReader()
        {
            _computer = CreateComputer();
            _computer.Open();
            System.Threading.Thread.Sleep(1000);
        }

        public PcSpecs ReadPcSpec()
        {
            var visitor = new Visitor();
            _computer.Accept(visitor);
            var info = visitor.Info;
            var disks = ReadDisks();
            return new PcSpecs(info.Motherboard, info.Cpu, info.Gpu, info.RamTotal, disks);
        }

        public HardwareMetrics GetCurrentMetrics()
        {
            var visitor = new Visitor();
            _computer.Accept(visitor);
            return visitor.Metrics;
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
                var searcher = new ManagementObjectSearcher("SELECT Model, Size FROM Win32_DiskDrive");

                foreach (ManagementObject disk in searcher.Get())
                {
                    var name = disk["Model"]?.ToString() ?? "Unknown";
                    var size = (ulong)(disk["Size"] ?? 0);
                    var gb = (float)Math.Round(size / 1024.0 / 1024.0 / 1024.0, 1);
                    disks.Add($"{name} — {gb} GB");
                }
            }
            catch
            {
                disks.Add("Не удалось получить информацию о дисках");
            }
            return disks;
        }

        public void Dispose() => _computer?.Close();
    }
}