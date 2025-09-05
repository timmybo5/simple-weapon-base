using SWB.Shared;
using System;
using System.Linq;

namespace SWB.Base;

[Group( "SWB" )]
[Title( "HitScan Bullet Info" )]
public class HitScanBulletInfo : BulletInfo
{
	public override void Shoot( Weapon weapon, ShootInfo shootInfo, Vector3 spreadOffset )
	{
		if ( !weapon.IsValid ) return;

		var player = weapon.Owner;
		if ( player is null ) return;

		var forward = player.EyeAngles.Forward + spreadOffset;
		forward = forward.Normal;
		var endPos = player.EyePos + forward * 999999;
		var bulletTr = weapon.TraceBullet( player.EyePos, endPos );
		var hitObj = bulletTr.GameObject;

		if ( SurfaceUtil.IsSkybox( bulletTr.Surface ) || bulletTr.HitPosition == Vector3.Zero ) return;

		// Impact
		Weapon.CreateBulletImpact( bulletTr );

		// Tracer
		if ( ShouldSpawnTracer( shootInfo ) )
			TracerEffects( weapon, shootInfo, bulletTr );

		// Damage
		if ( !weapon.IsProxy && hitObj is not null )
		{
			var target = hitObj.Components.GetInAncestorsOrSelf<IDamageable>();

			var hitTags = Array.Empty<string>();

			if ( bulletTr.Hitbox is not null )
				hitTags = bulletTr.Hitbox.Tags.TryGetAll().ToArray();

			var force = forward * 100 * shootInfo.Force;
			var dmgInfo = Shared.DamageInfo.FromBullet( weapon.Owner.GameObject,
				weapon.GameObject,
				bulletTr.Hitbox,
				bulletTr.EndPosition,
				bulletTr.Shape,
				weapon.ClassName,
				shootInfo.Damage,
				bulletTr.HitPosition,
				force,
				hitTags
			);
			target?.OnDamage( dmgInfo );
		}
	}

	public virtual void TracerEffects( Weapon weapon, ShootInfo shootInfo, SceneTraceResult tr )
	{
		var muzzleTransform = weapon.GetMuzzleTransform();
		if ( !muzzleTransform.HasValue ) return;

		var scale = weapon.CanSeeViewModel ? shootInfo.VMParticleScale : shootInfo.WMParticleScale;
		var direction = (tr.HitPosition - muzzleTransform.Value.Position).Normal;
		var rotation = Rotation.LookAt( direction );
		var particleTransform = muzzleTransform.Value.WithRotation( rotation );
		weapon.CreateParticle( shootInfo.BulletTracerParticle, particleTransform, scale );
	}
}
