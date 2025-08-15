
using System;
using System.Linq;
using SWB.Shared;

namespace SWB.Base;

[Group( "SWB" )]
[Title( "Physical Bullet Mover" )]
public class PhysicalBulletMover : Component
{
	public Vector3 BulletVelocity { get; set; }

	public Weapon Weapon { get; set; }
	public ShootInfo ShootInfo { get; set; }
	public PhysicalBulletInfo BulletInfo { get; set; }

	public float BulletGravity => BulletInfo.BulletGravity;

	public float BulletDrag => BulletInfo.BulletDrag;

	public bool HasImpacted { get; private set; } = false;

	public void Initialize( PhysicalBulletInfo bulletInfo, Weapon weapon, ShootInfo shootInfo, Vector3 bulletVelocity )
	{
		BulletInfo = bulletInfo;
		Weapon = weapon;
		ShootInfo = shootInfo;
		BulletVelocity = bulletVelocity;

		// If a bullet is flying this long it probably got bugged
		GetOrAddComponent<TemporaryEffect>().DestroyAfterSeconds = 20f;
	}

	protected override void OnFixedUpdate()
	{
		if ( HasImpacted ) return;

		// Apply drag (before gravity so we aren't immediately dragging on gravity)
		BulletVelocity *= 1 - BulletDrag;

		// Apply gravity
		BulletVelocity += Vector3.Down * BulletGravity * Time.Delta;

		var bulletMovement = BulletVelocity * Time.Delta;

		// Trace along path to see if we hit anything
		var bulletTrace = Weapon.TraceBullet( WorldPosition, WorldPosition + bulletMovement );
		if ( bulletTrace.Hit )
		{
			HandleImpact( bulletTrace );
			WorldPosition = bulletTrace.HitPosition;

			// Allows for graceful ending of effects
			HasImpacted = true;
			GetOrAddComponent<TemporaryEffect>().DestroyAfterSeconds = 0.5f;
			ITemporaryEffect.DisableLoopingEffects( GameObject );
			return;
		}

		// If we didn't hit anything, we can move the bullet
		WorldPosition += bulletMovement;
	}

	protected void HandleImpact( SceneTraceResult traceResult )
	{
		var hitObject = traceResult.GameObject;

		if ( SurfaceUtil.IsSkybox( traceResult.Surface ) || traceResult.HitPosition == Vector3.Zero ) return;

		// Impact
		Weapon.CreateBulletImpact( traceResult );

		// Damage
		if ( !Weapon.IsProxy && hitObject is not null && hitObject.Tags.Has( TagsHelper.Player ) )
		{
			var target = hitObject.Components.GetInAncestorsOrSelf<IPlayerBase>();
			if ( !target.IsAlive ) return;

			var hitTags = Array.Empty<string>();

			if ( traceResult.Hitbox is not null )
				hitTags = traceResult.Hitbox.Tags.TryGetAll().ToArray();

			var forward = BulletVelocity.Normal;

			// Assume force from shoot info is the force at firing
			var force = BulletVelocity.Length.Remap( 0, BulletInfo.BulletVelocity, 0, ShootInfo.Force );

			var dmgInfo = Shared.DamageInfo.FromBullet( Weapon.Owner.Id, Weapon.ClassName, ShootInfo.Damage, traceResult.HitPosition, forward * 100 * force, hitTags );
			target?.TakeDamage( dmgInfo );
		}
	}
}
