global using WinForms = System.Windows.Forms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PcMonitoring.ViewModel;

namespace PcMonitoring
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private SettingsViewModel? _settingsViewModel;
        private HistoryViewModel? _historyViewModel;
        private bool _isDarkTheme = true;
        private WinForms.NotifyIcon? _trayIcon;
        private bool _isClosing;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel(ShowWindowsNotification);
            _settingsViewModel = new SettingsViewModel();
            _historyViewModel = new HistoryViewModel(_viewModel.Database);

            if (WelcomeScreen != null) WelcomeScreen.DataContext = _viewModel;
            if (HistoryScreen != null) HistoryScreen.DataContext = _historyViewModel;
            if (SettingsScreen != null) SettingsScreen.DataContext = _viewModel;

            ShowWelcome();
            InitializeTrayIcon();
            SizeChanged += (s, e) => UpdateClip();
            Loaded += (s, e) => UpdateClip();
        }

        private void UpdateClip()
        {
            var w = MainBorder.ActualWidth;
            var h = MainBorder.ActualHeight;
            if (w > 0 && h > 0)
                MainClip.Rect = new Rect(0, 0, w, h);
        }

        private void InitializeTrayIcon()
        {
            _trayIcon = new WinForms.NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                    ?? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PcMonitoring.exe")),
                Visible = true,
                Text = "ПК-Страж"
            };

            _trayIcon.DoubleClick += (s, e) => ShowWindow();

            // Контекстное меню
            var menu = new WinForms.ContextMenuStrip();
            var openItem = menu.Items.Add("Открыть");
            openItem.Click += (s, e) => ShowWindow();
            var exitItem = menu.Items.Add("Выход");
            exitItem.Click += (s, e) => { _isClosing = true; Close(); };
            _trayIcon.ContextMenuStrip = menu;
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ShowWindowsNotification(string title, string message)
        {
            _trayIcon?.ShowBalloonTip(3000, title, message, WinForms.ToolTipIcon.Warning);
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel?.Stop();
            _trayIcon?.Dispose();
            base.OnClosed(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized && !_isClosing)
            {
                Hide();
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
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
            SetActiveButton(MonitoringBtn);
            ShowMonitoring();
        }

        private void Specs_Click(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;
            SetActiveButton(SpecsBtn);
            ShowSpecs();
        }

        private async void History_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(HistoryBtn);
            ShowHistory();

            if (_historyViewModel != null)
            {
                await _historyViewModel.LoadDataAsync();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            DataContext = _settingsViewModel;
            SetActiveButton(SettingsBtn);
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
            var btn = (TextBlock)((Button)sender).Content;
            btn.Text = "🌙";
            FadeTheme(new ThemeColors
            {
                MainBorderBackground = Color.FromArgb(0xAA, 0xF0, 0xF0, 0xF0),
                SidebarBackground = Color.FromRgb(0xE8, 0xE8, 0xE8),
                ContentBackground = Color.FromRgb(0xF5, 0xF5, 0xF5),
                TextPrimary = Color.FromRgb(0x22, 0x22, 0x22),
                TextSecondary = Color.FromRgb(0x66, 0x66, 0x66),
                TextMenuTitle = Color.FromRgb(0x55, 0x55, 0x55),
                SeparatorBackground = Color.FromRgb(0xCC, 0xCC, 0xCC),
                NavButtonHover = Color.FromRgb(0xDD, 0xDD, 0xDD),
                NavButtonPressed = Color.FromRgb(0xCC, 0xCC, 0xCC),
                TextBoxBackground = Color.FromRgb(0xFF, 0xFF, 0xFF),
                TextBoxForeground = Color.FromRgb(0x22, 0x22, 0x22),
                TextBoxBorderBrush = Color.FromRgb(0xBB, 0xBB, 0xBB),
                CaretBrush = Color.FromRgb(0x22, 0x22, 0x22),
                SettingDescription = Color.FromRgb(0x77, 0x77, 0x77),
                SettingLabel = Color.FromRgb(0x44, 0x44, 0x44),
                ScrollBarBackground = Color.FromRgb(0xDD, 0xDD, 0xDD),
                ScrollBarThumb = Color.FromRgb(0xAA, 0xAA, 0xAA),
                ButtonHover = Color.FromRgb(0xDD, 0xDD, 0xDD),
                CloseButtonHover = Color.FromRgb(0xC4, 0x2B, 0x1C),
                ComboBoxPopupBackground = Color.FromRgb(0xF5, 0xF5, 0xF5),
                ComboBoxPopupForeground = Color.FromRgb(0x22, 0x22, 0x22),
                ComboBoxItemHover = Color.FromRgb(0xDD, 0xDD, 0xDD),
            }, Color.FromRgb(0xF5, 0xF5, 0xF5));
        }

        private void SwitchToDarkTheme(object sender, RoutedEventArgs e)
        {
            var btn = (TextBlock)((Button)sender).Content;
            btn.Text = "☀";
            FadeTheme(new ThemeColors
            {
                MainBorderBackground = Color.FromArgb(0xAA, 0x00, 0x00, 0x00),
                SidebarBackground = Color.FromRgb(0x1E, 0x1E, 0x1E),
                ContentBackground = Color.FromRgb(0x2A, 0x2A, 0x2A),
                TextPrimary = Color.FromRgb(0xFF, 0xFF, 0xFF),
                TextSecondary = Color.FromRgb(0xAA, 0xAA, 0xAA),
                TextMenuTitle = Color.FromRgb(0x88, 0x88, 0x88),
                SeparatorBackground = Color.FromRgb(0x44, 0x44, 0x44),
                NavButtonHover = Color.FromRgb(0x3A, 0x3A, 0x3A),
                NavButtonPressed = Color.FromRgb(0x2A, 0x2A, 0x2A),
                TextBoxBackground = Color.FromRgb(0x3A, 0x3A, 0x3A),
                TextBoxForeground = Color.FromRgb(0xFF, 0xFF, 0xFF),
                TextBoxBorderBrush = Color.FromRgb(0x55, 0x55, 0x55),
                CaretBrush = Color.FromRgb(0xFF, 0xFF, 0xFF),
                SettingDescription = Color.FromRgb(0x66, 0x66, 0x66),
                SettingLabel = Color.FromRgb(0xCC, 0xCC, 0xCC),
                ScrollBarBackground = Color.FromRgb(0x33, 0x33, 0x33),
                ScrollBarThumb = Color.FromRgb(0x88, 0x88, 0x88),
                ButtonHover = Color.FromRgb(0x3A, 0x3A, 0x3A),
                CloseButtonHover = Color.FromRgb(0xC4, 0x2B, 0x1C),
                ComboBoxPopupBackground = Color.FromRgb(0x1E, 0x1E, 0x1E),
                ComboBoxPopupForeground = Color.FromRgb(0xFF, 0xFF, 0xFF),
                ComboBoxItemHover = Color.FromRgb(0x3A, 0x3A, 0x3A),
            }, Color.FromRgb(0x1E, 0x1E, 0x1E));
        }

        private async void FadeTheme(ThemeColors colors, Color overlayColor)
        {
            var duration = TimeSpan.FromMilliseconds(400);
            var easing = new CircleEase { EasingMode = EasingMode.EaseOut };

            var maxRadius = Math.Sqrt(Math.Pow(ActualWidth, 2) + Math.Pow(ActualHeight, 2));
            var scale = maxRadius / 5;
            ThemeOverlay.Fill = new SolidColorBrush(overlayColor);
            ThemeOverlay.Opacity = 0;
            ThemeScaleTransform.ScaleX = 1;
            ThemeScaleTransform.ScaleY = 1;

            var scaleXAnim = new DoubleAnimation(1, scale, duration) { EasingFunction = easing };
            var scaleYAnim = new DoubleAnimation(1, scale, duration) { EasingFunction = easing };
            var fadeIn = new DoubleAnimation(0, 1, duration) { EasingFunction = easing };

            ThemeScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            ThemeScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
            ThemeOverlay.BeginAnimation(Ellipse.OpacityProperty, fadeIn);

            // Ждём пока оверлей полностью заполнит экран и переключаем ресурсы
            await Task.Delay(380);
            ApplyTheme(colors);

            // Оверлей плавно исчезает
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = duration,
                EasingFunction = easing
            };
            ThemeOverlay.BeginAnimation(Ellipse.OpacityProperty, fadeOut);
        }

        private void ApplyTheme(ThemeColors colors)
        {
            Resources["MainBorderBackground"] = new SolidColorBrush(colors.MainBorderBackground);
            Resources["SidebarBackground"] = new SolidColorBrush(colors.SidebarBackground);
            Resources["ContentBackground"] = new SolidColorBrush(colors.ContentBackground);
            Resources["TextPrimary"] = new SolidColorBrush(colors.TextPrimary);
            Resources["TextSecondary"] = new SolidColorBrush(colors.TextSecondary);
            Resources["TextMenuTitle"] = new SolidColorBrush(colors.TextMenuTitle);
            Resources["SeparatorBackground"] = new SolidColorBrush(colors.SeparatorBackground);
            Resources["NavButtonHover"] = new SolidColorBrush(colors.NavButtonHover);
            Resources["NavButtonPressed"] = new SolidColorBrush(colors.NavButtonPressed);
            Resources["TextBoxBackground"] = new SolidColorBrush(colors.TextBoxBackground);
            Resources["TextBoxForeground"] = new SolidColorBrush(colors.TextBoxForeground);
            Resources["TextBoxBorderBrush"] = new SolidColorBrush(colors.TextBoxBorderBrush);
            Resources["CaretBrush"] = new SolidColorBrush(colors.CaretBrush);
            Resources["SettingDescription"] = new SolidColorBrush(colors.SettingDescription);
            Resources["SettingLabel"] = new SolidColorBrush(colors.SettingLabel);
            Resources["ScrollBarBackground"] = new SolidColorBrush(colors.ScrollBarBackground);
            Resources["ScrollBarThumb"] = new SolidColorBrush(colors.ScrollBarThumb);
            Resources["ButtonHover"] = new SolidColorBrush(colors.ButtonHover);
            Resources["CloseButtonHover"] = new SolidColorBrush(colors.CloseButtonHover);
            Resources["ComboBoxPopupBackground"] = new SolidColorBrush(colors.ComboBoxPopupBackground);
            Resources["ComboBoxPopupForeground"] = new SolidColorBrush(colors.ComboBoxPopupForeground);
            Resources["ComboBoxItemHover"] = new SolidColorBrush(colors.ComboBoxItemHover);
        }

        private void SetActiveButton(Button? active)
        {
            MonitoringBtn.ClearValue(TagProperty);
            SpecsBtn.ClearValue(TagProperty);
            HistoryBtn.ClearValue(TagProperty);
            SettingsBtn.ClearValue(TagProperty);
            if (active != null)
                active.Tag = "Active";
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

        private void ShowHistory()
        {
            HideAllScreens();
            HistoryScreen.Visibility = Visibility.Visible;
        }

        private void HideAllScreens()
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            MonitoringScreen.Visibility = Visibility.Collapsed;
            SpecsScreen.Visibility = Visibility.Collapsed;
            HistoryScreen.Visibility = Visibility.Collapsed;
            SettingsScreen.Visibility = Visibility.Collapsed;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }

    internal sealed class ThemeColors
    {
        public Color MainBorderBackground { get; set; }
        public Color SidebarBackground { get; set; }
        public Color ContentBackground { get; set; }
        public Color TextPrimary { get; set; }
        public Color TextSecondary { get; set; }
        public Color TextMenuTitle { get; set; }
        public Color SeparatorBackground { get; set; }
        public Color NavButtonHover { get; set; }
        public Color NavButtonPressed { get; set; }
        public Color TextBoxBackground { get; set; }
        public Color TextBoxForeground { get; set; }
        public Color TextBoxBorderBrush { get; set; }
        public Color CaretBrush { get; set; }
        public Color SettingDescription { get; set; }
        public Color SettingLabel { get; set; }
        public Color ScrollBarBackground { get; set; }
        public Color ScrollBarThumb { get; set; }
        public Color ButtonHover { get; set; }
        public Color CloseButtonHover { get; set; }
        public Color ComboBoxPopupBackground { get; set; }
        public Color ComboBoxPopupForeground { get; set; }
        public Color ComboBoxItemHover { get; set; }
    }
}