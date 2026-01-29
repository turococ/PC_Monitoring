using LibreHardwareMonitor.Hardware;
using Spectre.Console;
using System.Runtime.Intrinsics.Arm;

var CPU = "Unknown";
var GPU = "Unknown";
var RAM = "Unknown";


Console.Clear();

var computer = new Computer
{
    IsCpuEnabled = true,
    IsGpuEnabled = true,
    IsMemoryEnabled = true
};

computer.Open();

foreach (var hardware in computer.Hardware)
{
    hardware.Update();

    switch (hardware.HardwareType)
    {
        case HardwareType.Cpu:
            CPU = hardware.Name;
            break;

        case HardwareType.GpuNvidia:
        case HardwareType.GpuAmd:
        case HardwareType.GpuIntel:
            GPU = hardware.Name;
            break;
    }
}
foreach (var hardware in computer.Hardware)
{
    if (hardware.HardwareType == HardwareType.Memory)
    {
        hardware.Update();

        float used = 0;
        float available = 0;

        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Data)
            {
                if (sensor.Name == "Memory Used" && sensor.Value.HasValue)
                    used = sensor.Value.Value;

                if (sensor.Name == "Memory Available" && sensor.Value.HasValue)
                    available = sensor.Value.Value;
            }
        }

        if (used > 0 || available > 0)
        {
            RAM = $"{used + available:0.#} GB";
        }
    }
}

AnsiConsole.WriteLine();
var PC_Spec = new Panel(
	new Rows(
		new Markup($"[bold] Cpu: {CPU}[/]"),
		new Markup($"[bold] Gpu: {GPU}[/]"),
		new Markup($"[bold] Ram: {RAM}[/]")
		)
	)
	.Header("[bold] Your PC [/]")
	.Border(BoxBorder.Heavy);

AnsiConsole.Write(PC_Spec);
AnsiConsole.WriteLine();
Console.ReadKey();
