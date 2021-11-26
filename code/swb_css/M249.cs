using Sandbox;
using SWB_Base;

namespace SWB_CSS
{
    [Library("swb_css_m249", Title = "M249 PARA")]
    public class M249 : WeaponBase
    {
        public override int Bucket => 4;
        public override HoldType HoldType => HoldType.Rifle;
        public override string ViewModelPath => "weapons/swb/css/m249/css_v_mach_m249para.vmdl";
        public override string WorldModelPath => "weapons/swb/css/m249/css_w_mach_m249para.vmdl";
        public override string Icon => "/swb_css/textures/ui/css_icon_m249.png";
        public override int FOV => 75;
        public override int ZoomFOV => 40;
        public override float WalkAnimationSpeedMod => 0.7f;

        public M249()
        {
            General = new WeaponInfo
            {
                ReloadTime = 5.7f
            };

            Primary = new ClipInfo
            {
                Ammo = 100,
                AmmoType = AmmoType.LMG,
                ClipSize = 100,

                BulletSize = 5f,
                Damage = 15f,
                Force = 4f,
                Spread = 0.2f,
                Recoil = 0.7f,
                RPM = 800,
                FiringType = FiringType.auto,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 1.0f,
                    Rotation = 0.5f
                },

                DryFireSound = "swb_lmg.empty",
                ShootSound = "css_m249.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_large.vpcf",
                BulletTracerParticle = "particles/swb/tracer/tracer_large.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(1f, 0f, 0),
                Pos = new Vector3(-4.425f, 2, 2.45f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 30, 0),
                Pos = new Vector3(4, 0, 0)
            };
        }
    }
}
