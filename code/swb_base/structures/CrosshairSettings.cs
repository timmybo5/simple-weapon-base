namespace SWB.Base;
public class CrosshairSettings
{
	[Property] public bool Enabled { get; set; } = true;
	[Property] public bool ShowDot { get; set; } = true;
	[Property] public bool ShowTop { get; set; } = true;
	[Property] public bool ShowBottom { get; set; } = true;
	[Property] public bool ShowLeft { get; set; } = true;
	[Property] public bool ShowRight { get; set; } = true;
}
