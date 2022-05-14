using Sandbox;

/* 
 * Collection of non-specific getters
*/

namespace SWB_Base
{
    public partial class WeaponBase
    {
        public virtual ModelEntity GetEffectModel()
        {
            ModelEntity effectModel = ViewModelEntity;

            // We don't want to change the world effect origin if we or others can see it
            if ((IsLocalPawn && !Owner.IsFirstPersonMode) || !IsLocalPawn)
            {
                effectModel = EffectEntity;
            }

            return effectModel;
        }

        /// <summary>
        /// Gets the correct shoot animation
        /// </summary>
        /// <param name="clipInfo">Info used for the current attack</param>
        /// <returns></returns>
        public virtual string GetShootAnimation(ClipInfo clipInfo)
        {
            if (IsZooming && (!string.IsNullOrEmpty(clipInfo.ShootZoomedAnim)))
            {
                return clipInfo.ShootZoomedAnim;
            }
            else if (!string.IsNullOrEmpty(clipInfo.ShootAnim))
            {
                return clipInfo.ShootAnim;
            }

            return null;
        }

        // Pass the active child from before the delay
        protected bool IsAsyncValid(Entity activeChild, int instanceID)
        {
            var player = Owner as PlayerBase;

            return player != null && activeChild == player.ActiveChild && instanceID == InstanceID;
        }

        protected bool IsShooting()
        {
            if (Secondary == null)
                return GetRealRPM(Primary.RPM) > TimeSincePrimaryAttack;

            return GetRealRPM(Primary.RPM) > TimeSincePrimaryAttack || GetRealRPM(Secondary.RPM) > TimeSinceSecondaryAttack;
        }

        protected bool CanSeeViewModel()
        {
            return IsLocalPawn && Owner.IsFirstPersonMode;
        }

        public virtual float GetRealSpread(float baseSpread = -1)
        {
            if (!Owner.IsValid()) return 0;

            float spread = baseSpread != -1 ? baseSpread : Primary.Spread;
            float floatMod = 1f;

            // Ducking
            if (Input.Down(InputButton.Duck) && (!IsZooming || this is WeaponBaseShotty))
                floatMod -= 0.25f;

            // Aiming
            if (IsZooming && this is not WeaponBaseShotty)
                floatMod /= 4;
            
            if (Owner.GroundEntity == null) {
                // Jumping
                floatMod += 0.75f;
            } else if (Owner.Velocity.Length > 100)
            {
                // Moving 
                floatMod += 0.25f;
            }

            return spread * floatMod;
        }

        public Transform? GetModelAttachment(string name, bool worldspace = true)
        {
            return base.GetAttachment(name, worldspace);
        }

        public int GetAvailableAmmo()
        {
            if (Owner is not PlayerBase owner) return 0;

            // Show clipsize as the available ammo
            if (Primary.InfiniteAmmo == InfiniteAmmoType.reserve)
                return Primary.ClipSize;

            return owner.AmmoCount(Primary.AmmoType);
        }

        public bool IsUsable()
        {
            if (Primary.Ammo > 0) return true;
            return GetAvailableAmmo() > 0;
        }

        // Static
        protected static float GetRealRPM(int rpm)
        {
            return (60f / rpm);
        }
    }
}
