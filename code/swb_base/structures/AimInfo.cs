namespace SWB.Base;


/// <summary>
/// Describes effects of aiming with a weapon
/// Any value set to -1 implies "don't modify" or "use default"
/// </summary>
public class AimInfo
{
	/// <summary>
	/// Spread modifier to apply when Aiming Down Sights (ADS).
	/// </summary>
	[Property] public float SpreadModifier { get; set; } = 0.25f;

	/// <summary>
	/// Input sensitivity multiplier when aiming down sights (ADS). (lower is slower, 0 to disable)
	/// </summary>
	[Property] public float Sensitivity { get; set; } = 0.85f;

	/// <summary>
	/// FOV For the view model when aiming down sights.
	/// </summary>
	[Property, Group( "FOV" )] public float ViewModelFOV { get; set; } = -1f;

	/// <summary>
	/// FOV for the player camera when aiming down sights.
	/// </summary>
	[Property, Group( "FOV" )] public float PlayerFOV { get; set; } = -1f;

	/// <summary>
	/// FOV aim in speed
	/// </summary>
	[Property, Group( "FOV" ), Title( "Aim in FOV speed" )] public float AimInFOVSpeed { get; set; } = 1f;

	/// <summary>
	/// FOV aim out speed
	/// </summary>
	[Property, Group( "FOV" ), Title( "Aim out FOV speed" )] public float AimOutFOVSpeed { get; set; } = 1f;

	public AimInfo Clone()
	{
		// Can't use MemberwiseClone because of whitelist restrictions
		return new AimInfo()
		{
			SpreadModifier = SpreadModifier,
			Sensitivity = Sensitivity,
			ViewModelFOV = ViewModelFOV,
			PlayerFOV = PlayerFOV,
			AimInFOVSpeed = AimInFOVSpeed,
			AimOutFOVSpeed = AimOutFOVSpeed
		};
	}

	// Utility to fill unset values from a default AimInfo
	public void FillDefaults( AimInfo defaults )
	{
		if ( Sensitivity == -1 ) Sensitivity = defaults.Sensitivity;
		if ( ViewModelFOV == -1 ) ViewModelFOV = defaults.ViewModelFOV;
		if ( PlayerFOV == -1 ) PlayerFOV = defaults.PlayerFOV;
		if ( AimInFOVSpeed == -1 ) AimInFOVSpeed = defaults.AimInFOVSpeed;
		if ( AimOutFOVSpeed == -1 ) AimOutFOVSpeed = defaults.AimOutFOVSpeed;
	}
}
