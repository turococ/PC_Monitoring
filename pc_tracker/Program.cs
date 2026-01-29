using System;
using System.Diagnostics;
using Spectre.Console;

var CPU = "intel";
var GPU = "nvidia";
var RAM = "16gb";


Console.Clear();
AnsiConsole.WriteLine();
var PC_Spec = new Panel(
	new Rows(
		new Markup($"[bold] Cpu: {CPU}[/]"),
		new Markup($"[bold] Gpu: {GPU}[/]"),
		new Markup($"[bold] Ram: {RAM}[/]")
		)
	);
PC_Spec.Header = new PanelHeader("[bold] PC Specifications [/]");
PC_Spec.Border = BoxBorder.Rounded;


AnsiConsole.Write(PC_Spec);
AnsiConsole.WriteLine();