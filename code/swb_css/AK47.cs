using Sandbox;
using SWB_Base;

namespace SWB_CSS
{
    [Library("swb_css_ak47", Title = "AK-47")]
    public class AK47 : WeaponBase
    {
        public override int Bucket => 3;
        public override HoldType HoldType => HoldType.Rifle;
        public override string ViewModelPath => "weapons/swb/css/ak47/css_v_rif_ak47.vmdl";
        public override string WorldModelPath => "weapons/swb/css/ak47/css_w_rif_ak47.vmdl";
        public override string Icon => "/swb_css/textures/ui/css_icon_ak47.png";
        public override int FOV => 75;
        public override int ZoomFOV => 35;
        public override float WalkAnimationSpeedMod => 0.85f;

        public AK47()
        {
            General = new WeaponInfo
            {
                ReloadTime = 2.17f
            };

            Primary = new ClipInfo
            {
                Ammo = 30,
                AmmoType = AmmoType.Rifle,
                ClipSize = 30,

                BulletSize = 4f,
                Damage = 15f,
                Force = 3f,
                Spread = 0.1f,
                Recoil = 0.5f,
                RPM = 600,
                FiringType = FiringType.auto,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 0.5f,
                    Rotation = 0.5f
                },

                DryFireSound = "swb_rifle.empty",
                ShootSound = "css_ak47.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_medium.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(-2.3f, -0.05f, 0),
                Pos = new Vector3(-6.085f, 0, 2.2f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 40, 0),
                Pos = new Vector3(5, 0, 0)
            };
        }
    }
}
