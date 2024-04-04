using SWB.Base;
using SWB.HUD;
using SWB.Player;

namespace SWB.Demo;

[Group( "SWB" )]
[Title( "Demo Player" )]
public class DemoPlayer : PlayerBase
{
	void GiveWeapon( string className, bool setActive = false )
	{
		var weaponRegistery = Scene.Components.GetInChildren<WeaponRegistry>();
		var weaponGO = weaponRegistery.Get( className );
		var weapon = weaponGO.Components.Get<Weapon>( true );
		Inventory.AddClone( weaponGO, setActive );
		SetAmmo( weapon.Primary.AmmoType, 360 );
	}

	public override void Respawn()
	{
		base.Respawn();

		if ( IsBot ) return;

		// Give weapon
		GiveWeapon( "swb_colt" );
		GiveWeapon( "swb_revolver" );
		GiveWeapon( "swb_remington" );
		GiveWeapon( "swb_veresk" );
		GiveWeapon( "swb_scarh", true );
		GiveWeapon( "swb_l96a1" );
	}

	public override void OnDeath( Shared.DamageInfo info )
	{
		base.OnDeath( info );

		var localPly = PlayerBase.GetLocal();
		var display = localPly.RootDisplay as RootDisplay;
		display.AddToKillFeed( info.AttackerId, GameObject.Id, info.Inflictor );
	}
}
