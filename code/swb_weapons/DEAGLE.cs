using Sandbox;
using SWB_Base;

namespace SWB_WEAPONS
{
    [Library("swb_deagle", Title = "Desert Eagle")]
    public class DEAGLE : WeaponBase
    {
        public override int Bucket => 1;
        public override HoldType HoldType => HoldType.Pistol;
        public override string HandsModelPath => "weapons/swb/hands/rebel/v_hands_rebel.vmdl";
        public override string ViewModelPath => "weapons/swb/pistols/deagle/v_deagle.vmdl";
        public override AngPos ViewModelOffset => new()
        {
            Angle = new Angles(0, -5, 0),
            Pos = new Vector3(-5, 0, 0)
        };
        public override string WorldModelPath => "weapons/swb/pistols/deagle/w_deagle.vmdl";
        public override string Icon => "/swb_weapons/textures/deagle.png";
        public override int FOV => 75;
        public override int ZoomFOV => 60;

        public DEAGLE()
        {
            Primary = new ClipInfo
            {
                Ammo = 7,
                AmmoType = AmmoType.Revolver,
                ClipSize = 7,
                ReloadTime = 1.8f,
                ReloadEmptyTime = 2.9f,

                BulletSize = 6f,
                Damage = 50f,
                Force = 5f,
                Spread = 0.06f,
                Recoil = 1f,
                RPM = 300,
                FiringType = FiringType.semi,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 1.0f,
                    Rotation = 0.5f
                },

                DryFireSound = "swb_pistol.empty",
                ShootSound = "deagle.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_medium.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(0.25f, 4.95f, -0.4f),
                Pos = new Vector3(-5f, -2f, 2.45f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(-30, 0, 0),
                Pos = new Vector3(0, -3, -8)
            };
        }
    }
}
