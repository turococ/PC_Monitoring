using System.Windows;
using System.Windows.Input;
using PcMonitoring.ViewModel;
using System.Windows.Threading;

namespace PcMonitoring
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private SettingsViewModel? _settingsViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _settingsViewModel = new SettingsViewModel();
            DataContext = _viewModel;
            ShowWelcome();
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel?.Stop();
            base.OnClosed(e);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Monitoring_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.UpdateCriticalTempThresholds(
                _settingsViewModel?.CpuCriticalTemp ?? 90,
                _settingsViewModel?.GpuCriticalTemp ?? 85);
            DataContext = _viewModel;
            ShowMonitoring();
        }

        private void Specs_Click(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;
            ShowSpecs();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            DataContext = _settingsViewModel;
            ShowSettings();
        }

        private void ShowWelcome()
        {
            HideAllScreens();
            WelcomeScreen.Visibility = Visibility.Visible;
        }

        private void ShowMonitoring()
        {
            HideAllScreens();
            MonitoringScreen.Visibility = Visibility.Visible;
        }

        private void ShowSpecs()
        {
            HideAllScreens();
            SpecsScreen.Visibility = Visibility.Visible;
        }

        private void ShowSettings()
        {
            HideAllScreens();
            SettingsScreen.Visibility = Visibility.Visible;
        }

        private void HideAllScreens()
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            MonitoringScreen.Visibility = Visibility.Collapsed;
            SpecsScreen.Visibility = Visibility.Collapsed;
            SettingsScreen.Visibility = Visibility.Collapsed;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}