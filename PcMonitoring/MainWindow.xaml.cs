using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PcMonitoring.ViewModel;
using System.Windows.Threading;

namespace PcMonitoring
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private SettingsViewModel? _settingsViewModel;
        private bool _isDarkTheme = true;

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

        private void Theme_Click(object sender, RoutedEventArgs e)
        {
            if (_isDarkTheme)
                SwitchToLightTheme(sender, e);
            else
                SwitchToDarkTheme(sender, e);
            _isDarkTheme = !_isDarkTheme;
        }

        private void SwitchToLightTheme(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var btn = (TextBlock)button.Content;

            Resources["MainBorderBackground"] = new SolidColorBrush(Color.FromArgb(0xAA, 0xF0, 0xF0, 0xF0));
            Resources["SidebarBackground"] = new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xE8));
            Resources["ContentBackground"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
            Resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
            Resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            Resources["TextMenuTitle"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
            Resources["SeparatorBackground"] = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
            Resources["NavButtonHover"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
            Resources["NavButtonPressed"] = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
            Resources["TextBoxBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            Resources["TextBoxForeground"] = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
            Resources["TextBoxBorderBrush"] = new SolidColorBrush(Color.FromRgb(0xBB, 0xBB, 0xBB));
            Resources["CaretBrush"] = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
            Resources["SettingDescription"] = new SolidColorBrush(Color.FromRgb(0x77, 0x77, 0x77));
            Resources["SettingLabel"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
            Resources["ScrollBarBackground"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
            Resources["ScrollBarThumb"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
            Resources["ButtonHover"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
            Resources["CloseButtonHover"] = new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C));

            // Меняем иконку на луну
            btn.Text = "🌙";

            // Анимация: расширяющийся круг через ScaleTransform
            var maxRadius = Math.Sqrt(Math.Pow(ActualWidth, 2) + Math.Pow(ActualHeight, 2));
            var scale = maxRadius / 5; // 5 = половина начального размера (10/2)

            ThemeOverlay.Opacity = 0.3;
            ThemeOverlay.Fill = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
            ThemeScaleTransform.ScaleX = 1;
            ThemeScaleTransform.ScaleY = 1;

            var scaleXAnim = new DoubleAnimation(1, scale, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            var scaleYAnim = new DoubleAnimation(1, scale, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            var opacityAnim = new DoubleAnimation(0.3, 0, TimeSpan.FromMilliseconds(500));

            ThemeScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            ThemeScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
            ThemeOverlay.BeginAnimation(Ellipse.OpacityProperty, opacityAnim);
        }

        private void SwitchToDarkTheme(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var btn = (TextBlock)button.Content;

            Resources["MainBorderBackground"] = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00));
            Resources["SidebarBackground"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));
            Resources["ContentBackground"] = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A));
            Resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            Resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
            Resources["TextMenuTitle"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            Resources["SeparatorBackground"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
            Resources["NavButtonHover"] = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A));
            Resources["NavButtonPressed"] = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A));
            Resources["TextBoxBackground"] = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A));
            Resources["TextBoxForeground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            Resources["TextBoxBorderBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
            Resources["CaretBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            Resources["SettingDescription"] = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            Resources["SettingLabel"] = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
            Resources["ScrollBarBackground"] = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            Resources["ScrollBarThumb"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            Resources["ButtonHover"] = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A));
            Resources["CloseButtonHover"] = new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C));

            // Меняем иконку на солнце
            btn.Text = "☀";

            // Анимация: расширяющийся круг через ScaleTransform
            var maxRadius = Math.Sqrt(Math.Pow(ActualWidth, 2) + Math.Pow(ActualHeight, 2));
            var scale = maxRadius / 5;

            ThemeOverlay.Opacity = 0.3;
            ThemeOverlay.Fill = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));
            ThemeScaleTransform.ScaleX = 1;
            ThemeScaleTransform.ScaleY = 1;

            var scaleXAnim = new DoubleAnimation(1, scale, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            var scaleYAnim = new DoubleAnimation(1, scale, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            var opacityAnim = new DoubleAnimation(0.3, 0, TimeSpan.FromMilliseconds(500));

            ThemeScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            ThemeScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
            ThemeOverlay.BeginAnimation(Ellipse.OpacityProperty, opacityAnim);
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