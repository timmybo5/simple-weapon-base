using Sandbox;
using SWB_Base;

namespace SWB_WEAPONS
{
    [Library("swb_fal", Title = "FN FAL")]
    public class FAL : WeaponBase
    {
        public override int Bucket => 3;
        public override HoldType HoldType => HoldType.Rifle;
        public override string HandsModelPath => "weapons/swb/hands/rebel/v_hands_rebel.vmdl";
        public override string ViewModelPath => "weapons/swb/rifles/fal/v_fal.vmdl";
        public override AngPos ViewModelOffset => new()
        {
            Angle = new Angles(0, -5, 0),
            Pos = new Vector3(-5, 0, 0)
        };
        public override string WorldModelPath => "weapons/swb/rifles/fal/w_fal.vmdl";
        public override string Icon => "/swb_weapons/textures/fal.png";
        public override int FOV => 75;
        public override int ZoomFOV => 35;
        public override float WalkAnimationSpeedMod => 0.85f;

        public FAL()
        {
            Primary = new ClipInfo
            {
                Ammo = 20,
                AmmoType = AmmoType.Rifle,
                ClipSize = 20,
                ReloadTime = 2.03f,
                ReloadEmptyTime = 2.67f,

                BulletSize = 4f,
                Damage = 20f,
                Force = 3f,
                Spread = 0.1f,
                Recoil = 0.5f,
                RPM = 600,
                FiringType = FiringType.semi,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 0.5f,
                    Rotation = 0.5f
                },

                DryFireSound = "swb_rifle.empty",
                ShootSound = "fal.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_medium.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(-0.1f, 4.95f, -1f),
                Pos = new Vector3(-5f, -2f, 0.75f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 40, 0),
                Pos = new Vector3(5, 0, 0)
            };
        }
    }
}
