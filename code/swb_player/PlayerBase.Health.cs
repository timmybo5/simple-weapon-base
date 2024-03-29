
using System;

namespace SWB.Player;

public partial class PlayerBase
{
	[Property] public int MaxHealth { get; set; } = 100;
	[Sync] public int Health { get; set; } = 100;

	public bool IsAlive => Health > 0;

	[Broadcast]
	public virtual void TakeDamage( Shared.DamageInfo info )
	{
		if ( IsProxy || !IsAlive )
			return;

		if ( Array.Exists( info.Tags, tag => tag == "head" ) )
			info.Damage *= 2;

		Health -= (int)info.Damage;

		if ( Health <= 0 )
		{
			OnDeath( info );
		}
	}
}
