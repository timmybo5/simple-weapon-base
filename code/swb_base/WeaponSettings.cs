namespace SWB.Base;

/// <summary>
/// Attach this component somewhere in the root of your scene.
/// Gives control over weapon settings (host only)
/// </summary>
[Group( "SWB" )]
[Title( "Weapon Settings" )]
public class WeaponSettings : Component
{
	public static WeaponSettings Instance { get; private set; }

	/// <summary>Enable the weapon customization menu (Q)</summary>
	[Sync( SyncFlags.FromHost ), Property] public bool Customization { get; set; } = true;

	/// <summary>Reload weapons automatically when trying to shoot if clip is empty</summary>
	[Sync( SyncFlags.FromHost ), Property] public bool AutoReload { get; set; } = true;

	/// <summary>Enable controller aim assist</summary>
	[Sync( SyncFlags.FromHost ), Property] public bool AimAssist { get; set; } = true;

	protected override void OnDestroy()
	{
		if ( Instance == this )
			Instance = null;
	}

	protected override void OnAwake()
	{
		Instance = this;
		GameObject.NetworkMode = NetworkMode.Object;
	}
}
