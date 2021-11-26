using Sandbox;
using SWB_Base;

namespace SWB_CSS
{
    [Library("swb_css_mac10", Title = "MAC-10")]
    public class MAC10 : WeaponBase
    {
        public override int Bucket => 2;
        public override HoldType HoldType => HoldType.Pistol;
        public override string ViewModelPath => "weapons/swb/css/mac10/css_v_smg_mac10.vmdl";
        public override string WorldModelPath => "weapons/swb/css/mac10/css_w_smg_mac10.vmdl";
        public override string Icon => "/swb_css/textures/ui/css_icon_mac10.png";
        public override int FOV => 75;
        public override int ZoomFOV => 75;
        public override float WalkAnimationSpeedMod => 0.85f;

        public MAC10()
        {
            General = new WeaponInfo
            {
                ReloadTime = 3.14f
            };

            Primary = new ClipInfo
            {
                Ammo = 32,
                AmmoType = AmmoType.SMG,
                ClipSize = 32,

                BulletSize = 2f,
                Damage = 12f,
                Force = 3f,
                Spread = 0.2f,
                Recoil = 0.9f,
                RPM = 1090,
                FiringType = FiringType.auto,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 0.3f,
                    Rotation = 0.4f
                },

                DryFireSound = "swb_smg.empty",
                ShootSound = "css_mac10.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_small.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(-0.7f, -5.4f, -7f),
                Pos = new Vector3(-6.94f, 0f, 2.9f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 40, 0),
                Pos = new Vector3(5, 0, 0)
            };
        }
    }
}
