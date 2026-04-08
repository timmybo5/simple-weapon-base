using System;

namespace SWB.Base;

public abstract class BulletInfo : Component
{
	public abstract void Shoot( Weapon weapon, bool isPrimary, Vector3 spreadOffset );

	public Vector3 GetRandomSpread( float spread )
	{
		return (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
	}

	public virtual bool ShouldSpawnTracer( ShootInfo shootInfo )
	{
		if ( shootInfo.BulletTracerParticle is null )
			return false;

		var randVal = Random.Shared.NextDouble();
		return randVal < shootInfo.BulletTracerChance;
	}
}
