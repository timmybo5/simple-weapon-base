namespace SWB.Base;

public class StatsModifier
{
	public float Damage { get; set; }
	public float Recoil { get; set; }
	public float Spread { get; set; }
	public float RPM { get; set; }
	public float Force { get; set; }
	public float Mobility { get; set; }
	public float ReloadSpeed { get; set; }

	// After phys bullets have been recreated
	//public float BulletVelocity { get; set; } = 0;

	public static readonly StatsModifier Zero = new();

	private bool applied;

	public static StatsModifier FromShootInfo( Weapon weapon, ShootInfo shootInfo )
	{
		return new()
		{
			Damage = shootInfo.Damage,
			Recoil = shootInfo.Recoil,
			Spread = shootInfo.Spread,
			RPM = shootInfo.RPM,
			Force = shootInfo.Force,
			Mobility = weapon.Mobility,
			ReloadSpeed = weapon.ReloadSpeed,
		};
	}

	public void Apply( Weapon weapon, bool onPrimary = true )
	{
		if ( applied ) return;

		var shootInfo = onPrimary ? weapon.Primary : weapon.Secondary;
		Apply( weapon, shootInfo, weapon.InitialPrimaryStats );

		applied = true;
	}

	private void Apply( Weapon weapon, ShootInfo shootInfo, StatsModifier initialStats )
	{
		if ( shootInfo is null || initialStats is null ) return;

		shootInfo.Damage += initialStats.Damage * Damage;
		shootInfo.Recoil += initialStats.Recoil * Recoil;
		shootInfo.Spread += initialStats.Spread * Spread;
		shootInfo.RPM += (int)(initialStats.RPM * RPM);

		weapon.Mobility += initialStats.Mobility * Mobility;
		weapon.ReloadSpeed += initialStats.ReloadSpeed * ReloadSpeed;
	}
	public void Remove( Weapon weapon, bool onPrimary = true )
	{
		if ( !applied ) return;

		var shootInfo = onPrimary ? weapon.Primary : weapon.Secondary;
		Remove( weapon, shootInfo, weapon.InitialPrimaryStats );

		applied = false;
	}

	private void Remove( Weapon weapon, ShootInfo shootInfo, StatsModifier initialStats )
	{
		if ( shootInfo is null || initialStats is null ) return;

		shootInfo.Damage -= initialStats.Damage * Damage;
		shootInfo.Recoil -= initialStats.Recoil * Recoil;
		shootInfo.Spread -= initialStats.Spread * Spread;
		shootInfo.RPM -= (int)(initialStats.RPM * RPM);

		weapon.Mobility -= initialStats.Mobility * Mobility;
		weapon.ReloadSpeed -= initialStats.ReloadSpeed * ReloadSpeed;
	}
}
