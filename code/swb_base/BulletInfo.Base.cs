namespace SWB.Base;

public abstract class BulletInfo : Component
{
	public abstract void Shoot( Weapon weapon, ShootInfo shootInfo, Vector3 spreadOffset );

	public Vector3 GetRandomSpread( float spread )
	{
		return (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
	}
}
