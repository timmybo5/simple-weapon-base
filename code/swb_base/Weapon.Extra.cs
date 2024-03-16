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

	// Tucking
	public virtual float GetTuckDist()
	{
		if ( TuckRange == -1 )
			return -1;

		if ( !Owner.IsValid ) return -1;

		var pos = Owner.EyePos;
		var forward = Owner.EyeAngles.ToRotation().Forward;
		var trace = TraceBullet( Owner.EyePos, pos + forward * TuckRange );

		if ( !trace.Hit )
			return -1;

		return trace.Distance;
	}

	public bool ShouldTuck()
	{
		return GetTuckDist() != -1;
	}

	public bool ShouldTuck( out float dist )
	{
		dist = GetTuckDist();
		return dist != -1;
	}
}
