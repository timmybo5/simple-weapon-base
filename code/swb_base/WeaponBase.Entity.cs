using Sandbox;
using System;
using System.Threading.Tasks;

/* 
 * Weapon base for weapons firing entities
*/

namespace SWB_Base
{
	public class FiredEntity : ModelEntity
	{
		public WeaponBase Weapon { get; set; } // The parent weapon
		public Vector3 StartVelocity { get; set; }
		public float RemoveDelay { get; set; }
		public float Damage { get; set; }
		public float Force { get; set; }
		public float Speed { get; set; }
		public bool UseGravity { get; set; }
		public bool IsSticky { get; set; }

		public bool canThink;
		public bool isStuck;

		public override void Spawn()
		{
			base.Spawn();
		}

		public virtual void Start()
		{
			if ( UseGravity )
			{
				// Initialize physics
				MoveType = MoveType.Physics;
				PhysicsEnabled = true;
				UsePhysicsCollision = true;
				SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
				PhysicsGroup.AddVelocity( StartVelocity * Speed );

				// Delete entity
				if ( RemoveDelay > 0 )
					_ = DeleteAsync( RemoveDelay );
			}
			else
			{
				canThink = true;
			}
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			base.OnPhysicsCollision( eventData );
		}

		public virtual void OnCollision( TraceResult traceResult )
		{
			if ( RemoveDelay <= 0 )
				Delete();
		}

		[Event.Tick.Server]
		public virtual void Tick()
		{
			if ( !canThink || !IsServer || isStuck )
				return;

			var velocity = Rotation.Forward * Speed;
			var start = Position;
			var end = start + velocity * Time.Delta;

			var tr = Trace.Ray( start, end )
					.UseHitboxes()
					.Ignore( Owner )
					.Ignore( this )
					.Size( 1.0f )
					.Run();


			if ( tr.Hit )
			{
				if ( IsSticky )
					isStuck = true;

				Position = tr.EndPos + Rotation.Forward * -1;

				if ( tr.Entity.IsValid() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, tr.Direction * 200, Damage )
														.UsingTraceResult( tr )
														.WithAttacker( Owner )
														.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}

				SetParent( tr.Entity, tr.Bone );
				Owner = null;

				// Surface impact effect
				tr.Normal = Rotation.Forward * -1;
				tr.Surface.DoBulletImpact( tr );
				velocity = default;

				// Delete entity
				if ( RemoveDelay > 0 )
					_ = DeleteAsync( RemoveDelay );

				OnCollision( tr );
			}
			else
			{
				Position = end;
			}
		}
	}

	public partial class WeaponBaseEntity : WeaponBase
	{
		public virtual Func<ClipInfo, bool, FiredEntity> CreateEntity => null; // Function that creates an entity and returns it ( to use custom entities in the base )
		public virtual string EntityModel => ""; // Path to the model of the entity
		public virtual Vector3 EntityVelocity => new Vector3( 0, 0, 100 ); // Velocity ( right, up, forward )
		public virtual Vector3 EntitySpawnOffset => Vector3.Zero; // Entity spawn offset ( right, up, forward )
		public virtual float PrimaryEntitySpeed => 100f; // Primary velocity speed
		public virtual float SecondaryEntitySpeed => 50f; // Secondary velocity Speed
		public virtual float RemoveDelay => -1; // Delay that the entity should be removed after
		public virtual bool UseGravity => true; // Should gravity affect the entity
		public virtual bool IsSticky => false; // Should the entity stick to the surface it hits

		public virtual void FireEntity( ClipInfo clipInfo, bool isPrimary )
		{
			FiredEntity firedEntity;

			if ( CreateEntity == null )
			{
				firedEntity = new FiredEntity();
			}
			else
			{
				firedEntity = CreateEntity( clipInfo, isPrimary );
			}

			if ( !string.IsNullOrEmpty( EntityModel ) )
				firedEntity.SetModel( EntityModel );

			firedEntity.Owner = Owner;
			firedEntity.Position = MathZ.RelativeAdd( Position, EntitySpawnOffset, Owner.EyeRot );
			firedEntity.Rotation = Owner.EyeRot;

			firedEntity.RemoveDelay = RemoveDelay;
			firedEntity.UseGravity = UseGravity;
			firedEntity.Speed = isPrimary ? PrimaryEntitySpeed : SecondaryEntitySpeed;
			firedEntity.IsSticky = IsSticky;
			firedEntity.Damage = clipInfo.Damage;
			firedEntity.Force = clipInfo.Force;
			firedEntity.StartVelocity = MathZ.RelativeAdd( Vector3.Zero, EntityVelocity, Owner.EyeRot );

			firedEntity.Start();
		}

		public override void Attack( ClipInfo clipInfo, bool isPrimary )
		{
			if ( IsRunning && RunAnimData != null ) return;

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( !TakeAmmo( 1 ) )
			{
				DryFire( clipInfo.DryFireSound );
				return;
			}

			// Player anim
			(Owner as AnimEntity).SetAnimBool( "b_attack", true );

			// Weapon anim
			ShootEffects( clipInfo.MuzzleFlashParticle, clipInfo.BulletEjectParticle, clipInfo.ShootAnim, clipInfo.ScreenShake );

			if ( !string.IsNullOrEmpty( clipInfo.ShootSound ) )
				PlaySound( clipInfo.ShootSound );

			if ( Host.IsServer )
			{
				using ( Prediction.Off() )
				{
					FireEntity( clipInfo, isPrimary );
				}
			}
		}

		async Task AsyncAttack( ClipInfo clipInfo, bool isPrimary, float delay )
		{
			if ( AvailableAmmo() <= 0 ) return;

			TimeSincePrimaryAttack -= delay;
			TimeSinceSecondaryAttack -= delay;

			// Player anim
			(Owner as AnimEntity).SetAnimBool( "b_attack", true );

			// Play pre-fire animation
			ShootEffects( null, null, clipInfo.ShootAnim, null );

			var owner = Owner as PlayerBase;
			if ( owner == null ) return;
			var activeWeapon = owner.ActiveChild;

			await GameTask.DelaySeconds( delay );

			// Check if owner and weapon are still valid
			if ( owner == null || activeWeapon != owner.ActiveChild ) return;

			// Take ammo
			TakeAmmo( 1 );

			// Play shoot effects
			ShootEffects( clipInfo.MuzzleFlashParticle, clipInfo.BulletEjectParticle, null, clipInfo.ScreenShake );

			if ( clipInfo.ShootSound != null )
				PlaySound( clipInfo.ShootSound );

			if ( IsServer )
			{
				using ( Prediction.Off() )
				{
					FireEntity( clipInfo, isPrimary );
				}
			}
		}

		public override void DelayedAttack( ClipInfo clipInfo, bool isPrimary, float delay )
		{
			_ = AsyncAttack( Primary, isPrimary, PrimaryDelay );
		}
	}
}
