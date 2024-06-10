namespace SWB.Base;

public class StatsModifier
{
	public float Damage { get; set; }
	public float Recoil { get; set; }
	public float Spread { get; set; }
	public float RPM { get; set; }
	public float Force { get; set; }

	// After phys bullets have been recreated
	//public float BulletVelocity { get; set; } = 0;

	public static readonly StatsModifier Zero = new();

	private bool applied;

	public static StatsModifier FromShootInfo( ShootInfo shootInfo )
	{
		return new()
		{
			Damage = shootInfo.Damage,
			Recoil = shootInfo.Recoil,
			Spread = shootInfo.Spread,
			RPM = shootInfo.RPM,
			Force = shootInfo.Force,
		};
	}

	public void Apply( Weapon weapon, bool onPrimary = true )
	{
		if ( applied ) return;

		if ( onPrimary )
			Apply( weapon.Primary, weapon.InitialPrimaryStats );
		else
			Apply( weapon.Secondary, weapon.InitialSecondaryStats );

		applied = true;
	}

	private void Apply( ShootInfo shootInfo, StatsModifier initialStats )
	{
		if ( shootInfo is null || initialStats is null ) return;

		shootInfo.Damage += initialStats.Damage * Damage;
		shootInfo.Recoil += initialStats.Recoil * Recoil;
		shootInfo.Spread += initialStats.Spread * Spread;
		shootInfo.RPM += (int)(initialStats.RPM * RPM);

		//weapon.BulletVelocityMod += BulletVelocity;
	}

	public void Remove( Weapon weapon, bool onPrimary = true )
	{
		if ( !applied ) return;

		if ( onPrimary )
			Remove( weapon.Primary, weapon.InitialPrimaryStats );
		else
			Remove( weapon.Secondary, weapon.InitialSecondaryStats );

		//weapon.BulletVelocityMod -= BulletVelocity;
		applied = false;
	}

	private void Remove( ShootInfo shootInfo, StatsModifier initialStats )
	{
		if ( shootInfo is null || initialStats is null ) return;

		shootInfo.Damage -= initialStats.Damage * Damage;
		shootInfo.Recoil -= initialStats.Recoil * Recoil;
		shootInfo.Spread -= initialStats.Spread * Spread;
		shootInfo.RPM -= (int)(initialStats.RPM * RPM);
	}
}
