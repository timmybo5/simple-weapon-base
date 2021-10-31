/* 
 * Weapon base for weapons using shell based reloading 
*/

using Sandbox;

namespace SWB_Base
{
    public partial class WeaponBaseShotty : WeaponBase
    {
        public virtual float ShellReloadTimeStart => -1; // Duration of the reload start animation
        public virtual float ShellReloadTimeInsert => -1; // Duration of the reload insert animation
        public virtual bool CanShootDuringReload => true; // Can the shotgun shoot while reloading
        public override bool BulletCocking => false;

        private void CancelReload()
        {
            if (!CanShootDuringReload) return;

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

        public override void Reload()
        {
            Primary.ReloadTime = ShellReloadTimeStart;
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
                    Primary.ReloadTime = ShellReloadTimeInsert;
                    base.Reload();
                }
                else
                {
                    SendWeaponAnim("reload_finished");
                }
            }
        }
    }
}
