using SWB.Shared;
using System;
using System.Linq;

namespace SWB.Base;

[Group( "SWB" )]
[Title( "HitScan Bullet Info" )]
public class HitScanBulletInfo : BulletInfo
{
	public override void Shoot( Weapon weapon, bool isPrimary, Vector3 spreadOffset )
	{
		if ( !weapon.IsValid() ) return;

		var player = weapon.Owner;
		if ( !player.IsValid() ) return;

		var forward = player.EyeAngles.Forward + spreadOffset;
		forward = forward.Normal;
		var endPos = player.EyePos + forward * 999999;
		var bulletTr = weapon.TraceBullet( player.EyePos, endPos );
		var hitObj = bulletTr.GameObject;
		var shootInfo = weapon.GetShootInfo( isPrimary );
		var hasTracer = ShouldSpawnTracer( shootInfo );
		var hasImpact = !SurfaceUtil.IsSkybox( bulletTr.Surface ) && bulletTr.HitPosition != Vector3.Zero;

		// Damage
		if ( hitObj is not null )
		{
			var target = hitObj.Components.GetInAncestorsOrSelf<IDamageable>();
			var hitTags = Array.Empty<string>();

			if ( bulletTr.Hitbox is not null )
				hitTags = bulletTr.Hitbox.Tags.TryGetAll().ToArray();

			var force = forward * 100 * shootInfo.Force;
			var dmgInfo = Shared.DamageInfo.FromBullet(
				weapon.Owner.GameObject,
				weapon.GameObject,
				bulletTr.Hitbox,
				bulletTr.EndPosition,
				bulletTr.Shape,
				weapon.ClassName,
				shootInfo.Damage,
				bulletTr.HitPosition,
				force,
				shootInfo.HitFlinch,
				Weapon.GetMovementImpactFromForce( shootInfo.Force ),
				hitTags
			);
			target?.OnDamage( dmgInfo );
		}

		// Effects
		if ( hasTracer || hasImpact )
			SpawnEffects( weapon, isPrimary, hasImpact, hasTracer, bulletTr.EndPosition, bulletTr.Normal, bulletTr.Surface?.SoundCollection.Bullet, bulletTr.Surface?.PrefabCollection.BulletImpact );
	}

	[Rpc.Broadcast( NetFlags.Unreliable )]
	public void SpawnEffects( Weapon weapon, bool isPrimary, bool hasImpact, bool hasTracer, Vector3 hitPos, Vector3 hitNormal, SoundEvent hitSound, GameObject hitParticles )
	{
		if ( !weapon.IsValid() ) return;

		// Impact
		if ( hasImpact )
			Weapon.CreateBulletImpact( hitPos, hitNormal, hitSound, hitParticles );

		// Tracer
		if ( hasTracer )
			TracerEffects( weapon, isPrimary, hitPos );
	}

	public virtual void TracerEffects( Weapon weapon, bool isPrimary, Vector3 hitPos )
	{
		var muzzleTransform = weapon.GetMuzzleTransform();
		if ( !muzzleTransform.HasValue ) return;

		var shootInfo = weapon.GetShootInfo( isPrimary );
		var scale = weapon.CanSeeViewModel ? shootInfo.VMParticleScale : shootInfo.WMParticleScale;
		var direction = (hitPos - muzzleTransform.Value.Position).Normal;
		var rotation = Rotation.LookAt( direction );
		var particleTransform = muzzleTransform.Value.WithRotation( rotation );
		weapon.CreateParticle( shootInfo.BulletTracerParticle, particleTransform, scale );
	}
}
