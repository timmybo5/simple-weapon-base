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

	public void OnDamage( in DamageInfo info )
	{
		if ( info is not Shared.DamageInfo )
		{
			Log.Warning( "PlayerBase: OnDamage called with non-Shared.DamageInfo. Ignoring." );
			return;
		}
		info.Shape = null; // Remove physics shape to avoid issues with networking
		info.Hitbox = null; // Remove hitbox to avoid issues with networking
		TakeDamage( info as Shared.DamageInfo );
	}

	[Rpc.Broadcast]
	public virtual void TakeDamage( Shared.DamageInfo info )
	{
		if ( !IsValid || IsProxy || !IsAlive || GodMode )
			return;

		if ( info.Tags.Has("head" ) )
			info.Damage *= 2;

		Health -= (int)info.Damage;

		// Flinch
		var weapon = WeaponRegistry.Instance.Get( info.Inflictor );
		if ( weapon is not null )
			DoHitFlinch( weapon.Primary.HitFlinch );

		if ( Health <= 0 )
			OnDeath( info );
	}
}
