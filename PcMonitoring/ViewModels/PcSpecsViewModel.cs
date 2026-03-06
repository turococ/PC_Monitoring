using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using HardwareMonitor.Hardware;

namespace PcMonitoring.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly HardwareReader _reader;
        private readonly DispatcherTimer _timer;

        public string Cpu { get; }
        public string Gpu { get; }
        public string Motherboard { get; }
        public string Ram { get; }
        public IReadOnlyList<string> Disks { get; }

        private float? _cpuLoad;
        public float? CpuLoad
        {
            get => _cpuLoad;
            set { _cpuLoad = value; OnPropertyChanged(); }
        }

        private float? _cpuTemp;
        public float? CpuTemp
        {
            get => _cpuTemp;
            set { _cpuTemp = value; OnPropertyChanged(); }
        }

        private float? _gpuLoad;
        public float? GpuLoad
        {
            get => _gpuLoad;
            set { _gpuLoad = value; OnPropertyChanged(); }
        }

        private float? _gpuTemp;
        public float? GpuTemp
        {
            get => _gpuTemp;
            set { _gpuTemp = value; OnPropertyChanged(); }
        }

        private float? _ramUsedPercent;
        public float? RamUsedPercent
        {
            get => _ramUsedPercent;
            set { _ramUsedPercent = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _reader = new HardwareReader();

            var specs = _reader.ReadPcSpec();
            Cpu = specs.Cpu;
            Gpu = specs.Gpu;
            Motherboard = specs.Motherboard;
            Ram = specs.RamTotal;
            Disks = specs.Disks;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += (s, e) => UpdateMetrics();
            _timer.Start();
        }

        private void UpdateMetrics()
        {
            var m = _reader.GetCurrentMetrics();

            System.Diagnostics.Debug.WriteLine(
                $"CPU: {m.CpuLoad?.ToString() ?? "null"}% / {m.CpuTemp?.ToString() ?? "null"}°C | " +
                $"GPU: {m.GpuLoad?.ToString() ?? "null"}% / {m.GpuTemp?.ToString() ?? "null"}°C");


            CpuLoad = m.CpuLoad;
            CpuTemp = m.CpuTemp;
            GpuLoad = m.GpuLoad;
            GpuTemp = m.GpuTemp;
            RamUsedPercent = m.RamUsedPercent;
        }

        public void Stop() => _reader?.Dispose();

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}