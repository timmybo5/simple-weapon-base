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
        public override bool BulletCocking => false;

        public override void Reload()
        {
            Primary.ReloadTime = ShellReloadTimeStart;
            base.Reload();
        }

        public override void OnReloadFinish()
        {
            IsReloading = false;

            TimeSincePrimaryAttack = 0;
            TimeSinceSecondaryAttack = 0;

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

                if (Primary.Ammo < Primary.ClipSize)
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
