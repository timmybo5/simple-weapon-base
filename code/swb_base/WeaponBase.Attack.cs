using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base
{
    public partial class WeaponBase
    {
        public virtual bool CanAttack(ClipInfo clipInfo, TimeSince lastAttackTime, InputButton inputButton)
        {
            if (IsAnimating || inBoltBack) return false;
            if (clipInfo == null || !Owner.IsValid() || !Input.Down(inputButton)) return false;
            if (clipInfo.FiringType == FiringType.semi && !Input.Pressed(inputButton)) return false;
            if (clipInfo.FiringType == FiringType.burst)
            {
                if (burstCount > 2) return false;

                if (Input.Down(inputButton) && lastAttackTime > GetRealRPM(clipInfo.RPM))
                {
                    burstCount++;
                    return true;
                }

                return false;
            };

            if (clipInfo.RPM <= 0) return true;

            return lastAttackTime > GetRealRPM(clipInfo.RPM);
        }

        public virtual bool CanPrimaryAttack()
        {
            return CanAttack(Primary, TimeSincePrimaryAttack, InputButton.Attack1);
        }

        public virtual bool CanSecondaryAttack()
        {
            return CanAttack(Secondary, TimeSinceSecondaryAttack, InputButton.Attack2);
        }

        public virtual void Attack(ClipInfo clipInfo, bool isPrimary)
        {
            if (IsRunning || ShouldTuck()) return;

            TimeSincePrimaryAttack = 0;
            TimeSinceSecondaryAttack = 0;
            TimeSinceFired = 0;

            if (!TakeAmmo(1))
            {
                DryFire(clipInfo.DryFireSound);
                return;
            }

            // Boltback
            var bulletEjectParticle = clipInfo.BoltBackTime > -1 ? "" : clipInfo.BulletEjectParticle;

            if (clipInfo.Ammo > 0 && clipInfo.BoltBackTime > -1)
            {
                if (IsServer)
                    _ = AsyncBoltBack(GetRealRPM(clipInfo.RPM), clipInfo.BoltBackAnim, clipInfo.BoltBackTime, clipInfo.BoltBackEjectDelay, clipInfo.BulletEjectParticle, true);
            }

            // Shotgun
            if (this is WeaponBaseShotty shotty)
            {
                if (IsServer)
                    _ = shotty.EjectShell(bulletEjectParticle);

                bulletEjectParticle = "";
            }


            // Player anim
            (Owner as AnimEntity).SetAnimBool("b_attack", true);

            // Shoot effects
            if (IsClient)
                ScreenUtil.Shake(clipInfo.ScreenShake);

            ShootEffects(clipInfo.MuzzleFlashParticle, bulletEjectParticle, clipInfo.ShootAnim);

            // Barrel smoke
            if (IsServer && BarrelSmoking)
            {
                AddBarrelHeat();
                if (barrelHeat >= clipInfo.ClipSize * 0.75)
                {
                    ShootEffects(clipInfo.BarrelSmokeParticle, null, null);
                }
            }

            if (clipInfo.ShootSound != null)
                PlaySound(clipInfo.ShootSound);

            // Shoot the bullets
            float realSpread;

            if (this is WeaponBaseShotty)
            {
                realSpread = clipInfo.Spread;
            }
            else
            {
                realSpread = IsZooming ? clipInfo.Spread / 4 : clipInfo.Spread;
            }

            if (IsServer)
            {
                for (int i = 0; i < clipInfo.Bullets; i++)
                {
                    ShootBullet(realSpread, clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize);
                }
            }

            // Recoil
            doRecoil = true;
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
            var instanceID = InstanceID;

            await GameTask.DelaySeconds(delay);

            // Check if owner and weapon are still valid
            if (!IsAsyncValid(activeWeapon, instanceID)) return;

            // Take ammo
            TakeAmmo(1);

            // Shoot effects
            if (IsClient)
                ScreenUtil.Shake(clipInfo.ScreenShake);

            ShootEffects(clipInfo.MuzzleFlashParticle, clipInfo.BulletEjectParticle, null);

            if (clipInfo.ShootSound != null)
                PlaySound(clipInfo.ShootSound);

            // Shoot the bullets
            if (IsServer)
            {
                for (int i = 0; i < clipInfo.Bullets; i++)
                {
                    ShootBullet(GetRealSpread(clipInfo.Spread), clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize);
                }
            }
        }

        public virtual void DelayedAttack(ClipInfo clipInfo, bool isPrimary, float delay)
        {
            _ = AsyncAttack(Primary, isPrimary, PrimaryDelay);
        }

        public virtual void AttackPrimary()
        {
            if (PrimaryDelay > 0)
            {
                DelayedAttack(Primary, true, PrimaryDelay);
            }
            else
            {
                Attack(Primary, true);
            }
        }

        public virtual void AttackSecondary()
        {
            if (Secondary != null)
            {
                if (SecondaryDelay > 0)
                {
                    DelayedAttack(Secondary, false, SecondaryDelay);
                }
                else
                {
                    Attack(Secondary, false);
                }
                return;
            }
        }

        /// <summary>
        /// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
        /// hits, like if you're going through layers or ricocet'ing or something.
        /// </summary>
        public virtual IEnumerable<TraceResult> TraceBullet(Vector3 start, Vector3 end, float radius = 2.0f)
        {
            bool InWater = Physics.TestPointContents(start, CollisionLayer.Water);

            var tr = Trace.Ray(start, end)
                    .UseHitboxes()
                    .HitLayer(CollisionLayer.Water, !InWater)
                    .Ignore(Owner)
                    .Ignore(this)
                    .Size(radius)
                    .Run();

            yield return tr;

            //
            // Another trace, bullet going through thin material, penetrating water surface?
            //
        }

        /// Shoot a single bullet (server)
        public virtual void ShootBullet(float spread, float force, float damage, float bulletSize)
        {
            // Spread
            var forward = Owner.EyeRot.Forward;
            forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
            forward = forward.Normal;
            var endPos = Owner.EyePos + forward * 999999;

            // Client bullet
            ShootClientBullet(Owner.EyePos, endPos, bulletSize);

            // Server bullet
            foreach (var tr in TraceBullet(Owner.EyePos, endPos, bulletSize))
            {

                if (!tr.Entity.IsValid()) continue;

                // We turn prediction off for this, so any exploding effects don't get culled etc
                using (Prediction.Off())
                {
                    var damageInfo = DamageInfo.FromBullet(tr.EndPos, forward * 100 * force, damage)
                        .UsingTraceResult(tr)
                        .WithAttacker(Owner)
                        .WithWeapon(this);

                    tr.Entity.TakeDamage(damageInfo);
                }
            }
        }

        [ClientRpc]
        public virtual void ShootClientBullet(Vector3 startPos, Vector3 endPos, float radius = 2.0f)
        {
            foreach (var tr in TraceBullet(startPos, endPos, radius))
            {
                // Impact
                tr.Surface.DoBulletImpact(tr);

                // Tracer
                if (!string.IsNullOrEmpty(Primary.BulletTracerParticle))
                {
                    var random = new Random();
                    var randVal = random.Next(0, 2);

                    if (randVal == 0)
                        TracerEffects(Primary.BulletTracerParticle, tr.EndPos);
                }
            }
        }

        async Task AsyncBoltBack(float boltBackDelay, string boltBackAnim, float boltBackTime, float boltBackEjectDelay, string bulletEjectParticle, bool force = false)
        {
            var activeWeapon = Owner.ActiveChild;
            var instanceID = InstanceID;
            inBoltBack = force;

            // Start boltback
            await GameTask.DelaySeconds(boltBackDelay);
            if (!IsAsyncValid(activeWeapon, instanceID)) return;
            SendWeaponAnim(boltBackAnim);

            // Eject shell
            await GameTask.DelaySeconds(boltBackEjectDelay);
            if (!IsAsyncValid(activeWeapon, instanceID)) return;
            ShootEffects(null, bulletEjectParticle, null);

            // Finished
            await GameTask.DelaySeconds(boltBackTime - boltBackEjectDelay);
            if (!IsAsyncValid(activeWeapon, instanceID)) return;
            inBoltBack = false;
        }

        [ClientRpc]
        protected virtual void ShootEffects(string muzzleFlashParticle, string bulletEjectParticle, string shootAnim)
        {
            Host.AssertClient();

            ModelEntity firingViewModel = ViewModelEntity;

            if (firingViewModel == null) return;

            // We don't want to change the world effect origin if we or others can see it
            if ((IsLocalPawn && !Owner.IsFirstPersonMode) || !IsLocalPawn)
            {
                firingViewModel = EffectEntity;
            }

            if (!string.IsNullOrEmpty(muzzleFlashParticle))
                Particles.Create(muzzleFlashParticle, firingViewModel, "muzzle");

            if (!string.IsNullOrEmpty(bulletEjectParticle))
                Particles.Create(bulletEjectParticle, firingViewModel, "ejection_point");

            if (!string.IsNullOrEmpty(shootAnim))
            {
                ViewModelEntity?.SetAnimBool(shootAnim, true);
                CrosshairPanel?.CreateEvent("fire", (60f / Primary.RPM));
            }
        }

        protected virtual void TracerEffects(string tracerParticle, Vector3 endPos)
        {
            ModelEntity firingViewModel = GetEffectModel();

            if (firingViewModel == null) return;

            var muzzleAttach = firingViewModel.GetAttachment("muzzle");
            var tracer = Particles.Create(tracerParticle);
            tracer.SetPosition(1, muzzleAttach.GetValueOrDefault().Position);
            tracer.SetPosition(2, endPos);
        }

        [ClientRpc]
        public virtual void DryFire(string dryFireSound)
        {
            if (!string.IsNullOrEmpty(dryFireSound))
                PlaySound(dryFireSound);
        }
    }
}
