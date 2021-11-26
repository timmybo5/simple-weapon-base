/* 
 * Weapon base for weapons using shell based reloading 
*/

using System.Threading.Tasks;
using Sandbox;

namespace SWB_Base
{
    public partial class WeaponBaseShotty : WeaponBase
    {
        public virtual float ShellReloadTimeStart => -1; // Duration of the reload start animation
        public virtual float ShellReloadTimeInsert => -1; // Duration of the reload insert animation
        public virtual float ShellEjectDelay => -1; // The shell eject delay after firing
        public virtual string ReloadFinishAnim => "reload_finished"; // Finishing reload animation
        public virtual bool CanShootDuringReload => true; // Can the shotgun shoot while reloading

        public override bool BulletCocking => false;

        private void CancelReload()
        {
            IsReloading = false;
        }

        public override void AttackPrimary()
        {
            if (IsReloading && !CanShootDuringReload) return;

            CancelReload();
            base.AttackPrimary();
        }

        public override void AttackSecondary()
        {
            if (IsReloading && !CanShootDuringReload) return;

            CancelReload();
            base.AttackSecondary();
        }

        public async Task EjectShell(string bulletEjectParticle)
        {
            var activeWeapon = Owner.ActiveChild;
            var instanceID = InstanceID;

            await GameTask.DelaySeconds(ShellEjectDelay);
            if (!IsAsyncValid(activeWeapon, instanceID)) return;
            ShootEffects(null, bulletEjectParticle, null);
        }

        public override void Reload()
        {
            General.ReloadTime = ShellReloadTimeStart;
            base.Reload();
        }

        public override void OnReloadFinish()
        {
            IsReloading = false;

            if (!CanShootDuringReload)
            {
                TimeSincePrimaryAttack = 0;
                TimeSinceSecondaryAttack = 0;
            }

            if (Primary.Ammo >= Primary.ClipSize)
                return;

            if (Owner is PlayerBase player)
            {
                var hasInfiniteReserve = Primary.InfiniteAmmo == InfiniteAmmoType.reserve;
                var ammo = hasInfiniteReserve ? 1 : player.TakeAmmo(Primary.AmmoType, 1);

                if (ammo != 0)
                {
                    Primary.Ammo += 1;
                }

                if (ammo != 0 && Primary.Ammo < Primary.ClipSize)
                {
                    General.ReloadTime = ShellReloadTimeInsert;
                    base.Reload();
                }
                else
                {
                    StartReloadEffects(false, General.ReloadAnim);
                    _ = FinishReload();
                }
            }
        }

        public async Task FinishReload()
        {
            var activeWeapon = Owner.ActiveChild;
            var instanceID = InstanceID;

            await GameTask.DelaySeconds(ShellEjectDelay);
            if (!IsAsyncValid(activeWeapon, instanceID)) return;
            StartReloadEffects(false, ReloadFinishAnim);
        }
    }
}
