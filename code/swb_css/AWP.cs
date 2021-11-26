using Sandbox;
using SWB_Base;

namespace SWB_CSS
{
    [Library("swb_css_awp", Title = "AWP")]
    public class AWP : WeaponBaseSniper
    {
        public override int Bucket => 5;
        public override HoldType HoldType => HoldType.Rifle;
        public override string ViewModelPath => "weapons/swb/css/awp/css_v_awp.vmdl";
        public override string WorldModelPath => "weapons/swb/css/awp/css_w_awp.vmdl";
        public override string Icon => "/swb_css/textures/ui/css_icon_awp.png";
        public override int FOV => 75;
        public override int ZoomFOV => 75;
        public override float WalkAnimationSpeedMod => 0.8f;
        public override float AimSensitivity => 0.25f;

        public override string LensTexture => "/materials/swb/scopes/swb_lens_hunter.png";
        public override string ScopeTexture => "/materials/swb/scopes/swb_scope_hunter.png";
        public override string ZoomInSound => "swb_sniper.zoom_in";
        public override float ZoomAmount => 15f;
        public override bool UseRenderTarget => false;

        public AWP()
        {
            UISettings = new UISettings
            {
                ShowCrosshair = false
            };

            General = new WeaponInfo
            {
                ReloadTime = 3.67f
            };

            Primary = new ClipInfo
            {
                Ammo = 10,
                AmmoType = AmmoType.Sniper,
                ClipSize = 10,

                BulletSize = 5f,
                Damage = 100f,
                Force = 7f,
                Spread = 0.75f,
                Recoil = 2f,
                RPM = 50,
                FiringType = FiringType.semi,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 1f,
                    Rotation = 0.5f
                },

                DryFireSound = "swb_sniper.empty",
                ShootSound = "css_awp.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_large.vpcf",
                BulletTracerParticle = "particles/swb/tracer/tracer_large.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(0f, 0.5f, -2f),
                Pos = new Vector3(-5.5f, 4f, -2f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 40, 0),
                Pos = new Vector3(5, 0, 0)
            };
        }
    }
}
