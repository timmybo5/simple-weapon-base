using Sandbox;
using SWB_Base;

/*
 * This serves as a code example, css models are not included! 
*/

namespace SWB_CSS;

[Library("swb_css_deagle", Title = "Desert Eagle")]
public class Deagle : WeaponBase
{
    public override int Bucket => 1;
    public override HoldType HoldType => HoldType.Pistol;
    public override string ViewModelPath => "weapons/swb/css/deagle/css_v_pist_deagle.vmdl";
    public override string WorldModelPath => "weapons/swb/css/deagle/css_w_pist_deagle.vmdl";
    public override string Icon => "code/swb_css/textures/ui/css_icon_deagle.png";

    public Deagle()
    {
        General = new WeaponInfo
        {
            FOV = 75,
            ReloadTime = 2.17f
        };

        Primary = new ClipInfo
        {
            Ammo = 7,
            AmmoType = AmmoTypes.Revolver,
            ClipSize = 7,

            BulletSize = 6f,
            BulletType = new HitScanBullet(),
            Damage = 50f,
            Force = 5f,
            Spread = 0.06f,
            Recoil = 1f,
            RPM = 300,
            FiringType = FiringType.semi,
            ScreenShake = new ScreenShake
            {
                Length = 0.08f,
                Delay = 0.02f,
                Size = 0.5f,
                Rotation = 0.1f
            },

            DryFireSound = "swb_pistol.empty",
            ShootSound = "css_deagle.fire",

            BulletEjectParticle = new("particles/pistol_ejectbrass.vpcf"),
            MuzzleFlashParticle = new("particles/swb/muzzle/flash.vpcf"),
            BulletTracerParticle = new("particles/swb/tracer/tracer.vpcf"),

            InfiniteAmmo = InfiniteAmmoType.reserve
        };

        ZoomAnimData = new AngPos
        {
            Angle = new Angles(0, -0.1f, 0),
            Pos = new Vector3(-5.125f, 0, 2.67f)
        };

        RunAnimData = new AngPos
        {
            Angle = new Angles(-40, 0, 0),
            Pos = new Vector3(0, -3, -8)
        };
    }
}
