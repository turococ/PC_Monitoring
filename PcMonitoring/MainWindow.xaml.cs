using HardwareMonitor.Hardware;
using PcMonitoring.UI;
using System.Windows;
using System.Windows.Input;

namespace HardwareMonitor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var reader = new HardwareReader();
        var specs = reader.ReadPcSpec();

        DataContext = new PcSpecsViewModel(specs);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

}