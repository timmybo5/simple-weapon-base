using SWB.Shared;
using System.Collections.Generic;

namespace SWB.Base;

/*
 * Attach this component somewhere in the root of your scene.
 * Register all weapons that you want to use in here
 */
[Group( "SWB" )]
[Title( "Weapon Registry" )]
public class WeaponRegistry : Component
{
	[Property] public List<PrefabScene> WeaponPrefabs { get; set; } = new();
	public Dictionary<string, Weapon> Weapons { get; set; } = new();

	static public WeaponRegistry Instance
	{
		get
		{
			return Game.ActiveScene.Components.GetInChildren<WeaponRegistry>();
		}
	}

	protected override void OnAwake()
	{
		WeaponPrefabs.ForEach( weaponPrefab =>
		{
			CloneConfig config = new()
			{
				StartEnabled = false
			};

			var weaponGO = weaponPrefab.Clone( config );
			weaponGO.SetParent( this.GameObject );

			// Makes it so we can clone this with additional components added at runtime
			weaponGO.BreakFromPrefab();

			var weapon = weaponGO.Components.Get<Weapon>( true );
			Weapons.TryAdd( weapon.ClassName, weapon );

			weaponGO.Name = weapon.ClassName;
		} );

		// Lights
		MapUtil.TagLights();
	}

	public Weapon Get( string className )
	{
		if ( className is null ) return null;

		Weapons.TryGetValue( className, out var weapon );
		return weapon;
	}
}
