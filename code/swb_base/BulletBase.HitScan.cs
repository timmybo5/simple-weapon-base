using SWB.Player;
using SWB.Shared;

namespace SWB.Base;

public class HitScanBullet : IBulletBase
{
	public void Shoot( Weapon weapon, ShootInfo shootInfo )
	{
		var player = weapon.Owner;
		var forward = player.EyeAngles.Forward;
		var spread = weapon.GetRealSpread( shootInfo.Spread );
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;
		var endPos = player.EyePos + forward * 999999;
		var bulletTr = weapon.TraceBullet( player.EyePos, endPos );
		var hitObj = bulletTr.GameObject;

		if ( SurfaceUtil.IsSkybox( bulletTr.Surface ) ) return;

		weapon.CreateBulletImpact( bulletTr );

		if ( !weapon.IsProxy && hitObj.Tags.Has( TagsHelper.Player ) )
		{
			var target = hitObj.Components.GetInAncestorsOrSelf<PlayerBase>();
			target?.TakeDamage( Shared.DamageInfo.FromBullet( shootInfo.Damage, forward * 25 * shootInfo.Force ) );
		}
	}
}
