using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PcMonitoring.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private const string SettingsFileName = "settings.json";

        private int _cpuCriticalTemp = 90;
        public int CpuCriticalTemp
        {
            get => _cpuCriticalTemp;
            set
            {
                if (_cpuCriticalTemp != value)
                {
                    _cpuCriticalTemp = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        private int _gpuCriticalTemp = 85;
        public int GpuCriticalTemp
        {
            get => _gpuCriticalTemp;
            set
            {
                if (_gpuCriticalTemp != value)
                {
                    _gpuCriticalTemp = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsViewModel()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    if (settings != null)
                    {
                        _cpuCriticalTemp = settings.CpuCriticalTemp;
                        _gpuCriticalTemp = settings.GpuCriticalTemp;
                    }
                }
            }
            catch
            {
                // Если ошибка загрузки — используем значения по умолчанию
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new SettingsData
                {
                    CpuCriticalTemp = _cpuCriticalTemp,
                    GpuCriticalTemp = _gpuCriticalTemp
                };
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch
            {
                // Если ошибка сохранения — просто игнорируем
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SettingsData
    {
        public int CpuCriticalTemp { get; set; }
        public int GpuCriticalTemp { get; set; }
    }
}
