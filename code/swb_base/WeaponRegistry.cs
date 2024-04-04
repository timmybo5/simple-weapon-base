using System.Collections.Generic;

namespace SWB.Base;

/*
 * Attach this component somewhere in the root of your scene.
 * Register all weapons that you want to use in here
 */

[Group( "SWB" )]
[Title( "WeaponRegistry" )]
public class WeaponRegistry : Component
{
	[Property] public List<PrefabScene> WeaponPrefabs { get; set; } = new();
	public Dictionary<string, GameObject> Weapons { get; set; } = new();

	protected override void OnAwake()
	{
		WeaponPrefabs.ForEach( weaponPrefab =>
		{
			var weaponGO = weaponPrefab.Clone();
			weaponGO.SetParent( this.GameObject );
			weaponGO.Enabled = false;

			var weapon = weaponGO.Components.Get<Weapon>( true );
			Weapons.TryAdd( weapon.ClassName, weaponGO );

			weaponGO.Name = weapon.ClassName;
		} );
	}

	public GameObject Get( string className )
	{
		if ( className is null ) return null;

		Weapons.TryGetValue( className, out var weaponGO );
		return weaponGO;
	}

	public Weapon GetWeapon( string className )
	{
		var weaponGO = Get( className );
		if ( weaponGO is null ) return null;

		return weaponGO.Components.Get<Weapon>( true );
	}
}
