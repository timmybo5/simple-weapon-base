using SWB.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SWB.Base;

[Group( "SWB" )]
[Title( "HitScan Bullet Info" )]
public class HitScanBulletInfo : BulletInfo
{
	private const int MaxPenetrations = 10; // safety valve, also acts as a practical per-bullet pierce cap

	public override void Shoot( Weapon weapon, bool isPrimary, Vector3 spreadOffset )
	{
		if ( !weapon.IsValid() ) return;

		var player = weapon.Owner;
		if ( !player.IsValid() ) return;

		var forward = player.EyeAngles.Forward + spreadOffset;
		forward = forward.Normal;
		var endPos = player.EyePos + forward * 999999;
		var shootInfo = weapon.GetShootInfo( isPrimary );
		var hasTracer = ShouldSpawnTracer( shootInfo );

		var traceStart = player.EyePos;
		var ignoreGameObjects = new List<GameObject>();
		var traceIgnoreTags = shootInfo.Penetration ? Weapon.PenetrationBulletTraceIgnoreTags : null;
		var ricochetCount = 0;
		var shotId = Guid.NewGuid(); // correlates every hit from this single bullet (penetration + ricochet)
		Vector3? tracerSegmentStart = null; // null = draw from the muzzle (first segment)

		for ( int i = 0; i < MaxPenetrations; i++ )
		{
			var bulletTr = weapon.TraceBullet( traceStart, endPos, ignoreTags: traceIgnoreTags, extraIgnoreGOs: ignoreGameObjects );
			var hitObj = bulletTr.GameObject;
			var hasImpact = !SurfaceUtil.IsSkybox( bulletTr.Surface ) && bulletTr.HitPosition != Vector3.Zero;
			IPlayerBase penetratedPlayer = null;

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
					hitTags,
					weapon.GetKillDetails(),
					shotId
				);
				target?.OnDamage( dmgInfo );

				if ( shootInfo.Penetration && target is IPlayerBase playerTarget )
					penetratedPlayer = playerTarget;
			}

			// A ricochet can only happen off world geometry (not mid-penetration) that's angled shallow enough
			var canRicochet = penetratedPlayer is null
				&& hasImpact
				&& shootInfo.Ricochet
				&& ricochetCount < shootInfo.MaxRicochets
				&& SurfaceUtil.CanRicochet( bulletTr.Surface )
				&& SurfaceUtil.GetGrazingAngle( forward, bulletTr.Normal ) <= shootInfo.RicochetAngle
				&& Game.Random.Float( 0f, 1f ) < shootInfo.RicochetChance;

			// Effects
			var isFinalHit = penetratedPlayer is null && !canRicochet;
			var tracerSegmentEnds = isFinalHit || canRicochet; // penetration keeps the same straight tracer segment going
			var tracerThisIteration = hasTracer && tracerSegmentEnds;

			if ( hasImpact || tracerThisIteration )
				SpawnEffects( weapon, isPrimary, hasImpact, tracerThisIteration, tracerSegmentStart, bulletTr.EndPosition, bulletTr.Normal, bulletTr.Surface?.SoundCollection.Bullet, bulletTr.Surface?.PrefabCollection.BulletImpact );

			if ( isFinalHit )
				break;

			if ( canRicochet )
			{
				forward = Vector3.Reflect( forward, bulletTr.Normal ).Normal;
				endPos = bulletTr.EndPosition + forward * 999999;
				traceStart = bulletTr.EndPosition + bulletTr.Normal * 1.0f; // nudge off the surface so we don't immediately re-hit it
				tracerSegmentStart = bulletTr.EndPosition;
				ricochetCount++;
			}
			else
			{
				ignoreGameObjects.Add( penetratedPlayer.GameObject );
				traceStart = bulletTr.EndPosition + forward * 1.0f;
			}
		}
	}

	[Rpc.Broadcast( NetFlags.Unreliable )]
	public void SpawnEffects( Weapon weapon, bool isPrimary, bool hasImpact, bool hasTracer, Vector3? tracerStart, Vector3 hitPos, Vector3 hitNormal, SoundEvent hitSound, GameObject hitParticles )
	{
		if ( !weapon.IsValid() || Application.IsDedicatedServer ) return;

		// Impact
		if ( hasImpact )
			Weapon.CreateBulletImpact( hitPos, hitNormal, hitSound, hitParticles );

		// Tracer
		if ( hasTracer )
			TracerEffects( weapon, isPrimary, tracerStart, hitPos );
	}

	public virtual void TracerEffects( Weapon weapon, bool isPrimary, Vector3? tracerStart, Vector3 hitPos )
	{
		Vector3 startPos;

		if ( tracerStart.HasValue )
		{
			startPos = tracerStart.Value;
		}
		else
		{
			var muzzleTransform = weapon.GetMuzzleTransform();
			if ( !muzzleTransform.HasValue ) return;
			startPos = muzzleTransform.Value.Position;
		}

		var shootInfo = weapon.GetShootInfo( isPrimary );
		var scale = weapon.CanSeeViewModel ? shootInfo.VMParticleScale : shootInfo.WMParticleScale;
		var direction = (hitPos - startPos).Normal;
		var rotation = Rotation.LookAt( direction );
		var particleTransform = new Transform( startPos, rotation );
		weapon.CreateParticle( shootInfo.BulletTracerParticle, particleTransform, scale );
	}
}
