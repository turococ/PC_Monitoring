using HardwareMonitor.Hardware;
using HardwareMonitor.UI;

Console.Clear();

var reader = new HardwareReader();
var specs = reader.ReadPcSpec();

PcInfoRenderer.Render(specs);

Console.ReadKey();