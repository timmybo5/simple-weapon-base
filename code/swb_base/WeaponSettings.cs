﻿namespace SWB.Base;

/*
 * Attach this component somewhere in the root of your scene.
 * Gives control over weapon settings (host only)
 */

[Group( "SWB" )]
[Title( "Weapon Settings" )]
public class WeaponSettings : Component
{
	/// <summary>Enable the weapon customization menu (Q)</summary>
	[Sync( SyncFlags.FromHost ), Property] public bool Customization { get; set; } = true;

	/// <summary>Reload weapons automatically when trying to shoot if clip is empty</summary>
	[Sync( SyncFlags.FromHost ), Property] public bool AutoReload { get; set; } = true;

	protected override void OnAwake()
	{
		GameObject.NetworkMode = NetworkMode.Object;
	}

	static public WeaponSettings Instance
	{
		get
		{
			return Game.ActiveScene.Components.GetInChildren<WeaponSettings>();
		}
	}
}
