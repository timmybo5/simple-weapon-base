using System;
using System.Threading.Tasks;
using Sandbox;

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
            // Initialize physics
            MoveType = MoveType.Physics;
            PhysicsEnabled = true;
            UsePhysicsCollision = true;
            SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
            PhysicsGroup.AddVelocity(StartVelocity * Speed);
            PhysicsBody.GravityEnabled = UseGravity;

            // Delete entity
            if (RemoveDelay > 0)
                _ = DeleteAsync(RemoveDelay);
        }

        protected override void OnPhysicsCollision(CollisionEventData eventData)
        {
            base.OnPhysicsCollision(eventData);

            if (IsSticky && eventData.Entity.IsValid())
            {
                Velocity = Vector3.Zero;
                Parent = eventData.Entity;
            }
        }

        [Event.Tick.Server]
        public virtual void Tick()
        {
        }
    }

    public partial class WeaponBaseEntity : WeaponBase
    {
        public virtual Func<ClipInfo, bool, FiredEntity> CreateEntity => null; // Function that creates an entity and returns it ( to use custom entities in the base )
        public virtual string EntityModel => ""; // Path to the model of the entity
        public virtual Vector3 EntityVelocity => new Vector3(0, 0, 100); // Velocity ( right, up, forward )
        public virtual Angles EntityAngles => new Angles(0, 0, 0); // Spawn angles
        public virtual Vector3 EntitySpawnOffset => Vector3.Zero; // Spawn offset ( right, up, forward )
        public virtual float PrimaryEntitySpeed => 100f; // Primary velocity speed
        public virtual float SecondaryEntitySpeed => 50f; // Secondary velocity Speed
        public virtual float RemoveDelay => -1; // Delay that the entity should be removed after
        public virtual bool UseGravity => true; // Should gravity affect the entity
        public virtual bool IsSticky => false; // Should the entity stick to the surface it hits

        public virtual void FireEntity(ClipInfo clipInfo, bool isPrimary)
        {
            FiredEntity firedEntity;

            if (CreateEntity == null)
            {
                firedEntity = new FiredEntity();
            }
            else
            {
                firedEntity = CreateEntity(clipInfo, isPrimary);
            }

            if (!string.IsNullOrEmpty(EntityModel))
                firedEntity.SetModel(EntityModel);

            firedEntity.Owner = Owner;
            firedEntity.Position = MathUtil.RelativeAdd(Position, EntitySpawnOffset, Owner.EyeRot);
            firedEntity.Rotation = Owner.EyeRot * Rotation.From(EntityAngles);
            firedEntity.RemoveDelay = RemoveDelay;
            firedEntity.UseGravity = UseGravity;
            firedEntity.Speed = isPrimary ? PrimaryEntitySpeed : SecondaryEntitySpeed;
            firedEntity.IsSticky = IsSticky;
            firedEntity.Damage = clipInfo.Damage;
            firedEntity.Force = clipInfo.Force;
            firedEntity.StartVelocity = MathUtil.RelativeAdd(Vector3.Zero, EntityVelocity, Owner.EyeRot);
            firedEntity.Start();
        }

        public override void Attack(ClipInfo clipInfo, bool isPrimary)
        {
            if ((IsRunning && RunAnimData != null) || ShouldTuck()) return;

            TimeSincePrimaryAttack = 0;
            TimeSinceSecondaryAttack = 0;

            if (!TakeAmmo(1))
            {
                DryFire(clipInfo.DryFireSound);
                return;
            }

            // Player anim
            (Owner as AnimEntity).SetAnimBool("b_attack", true);

            // Weapon anim
            ScreenUtil.Shake(clipInfo.ScreenShake);
            ShootEffects(clipInfo.MuzzleFlashParticle, clipInfo.BulletEjectParticle, clipInfo.ShootAnim);

            if (!string.IsNullOrEmpty(clipInfo.ShootSound))
                PlaySound(clipInfo.ShootSound);

            if (Host.IsServer)
            {
                using (Prediction.Off())
                {
                    FireEntity(clipInfo, isPrimary);
                }
            }
        }

        async Task AsyncAttack(ClipInfo clipInfo, bool isPrimary, float delay)
        {
            if (AvailableAmmo() <= 0) return;

            TimeSincePrimaryAttack -= delay;
            TimeSinceSecondaryAttack -= delay;

            // Player anim
            (Owner as AnimEntity).SetAnimBool("b_attack", true);

            // Play pre-fire animation
            ShootEffects(null, null, clipInfo.ShootAnim);

            var owner = Owner as PlayerBase;
            if (owner == null) return;
            var activeWeapon = owner.ActiveChild;

            await GameTask.DelaySeconds(delay);

            // Check if owner and weapon are still valid
            if (owner == null || activeWeapon != owner.ActiveChild) return;

            // Take ammo
            TakeAmmo(1);

            // Play shoot effects
            ScreenUtil.Shake(clipInfo.ScreenShake);
            ShootEffects(clipInfo.MuzzleFlashParticle, clipInfo.BulletEjectParticle, null);

            if (clipInfo.ShootSound != null)
                PlaySound(clipInfo.ShootSound);

            if (IsServer)
            {
                using (Prediction.Off())
                {
                    FireEntity(clipInfo, isPrimary);
                }
            }
        }

        public override void DelayedAttack(ClipInfo clipInfo, bool isPrimary, float delay)
        {
            _ = AsyncAttack(Primary, isPrimary, PrimaryDelay);
        }
    }
}
