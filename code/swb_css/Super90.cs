using Sandbox;
using SWB_Base;

namespace SWB_CSS
{
    [Library("swb_css_super90", Title = "M3 Super 90")]
    public class Super90 : WeaponBaseShotty
    {
        public override int Bucket => 2;
        public override HoldType HoldType => HoldType.Shotgun;
        public override string ViewModelPath => "weapons/swb/css/super90/css_v_shot_m3super90.vmdl";
        public override string WorldModelPath => "weapons/swb/css/super90/css_w_shot_m3super90.vmdl";
        public override string Icon => "/swb_css/textures/ui/css_icon_super90.png";
        public override int FOV => 75;
        public override int ZoomFOV => 45;
        public override float WalkAnimationSpeedMod => 0.9f;

        public override float ShellReloadTimeStart => 0.38f;
        public override float ShellReloadTimeInsert => 0.49f;

        public Super90()
        {
            Primary = new ClipInfo
            {
                Ammo = 8,
                AmmoType = AmmoType.Shotgun,
                ClipSize = 8,

                Bullets = 8,
                BulletSize = 2f,
                Damage = 15f,
                Force = 5f,
                Spread = 0.3f,
                Recoil = 2f,
                RPM = 80,
                FiringType = FiringType.semi,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 1.0f,
                    Rotation = 0.5f
                },

                DryFireSound = "swb_shotty.empty",
                ShootSound = "css_super90.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_medium.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(0.1f, -0.07f, -0.5f),
                Pos = new Vector3(-5.76f, 6, 3.3f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 50, 0),
                Pos = new Vector3(5, 2, 0)
            };

        }
    }
}
