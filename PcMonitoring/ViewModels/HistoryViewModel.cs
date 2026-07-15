using PcMonitoring.Data;
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
                    _ = LoadDataAsync();
                }
            }
        }

        public List<string> Periods { get; } = new() { "1h", "24h", "7d", "30d" };

        public ObservableCollection<MetricRecord> RecentRecords { get; } = new();

        public HistoryViewModel(MetricsDatabase database)
        {
            _database = database;
            _ = LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            var timeRange = SelectedPeriod switch
            {
                "1h" => TimeSpan.FromHours(1),
                "24h" => TimeSpan.FromHours(24),
                "7d" => TimeSpan.FromDays(7),
                "30d" => TimeSpan.FromDays(30),
                _ => TimeSpan.FromHours(1)
            };

            try
            {
                var DataPackage = await Task.Run(() =>
                {
                    var metrics = _database.GetMetrics(timeRange);
                    if (metrics.Count == 0)
                        return null;

                    return new
                    {
                        MetricsCount = metrics.Count,
                        CpuLoad = _database.GetStats("CpuLoad", timeRange),
                        CpuTemp = _database.GetStats("CpuTemp", timeRange),
                        GpuTemp = _database.GetStats("GpuTemp", timeRange),
                        Ram = _database.GetStats("RamUsedPercent", timeRange),
                        Recent = _database.GetRecentMetrics(20)
                    };
                });

                if (DataPackage == null)
                {
                    SetEmptyStats("нет данных");
                    return;
                }

                CpuLoadStats = FormatStats(DataPackage.CpuLoad, "%");
                CpuTempStats = FormatStats(DataPackage.CpuTemp, "°C");
                GpuTempStats = FormatStats(DataPackage.GpuTemp, "°C");
                RamStats = FormatStats(DataPackage.Ram, "%");

                RecentRecords.Clear();
                foreach (var m in DataPackage.Recent)
                    RecentRecords.Add(m);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB Error] Ошибка загрузки истории: {ex.Message}");
                SetEmptyStats("Ошибка БД");
            }
        }

        private void SetEmptyStats(string message)
        {
            CpuLoadStats = message;
            CpuTempStats = message;
            GpuTempStats = message;
            RamStats = message;
            RecentRecords.Clear();
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
