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
        private bool CanAttack(ClipInfo clipInfo, TimeSince lastAttackTime, InputButton inputButton)
        {
            if (IsAnimating) return false;
            if (clipInfo == null || !Owner.IsValid() || !Input.Down(inputButton)) return false;
            if (clipInfo.FiringType == FiringType.semi && !Input.Pressed(inputButton)) return false;
            if (clipInfo.RPM <= 0) return true;

            return lastAttackTime > (60f / clipInfo.RPM);
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

            if (!TakeAmmo(1))
            {
                DryFire(clipInfo.DryFireSound);
                return;
            }

            // Player anim
            (Owner as AnimEntity).SetAnimBool("b_attack", true);

            // Tell the clients to play the shoot effects
            ScreenUtil.Shake(clipInfo.ScreenShake);
            ShootEffects(clipInfo.MuzzleFlashParticle, clipInfo.BulletEjectParticle, clipInfo.ShootAnim);

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

            for (int i = 0; i < clipInfo.Bullets; i++)
            {
                ShootBullet(realSpread, clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize);
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

            // Shoot the bullets
            for (int i = 0; i < clipInfo.Bullets; i++)
            {
                ShootBullet(GetRealSpread(clipInfo.Spread), clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize);
            }
        }

        public virtual void DelayedAttack(ClipInfo clipInfo, bool isPrimary, float delay)
        {
            _ = AsyncAttack(Primary, isPrimary, PrimaryDelay);
        }

        public virtual void AttackPrimary()
        {
            if (DualWield)
            {
                dualWieldLeftFire = !dualWieldLeftFire;
            }

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

        /// Shoot a single bullet
        public virtual void ShootBullet(float spread, float force, float damage, float bulletSize)
        {

            // Spread
            var forward = Owner.EyeRot.Forward;
            forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
            forward = forward.Normal;

            // ShootBullet is coded in a way where we can have bullets pass through shit
            // or bounce off shit, in which case it'll return multiple results
            foreach (var tr in TraceBullet(Owner.EyePos, Owner.EyePos + forward * 5000, bulletSize))
            {
                tr.Surface.DoBulletImpact(tr);

                if (!IsServer) continue;
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
        protected virtual void ShootEffects(string muzzleFlashParticle, string bulletEjectParticle, string shootAnim)
        {
            Host.AssertClient();

            var animatingViewModel = DualWield && dualWieldLeftFire ? dualWieldViewModel : ViewModelEntity;
            ModelEntity firingViewModel = animatingViewModel;

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
                animatingViewModel?.SetAnimBool(shootAnim, true);

            CrosshairPanel?.CreateEvent("fire", (60f / Primary.RPM));
        }

        [ClientRpc]
        public virtual void DryFire(string dryFireSound)
        {
            if (!string.IsNullOrEmpty(dryFireSound))
                PlaySound(dryFireSound);
        }
    }
}
