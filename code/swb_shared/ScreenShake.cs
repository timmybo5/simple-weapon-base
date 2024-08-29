namespace SWB.Shared;

public class ScreenShake
{
	/// <summary>Duration (s)</summary>
	[KeyProperty] public float Duration { get; set; } = 0f;

	/// <summary>Delay between shakes (s)</summary>
	[KeyProperty] public float Delay { get; set; } = 0f;

	/// <summary>Screen disposition amount</summary>
	[KeyProperty] public float Size { get; set; } = 0f;

	/// <summary>Screen rotation amount</summary>
	[KeyProperty] public float Rotation { get; set; } = 0f;
}
