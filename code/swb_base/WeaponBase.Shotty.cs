/* 
 * Weapon base for weapons using shell based reloading 
*/

using System.Threading.Tasks;
using Sandbox;

namespace SWB_Base;

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
        ViewModelEntity?.SetAnimParameter(General.ReloadAnim, false);
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

    public async Task EjectShell(ParticleData bulletEjectParticle)
    {
        Game.AssertServer();

        var player = Owner as ISWBPlayer;
        var activeWeapon = player.ActiveChild;
        var instanceID = InstanceID;

        await GameTask.DelaySeconds(ShellEjectDelay);
        if (!IsAsyncValid(activeWeapon, instanceID)) return;
        DoBullectEjectEffect(bulletEjectParticle.Path, bulletEjectParticle.VMScale, bulletEjectParticle.WMScale);
    }

    public override void Reload()
    {
        General.ReloadTime = ShellReloadTimeStart + ShellReloadTimeInsert;
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

        if (Owner is ISWBPlayer player)
        {
            var hasInfiniteReserve = Primary.InfiniteAmmo == InfiniteAmmoType.reserve;
            var ammo = hasInfiniteReserve ? 1 : player.TakeAmmo(Primary.AmmoType, 1);

            // Workaround since ammo is not predicted
            var localAmmo = Primary.Ammo;

            if (ammo != 0)
            {
                Primary.Ammo += 1;
                localAmmo += 1;
            }

            if (ammo != 0 && localAmmo < Primary.ClipSize)
            {
                General.ReloadTime = ShellReloadTimeInsert;
                base.Reload();
            }
            else
            {
                CancelReload();
            }
        }
    }
}
