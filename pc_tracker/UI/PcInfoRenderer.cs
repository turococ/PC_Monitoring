using Spectre.Console;
using HardwareMonitor.Hardware;
using Spectre.Console.Rendering;

namespace HardwareMonitor.UI;

public class PcInfoRenderer
{
    public static void Render(PcSpecs specs)
    {
        var innerPanel = new Panel(
            new Rows(
                new Markup($"[bold]MotherBoard:[/] {specs.MotherBoard}"),
                new Markup($"[bold]CPU:[/] {specs.Cpu}"),
                new Markup($"[bold]GPU:[/] {specs.Gpu}"),
                new Markup($"[bold]RAM:[/] {specs.Ram}"),
                new Markup($"[bold]SSD:[/] {specs.SSD}"),
                new Markup($"[bold]HDD:[/] {specs.HDD}")
            ))
            .Header("[bold] Specs [/]")
            .Border(BoxBorder.Rounded);

        var outerPanel = new Panel(innerPanel)
            .Header("[bold] Your PC [/]")
            .Border(BoxBorder.Heavy);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(outerPanel);
        AnsiConsole.WriteLine();
    }
}