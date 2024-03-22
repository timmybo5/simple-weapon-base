namespace SWB.Base;

public interface IBulletBase
{
	public void Shoot( Weapon weapon, ShootInfo shootInfo, Vector3 spreadOffset );

	public Vector3 GetRandomSpread( float spread );
}
