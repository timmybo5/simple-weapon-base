/* 
 * Weapon base for weapons using shell based reloading 
*/

using System.Threading.Tasks;
using Sandbox;

namespace SWB_Base
{
    public partial class WeaponBaseShotty : WeaponBase
    {
        /// <summary>Duration of the reload start animation</summary>
        public virtual float ShellReloadTimeStart => -1;

        /// <summary>Duration of the reload insert animatio</summary>
        public virtual float ShellReloadTimeInsert => -1;

        /// <summary>The shell eject delay after firing</summary>
        public virtual float ShellEjectDelay => -1;

        /// <summary>Animation for finishing the reload</summary>
        public virtual string ReloadFinishAnim => "reload_finished";

        /// <summary>Can the shotgun shoot while reloading</summary>
        public virtual bool CanShootDuringReload => true;

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
            var player = Owner as PlayerBase;
            var activeWeapon = player.ActiveChild;
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
            var player = Owner as PlayerBase;
            var activeWeapon = player.ActiveChild;
            var instanceID = InstanceID;

            await GameTask.DelaySeconds(ShellEjectDelay);
            if (!IsAsyncValid(activeWeapon, instanceID)) return;
            StartReloadEffects(false, ReloadFinishAnim);
        }
    }
}
