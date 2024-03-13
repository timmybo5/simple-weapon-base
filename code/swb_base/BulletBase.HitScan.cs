using SWB.Shared;
using System;

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

		// Impact
		weapon.CreateBulletImpact( bulletTr );

		// Tracer
		if ( shootInfo.BulletTracerParticle is not null )
		{
			var random = new Random();
			var randVal = random.NextDouble();

			if ( randVal < shootInfo.BulletTracerChance )
				TracerEffects( weapon, shootInfo, bulletTr.HitPosition );
		}


		// Damage
		if ( !weapon.IsProxy && hitObj.Tags.Has( TagsHelper.Player ) )
		{
			var target = hitObj.Components.GetInAncestorsOrSelf<IPlayerBase>();
			target?.TakeDamage( Shared.DamageInfo.FromBullet( shootInfo.Damage, forward * 25 * shootInfo.Force ) );
		}

	}

	void TracerEffects( Weapon weapon, ShootInfo shootInfo, Vector3 endPos )
	{
		var scale = weapon.CanSeeViewModel ? shootInfo.VMParticleScale : shootInfo.WMParticleScale;
		var muzzleTransform = weapon.GetMuzzleTransform();

		if ( !muzzleTransform.HasValue ) return;

		SceneParticles particles = new( weapon.Scene.SceneWorld, shootInfo.BulletTracerParticle );
		particles?.SetControlPoint( 1, muzzleTransform.Value );
		particles?.SetControlPoint( 2, endPos );
		particles?.SetNamedValue( "scale", scale );
		particles?.PlayUntilFinished( TaskSource.Create() );
	}
}
