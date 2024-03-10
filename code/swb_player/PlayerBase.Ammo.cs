using System;

namespace SWB.Player;

public partial class PlayerBase
{
	[Sync] public NetDictionary<string, int> Ammo { get; set; } = new();

	public virtual int AmmoCount( string ammoType )
	{
		if ( Ammo.TryGetValue( ammoType, out var amount ) )
		{
			return amount;
		}

		return 0;
	}

	public virtual void SetAmmo( string ammoType, int amount )
	{
		Ammo[ammoType] = amount;
	}

	public virtual void GiveAmmo( string ammoType, int amount )
	{
		SetAmmo( ammoType, AmmoCount( ammoType ) + amount );
	}

	public virtual int TakeAmmo( string ammoType, int amount )
	{
		var available = AmmoCount( ammoType );
		amount = Math.Min( available, amount );

		SetAmmo( ammoType, available - amount );

		return amount;
	}

	public virtual bool HasAmmo( string ammoType )
	{
		return AmmoCount( ammoType ) > 0;
	}
}
