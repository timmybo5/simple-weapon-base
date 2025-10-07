using SWB.Base;
using SWB.HUD;
using SWB.Player;
using SWB.Shared;
using System.Linq;

namespace SWB.Demo;

[Group( "SWB" )]
[Title( "Demo Player" )]
public class DemoPlayer : PlayerBase
{
	TimeSince timeSincePerspectiveSwitch;

	void GiveWeapon( string className, bool setActive = false )
	{
		var weapon = WeaponRegistry.Instance.Get( className );

		if ( weapon is null )
		{
			Log.Error( $"[SWB Demo] {className} not found in WeaponRegistry!" );
			return;
		}

		Inventory.AddClone( weapon.GameObject, setActive );
		SetAmmo( weapon.Primary.AmmoType, 360 );
	}

	Weapon GetWeapon( string className )
	{
		var weaponGO = Inventory.Items.First( x => x.Name == className );
		if ( weaponGO is not null )
			return weaponGO.Components.Get<Weapon>();

		return null;
	}

	public override void Respawn()
	{
		base.Respawn();

		if ( IsBot ) return;

		// Give weapons
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
		if ( localPly is null ) return;

		var display = localPly.RootDisplay as RootDisplay;
		display.AddToKillFeed( info.Attacker.Id, GameObject.Id, info.Inflictor );

		// Leaderboards
		if ( IsProxy && !IsBot && localPly.GameObject.Id == info.Attacker.Id )
			Sandbox.Services.Stats.Increment( "kills", 1 );

		if ( !IsProxy && !IsBot )
			Sandbox.Services.Stats.Increment( "deaths", 1 );
	}

	public override void TakeDamage( Shared.DamageInfo info )
	{
		base.TakeDamage( info );

		// Attacker only
		var localPly = PlayerBase.GetLocal();
		if ( localPly is null || !localPly.IsAlive || localPly.GameObject.Id != info.Attacker.Id ) return;

		var display = localPly.RootDisplay as RootDisplay;
		display.CreateHitmarker( Health <= 0 );
		Sound.Play( "hitmarker" );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy ) return;

		// Customization
		if ( Input.Pressed( InputButtonHelper.View ) && timeSincePerspectiveSwitch > 0.5 )
		{
			var localPly = PlayerBase.GetLocal();
			if ( localPly is null || !localPly.IsAlive ) return;

			var activeWep = localPly.Inventory.Active.GetComponent<Weapon>();
			if ( !activeWep.IsScoping && !activeWep.IsAiming )
			{
				ConsoleSystem.Run( "thirdperson" );
				timeSincePerspectiveSwitch = 0;
			}
		}
	}
}
