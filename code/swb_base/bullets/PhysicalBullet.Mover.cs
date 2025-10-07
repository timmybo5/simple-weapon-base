using SWB.Shared;
using System;
using System.Linq;

namespace SWB.Base;

[Group( "SWB" )]
[Title( "Physical Bullet Mover" )]
public class PhysicalBulletMover : Component
{
	public IPlayerBase Owner { get; set; }
	public string ClassName { get; set; }
	public Vector3 BulletVelocity { get; set; }
	public Weapon Weapon { get; set; }
	public ShootInfo ShootInfo { get; set; }
	public PhysicalBulletInfo BulletInfo { get; set; }
	public float BulletGravity => BulletInfo.Gravity;
	public float BulletDrag => BulletInfo.Drag;
	public bool HasImpacted { get; private set; } = false;

	public void Initialize( PhysicalBulletInfo bulletInfo, Weapon weapon, ShootInfo shootInfo, Vector3 bulletVelocity )
	{
		BulletInfo = bulletInfo;
		Owner = weapon.Owner;
		ClassName = weapon.ClassName;
		Weapon = weapon;
		ShootInfo = shootInfo;
		BulletVelocity = bulletVelocity;

		// If a bullet is flying this long it probably got bugged
		GetOrAddComponent<TemporaryEffect>().DestroyAfterSeconds = 20f;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy || HasImpacted || Owner is null ) return;

		// Apply drag (before gravity so we aren't immediately dragging on gravity)
		BulletVelocity *= 1 - BulletDrag;

		// Apply gravity
		BulletVelocity += Vector3.Down * BulletGravity * Time.Delta;

		var bulletMovement = BulletVelocity * Time.Delta;

		// Trace along path to see if we hit anything
		var bulletTrace = Weapon.TraceBullet( Owner.GameObject, WorldPosition, WorldPosition + bulletMovement );
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
		var decal = Weapon.CreateBulletImpact( traceResult );
		decal?.NetworkSpawn();

		// Damage
		if ( hitObject is not null )
		{
			var target = hitObject.Components.GetInAncestorsOrSelf<IDamageable>();

			var hitTags = Array.Empty<string>();

			if ( traceResult.Hitbox is not null )
				hitTags = traceResult.Hitbox.Tags.TryGetAll().ToArray();

			var forward = BulletVelocity.Normal;

			// Assume force from shoot info is the force at firing
			var force = BulletVelocity.Length.Remap( 0, BulletInfo.Velocity, 0, ShootInfo.Force );

			var dmgInfo = Shared.DamageInfo.FromBullet( Owner.GameObject, Weapon.GameObject, traceResult.Hitbox, traceResult.EndPosition, traceResult.Shape, ClassName, ShootInfo.Damage, traceResult.HitPosition, forward * 100 * force, hitTags );
			target?.OnDamage( dmgInfo );
		}
	}
}
