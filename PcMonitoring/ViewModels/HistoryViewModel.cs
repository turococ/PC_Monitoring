using PcMonitoring.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PcMonitoring.ViewModel
{
    public class HistoryViewModel : INotifyPropertyChanged
    {
        private readonly MetricsDatabase _database;

        private string _cpuLoadStats = "—";
        public string CpuLoadStats { get => _cpuLoadStats; set { _cpuLoadStats = value; OnPropertyChanged(); } }

        private string _cpuTempStats = "—";
        public string CpuTempStats { get => _cpuTempStats; set { _cpuTempStats = value; OnPropertyChanged(); } }

        private string _gpuTempStats = "—";
        public string GpuTempStats { get => _gpuTempStats; set { _gpuTempStats = value; OnPropertyChanged(); } }

        private string _ramStats = "—";
        public string RamStats { get => _ramStats; set { _ramStats = value; OnPropertyChanged(); } }

        private string _selectedPeriod = "1h";
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (_selectedPeriod != value)
                {
                    _selectedPeriod = value;
                    OnPropertyChanged();
                    LoadData();
                }
            }
        }

        public List<string> Periods { get; } = new() { "1h", "24h", "7d", "30d" };

        public ObservableCollection<MetricRecord> RecentRecords { get; } = new();

        public HistoryViewModel(MetricsDatabase database)
        {
            _database = database;
            LoadData();
        }

        public void LoadData()
        {
            var timeRange = SelectedPeriod switch
            {
                "1h" => TimeSpan.FromHours(1),
                "24h" => TimeSpan.FromHours(24),
                "7d" => TimeSpan.FromDays(7),
                "30d" => TimeSpan.FromDays(30),
                _ => TimeSpan.FromHours(1)
            };

            var metrics = _database.GetMetrics(timeRange);
            if (metrics.Count == 0)
            {
                CpuLoadStats = "Нет данных";
                CpuTempStats = "Нет данных";
                GpuTempStats = "Нет данных";
                RamStats = "Нет данных";
                RecentRecords.Clear();
                return;
            }

            var cpuLoad = _database.GetStats("CpuLoad", timeRange);
            CpuLoadStats = FormatStats(cpuLoad, "%");

            var cpuTemp = _database.GetStats("CpuTemp", timeRange);
            CpuTempStats = FormatStats(cpuTemp, "°C");

            var gpuTemp = _database.GetStats("GpuTemp", timeRange);
            GpuTempStats = FormatStats(gpuTemp, "°C");

            var ram = _database.GetStats("RamUsedPercent", timeRange);
            RamStats = FormatStats(ram, "%");

            // Последние 20 записей
            var recent = _database.GetRecentMetrics(20);
            RecentRecords.Clear();
            foreach (var m in recent)
                RecentRecords.Add(m);
        }

        private static string FormatStats((float min, float max, float avg) stats, string unit)
        {
            if (stats.max == 0) return "—";
            return $"Мин: {stats.min:0}{unit} | Макс: {stats.max:0}{unit} | Сред: {stats.avg:0.0}{unit}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
