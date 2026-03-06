using System.Windows;
using System.Windows.Input;
using PcMonitoring.ViewModels;
using System.Windows.Threading;

namespace PcMonitoring
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
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

        private void Monitoring_Click(object sender, RoutedEventArgs e) => ShowMonitoring();

        private void Specs_Click(object sender, RoutedEventArgs e) => ShowSpecs();

        private void Settings_Click(object sender, RoutedEventArgs e) =>
            MessageBox.Show("в разработке", "PC Monitoring PRO",
                           MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void HideAllScreens()
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            MonitoringScreen.Visibility = Visibility.Collapsed;
            SpecsScreen.Visibility = Visibility.Collapsed;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}