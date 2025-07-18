using SWB.Shared;
using System;
using System.Linq;

namespace SWB.Base;

public class HitScanBullet : IBulletBase
{
	public void Shoot( Weapon weapon, ShootInfo shootInfo, Vector3 spreadOffset )
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
		weapon.CreateBulletImpact( bulletTr );

		// Tracer
		if ( shootInfo.BulletTracerParticle is not null )
		{
			var random = new Random();
			var randVal = random.NextDouble();

			if ( randVal < shootInfo.BulletTracerChance )
				TracerEffects( weapon, shootInfo, bulletTr );
		}

		// Damage
		if ( !weapon.IsProxy && hitObj is not null && hitObj.Tags.Has( TagsHelper.Player ) )
		{
			var target = hitObj.Components.GetInAncestorsOrSelf<IPlayerBase>();
			if ( !target.IsAlive ) return;

			var hitTags = Array.Empty<string>();

			if ( bulletTr.Hitbox is not null )
				hitTags = bulletTr.Hitbox.Tags.TryGetAll().ToArray();

			var dmgInfo = Shared.DamageInfo.FromBullet( weapon.Owner.Id, weapon.ClassName, shootInfo.Damage, bulletTr.HitPosition, forward * 100 * shootInfo.Force, hitTags );
			target?.TakeDamage( dmgInfo );
		}
	}

	public Vector3 GetRandomSpread( float spread )
	{
		return (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
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
