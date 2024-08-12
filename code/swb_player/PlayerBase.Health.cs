
using SWB.Base;
using System;

namespace SWB.Player;

public partial class PlayerBase
{
	[Property] public int MaxHealth { get; set; } = 100;
	[Sync] public int Health { get; set; } = 100;
	[Sync] public int Kills { get; set; }
	[Sync] public int Deaths { get; set; }
	[Sync] public bool GodMode { get; set; }

	public bool IsAlive => Health > 0;

	[Broadcast]
	public virtual void TakeDamage( Shared.DamageInfo info )
	{
		if ( !IsValid || IsProxy || !IsAlive || GodMode )
			return;

		if ( Array.Exists( info.Tags, tag => tag == "head" ) )
			info.Damage *= 2;

		Health -= (int)info.Damage;

		// Flinch
		var weapon = WeaponRegistry.Instance.GetWeapon( info.Inflictor );
		if ( weapon is not null )
			DoHitFlinch( weapon.Primary.HitFlinch );

		if ( Health <= 0 )
			OnDeath( info );
	}
}
