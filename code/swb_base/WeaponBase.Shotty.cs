/* 
 * Weapon base for weapons using shell based reloading 
*/

namespace SWB_Base
{
    public partial class WeaponBaseShotty : WeaponBase
    {
        public virtual float ShellReloadTimeStart => -1; // Duration of the reload start animation
        public virtual float ShellReloadTimeInsert => -1; // Duration of the reload insert animation

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
                var ammo = player.TakeAmmo(Primary.AmmoType, 1);
                if (ammo == 0) return;

                Primary.Ammo += 1;

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
