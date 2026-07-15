using HardwareMonitor.Hardware;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using PcMonitoring.Data;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace PcMonitoring.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly HardwareReader _reader;
        private readonly DispatcherTimer _timer;
        private readonly MetricsDatabase _database;
        private readonly Action<string, string> _showNotification;

        // Для агрегации метрик (запись в БД раз в 60 секунд)
        private float? _aggCpuLoadSum, _aggCpuTempSum, _aggGpuLoadSum, _aggGpuTempSum, _aggRamSum;
        private int _aggCount;
        private readonly object _aggLock = new();

        public MetricsDatabase Database => _database;

        private int _cpuCriticalTemp = 90;
        public int CpuCriticalTemp
        {
            get => _cpuCriticalTemp;
            set { _cpuCriticalTemp = value; }
        }

        private int _gpuCriticalTemp = 85;
        public int GpuCriticalTemp
        {
            get => _gpuCriticalTemp;
            set { _gpuCriticalTemp = value; }
        }

        private bool _cpuAlertShown;
        private bool _gpuAlertShown;

        public string Cpu { get; }
        public string Gpu { get; }
        public string Motherboard { get; }
        public string Ram { get; }
        public IReadOnlyList<string> Disks { get; }

        private float? _cpuLoad;
        public float? CpuLoad
        {
            get => _cpuLoad;
            set { _cpuLoad = value; OnPropertyChanged(); UpdateCpuSeries(); }
        }

        private float? _cpuTemp;
        public float? CpuTemp
        {
            get => _cpuTemp;
            set { _cpuTemp = value; OnPropertyChanged(); UpdateCpuTempSeries(); }
        }

        private float? _gpuLoad;
        public float? GpuLoad
        {
            get => _gpuLoad;
            set { _gpuLoad = value; OnPropertyChanged(); UpdateGpuSeries(); }
        }

        private float? _gpuTemp;
        public float? GpuTemp
        {
            get => _gpuTemp;
            set { _gpuTemp = value; OnPropertyChanged(); UpdateGpuTempSeries(); }
        }

        private float? _ramUsedPercent;
        public float? RamUsedPercent
        {
            get => _ramUsedPercent;
            set { _ramUsedPercent = value; OnPropertyChanged(); UpdateRamSeries(); }
        }

        public string? CpuLoadText { get; set; }
        public string? CpuTempText { get; set; }
        public string? GpuLoadText { get; set; }
        public string? GpuTempText { get; set; }
        public string? RamUsedPercentText { get; set; }

        // Серии
        public ObservableCollection<ISeries> CpuSeries { get; set; }
        public ObservableCollection<ISeries> CpuTempSeries { get; set; }
        public ObservableCollection<ISeries> GpuSeries { get; set; }
        public ObservableCollection<ISeries> GpuTempSeries { get; set; }
        public ObservableCollection<ISeries> RamSeries { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(Action<string, string> showNotification)
        {
            _showNotification = showNotification;
            _reader = new HardwareReader();
            _database = new MetricsDatabase();

            var specs = _reader.ReadPcSpec();
            Cpu = specs.Cpu;
            Gpu = specs.Gpu;
            Motherboard = specs.Motherboard;
            Ram = specs.RamTotal;
            Disks = specs.Disks;

            CpuSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(),
                    Name = "CPU Load",
                    Fill = new SolidColorPaint(SKColors.Red.WithAlpha(100)),
                    Stroke = new SolidColorPaint(SKColors.Red, 2),
                    GeometryFill = null,
                    GeometryStroke = null,
                    GeometrySize = 0,
                    LineSmoothness = 0f
                }
            };

            CpuTempSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(),
                    Name = "CPU Temp",
                    Fill = new SolidColorPaint(SKColors.Cyan.WithAlpha(100)),
                    Stroke = new SolidColorPaint(SKColors.Cyan, 2),
                    GeometryFill = null,
                    GeometryStroke = null,
                    GeometrySize = 0,
                    LineSmoothness = 0f
                }
            };

            GpuSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(),
                    Name = "GPU Load",
                    Fill = new SolidColorPaint(SKColors.Green.WithAlpha(100)),
                    Stroke = new SolidColorPaint(SKColors.Green, 2),
                    GeometryFill = null,
                    GeometryStroke = null,
                    GeometrySize = 0,
                    LineSmoothness = 0f
                }
            };

            GpuTempSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(),
                    Name = "GPU Temp",
                    Fill = new SolidColorPaint(SKColors.Orange.WithAlpha(100)),
                    Stroke = new SolidColorPaint(SKColors.Orange, 2),
                    GeometryFill = null,
                    GeometryStroke = null,
                    GeometrySize = 0,
                    LineSmoothness = 0f
                }
            };

            RamSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(),
                    Name = "RAM Usage",
                    Fill = new SolidColorPaint(SKColors.White.WithAlpha(100)),
                    Stroke = new SolidColorPaint(SKColors.White, 2),
                    GeometryFill = null,
                    GeometryStroke = null,
                    GeometrySize = 0,
                    LineSmoothness = 0f
                }
            };

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateMetrics();
            _timer.Start();

            // Таймер для записи агрегированных метрик в БД каждые 60 секунд
            var dbTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            dbTimer.Tick += (s, e) => FlushAggregatedMetrics();
            dbTimer.Start();
        }

        private void FlushAggregatedMetrics()
        {
            lock (_aggLock)
            {
                if (_aggCount == 0) return;

                var avgCpuLoad = _aggCpuLoadSum / _aggCount;
                var avgCpuTemp = _aggCpuTempSum / _aggCount;
                var avgGpuLoad = _aggGpuLoadSum / _aggCount;
                var avgGpuTemp = _aggGpuTempSum / _aggCount;
                var avgRam = _aggRamSum / _aggCount;

                _database.InsertMetric(
                    _aggCpuLoadSum > 0 ? (float?)avgCpuLoad : null,
                    _aggCpuTempSum > 0 ? (float?)avgCpuTemp : null,
                    _aggGpuLoadSum > 0 ? (float?)avgGpuLoad : null,
                    _aggGpuTempSum > 0 ? (float?)avgGpuTemp : null,
                    _aggRamSum > 0 ? (float?)avgRam : null);

                _aggCpuLoadSum = 0;
                _aggCpuTempSum = 0;
                _aggGpuLoadSum = 0;
                _aggGpuTempSum = 0;
                _aggRamSum = 0;
                _aggCount = 0;
            }
        }

        private void UpdateMetrics()
        {
            var m = _reader.GetCurrentMetrics();

            CpuLoad = m.CpuLoad;
            CpuTemp = m.CpuTemp;
            GpuLoad = m.GpuLoad;
            GpuTemp = m.GpuTemp;
            RamUsedPercent = m.RamUsedPercent;

            // Агрегация для записи в БД
            lock (_aggLock)
            {
                _aggCpuLoadSum = (_aggCpuLoadSum ?? 0) + (m.CpuLoad ?? 0);
                _aggCpuTempSum = (_aggCpuTempSum ?? 0) + (m.CpuTemp ?? 0);
                _aggGpuLoadSum = (_aggGpuLoadSum ?? 0) + (m.GpuLoad ?? 0);
                _aggGpuTempSum = (_aggGpuTempSum ?? 0) + (m.GpuTemp ?? 0);
                _aggRamSum = (_aggRamSum ?? 0) + (m.RamUsedPercent ?? 0);
                _aggCount++;
            }

            CpuLoadText = FormatPercent(CpuLoad);
            CpuTempText = FormatTemp(CpuTemp);
            GpuLoadText = FormatPercent(GpuLoad);
            GpuTempText = FormatTemp(GpuTemp);
            RamUsedPercentText = FormatPercent(RamUsedPercent);
            
            OnPropertyChanged(nameof(CpuLoadText));
            OnPropertyChanged(nameof(CpuTempText));
            OnPropertyChanged(nameof(GpuLoadText));
            OnPropertyChanged(nameof(GpuTempText));
            OnPropertyChanged(nameof(RamUsedPercentText));

            CheckCriticalTemperatures();
        }

        private void CheckCriticalTemperatures()
        {
            if (CpuTemp.HasValue && CpuTemp.Value >= _cpuCriticalTemp && !_cpuAlertShown)
            {
                _cpuAlertShown = true;
                _showNotification?.Invoke(
                    "⚠️ Перегрев CPU",
                    $"Температура CPU достигла {CpuTemp.Value:0}°C (порог: {_cpuCriticalTemp}°C)! Проверьте систему охлаждения.");
            }
            else if (CpuTemp.HasValue && CpuTemp.Value < _cpuCriticalTemp - 5)
            {
                _cpuAlertShown = false;
            }

            if (GpuTemp.HasValue && GpuTemp.Value >= _gpuCriticalTemp && !_gpuAlertShown)
            {
                _gpuAlertShown = true;
                _showNotification?.Invoke(
                    "⚠️ Перегрев GPU",
                    $"Температура GPU достигла {GpuTemp.Value:0}°C (порог: {_gpuCriticalTemp}°C)! Проверьте систему охлаждения.");
            }
            else if (GpuTemp.HasValue && GpuTemp.Value < _gpuCriticalTemp - 5)
            {
                _gpuAlertShown = false;
            }
        }

        public void TestNotification()
        {
            _showNotification?.Invoke(
                "🔔 Тест уведомления",
                "Уведомления работают корректно!");
        }

        private static string? FormatPercent(float? value) =>
        value?.ToString("0") + " %";

        private static string? FormatTemp(float? value) =>
        value?.ToString("0") + " °C";

        private void UpdateCpuSeries() => AppendAndUpdate(CpuSeries[0], CpuLoad, 20);
        private void UpdateCpuTempSeries() => AppendAndUpdate(CpuTempSeries[0], CpuTemp, 20);
        private void UpdateGpuSeries() => AppendAndUpdate(GpuSeries[0], GpuLoad, 20);
        private void UpdateGpuTempSeries() => AppendAndUpdate(GpuTempSeries[0], GpuTemp, 20);
        private void UpdateRamSeries() => AppendAndUpdate(RamSeries[0], RamUsedPercent, 20);

        private static void AppendAndUpdate(ISeries series, float? value, int maxPoints)
        {
            if (series is LineSeries<double> ls && ls.Values is ObservableCollection<double> values)
            {
                if (value.HasValue) values.Add(value.Value);
                if (values.Count > maxPoints) values.RemoveAt(0);
            }
        }

        public void UpdateCriticalTempThresholds(int cpuTemp, int gpuTemp)
        {
            _cpuCriticalTemp = cpuTemp;
            _gpuCriticalTemp = gpuTemp;
        }

        public void Stop()
        {
            FlushAggregatedMetrics();
            _reader?.Dispose();
            _database?.Dispose();
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}