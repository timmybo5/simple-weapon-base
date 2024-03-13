namespace SWB.Base;

public partial class Weapon
{
	// Burst Fire
	public void ResetBurstFireCount( ShootInfo shootInfo, string inputButton )
	{
		if ( shootInfo is null || shootInfo.FiringType != FiringType.burst ) return;

		if ( Input.Released( inputButton ) )
		{
			burstCount = 0;
		}
	}

	// Barrel heat
	public void BarrelHeatCheck()
	{
		if ( TimeSincePrimaryShoot > 3 && TimeSinceSecondaryShoot > 0 )
		{
			barrelHeat = 0;
		}
	}
}
