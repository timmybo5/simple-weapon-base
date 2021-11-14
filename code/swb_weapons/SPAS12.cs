using Sandbox;
using SWB_Base;

namespace SWB_WEAPONS
{
    [Library("swb_css_spas12", Title = "SPAS-12")]
    public class SPAS12 : WeaponBaseShotty
    {
        public override int Bucket => 2;
        public override HoldType HoldType => HoldType.Shotgun;
        public override string HandsModelPath => "weapons/swb/hands/swat/v_hands_swat.vmdl";
        public override string ViewModelPath => "weapons/swb/shotguns/spas/v_spas12.vmdl";
        public override string WorldModelPath => "weapons/swb/shotguns/spas/w_spas12.vmdl";
        public override string Icon => "/swb_weapons/textures/spas12.png";
        public override int FOV => 75;
        public override int ZoomFOV => 45;
        public override float WalkAnimationSpeedMod => 0.9f;

        public override float ShellReloadTimeStart => 0.4f;
        public override float ShellReloadTimeInsert => 0.65f;
        public override float ShellEjectDelay => 0.5f;

        public SPAS12()
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
                ShootSound = "spas12.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_medium.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(0.08f, -0.06f, 0f),
                Pos = new Vector3(-5f, 0f, 1.95f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 50, 0),
                Pos = new Vector3(10, -2, 0)
            };

        }
    }
}
