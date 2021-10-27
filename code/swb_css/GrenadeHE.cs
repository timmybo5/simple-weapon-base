using System;
using System.Collections.Generic;
using Sandbox;
using SWB_Base;

namespace SWB_CSS
{
    [Library("swb_css_grenade_he", Title = "HE Grenade")]
    public class GrenadeHE : WeaponBaseEntity
    {
        public override int Bucket => 0;
        public override HoldType HoldType => HoldType.Pistol;
        public override string ViewModelPath => "weapons/swb/css/grenade_he/css_v_grenade_he.vmdl";
        public override string WorldModelPath => "weapons/swb/css/grenade_he/css_w_grenade_he.vmdl";
        public override string Icon => "/swb_css/textures/ui/css_icon_grenade.png";
        public override int FOV => 75;

        public override Func<ClipInfo, bool, FiredEntity> CreateEntity => CreateGrenadeEntity;
        public override string EntityModel => "weapons/swb/css/grenade_he/css_w_grenade_he_thrown.vmdl";
        public override Vector3 EntityVelocity => new Vector3(0, 25, 50);
        public override Angles EntityAngles => new Angles(0, 0, -45);
        public override bool IsSticky => true;
        public override float PrimaryEntitySpeed => 17;
        public override float SecondaryEntitySpeed => 10;
        public override float PrimaryDelay => 1.27f;
        public override float SecondaryDelay => 1.27f;

        public GrenadeHE()
        {
            UISettings = new UISettings
            {
                ShowCrosshairLines = false,
                ShowFireMode = false,
            };

            Primary = new ClipInfo
            {
                Ammo = -1,
                ClipSize = -1,
                AmmoType = AmmoType.Grenade,
                FiringType = FiringType.semi,
                RPM = 50,
            };
            Secondary = Primary;
        }

        private FiredEntity CreateGrenadeEntity(ClipInfo clipInfo, bool isPrimary)
        {
            var grenade = new Grenade();
            grenade.Weapon = this;
            grenade.ExplosionDelay = 3f;
            grenade.ExplosionRadius = 300f;
            grenade.ExplosionDamage = 200f;
            grenade.ExplosionForce = 350f;
            grenade.BounceSound = "css_grenade_he.bounce";
            grenade.ExplosionSounds = new List<string>
            {
                "css_grenade_he.explode"
            };
            grenade.ExplosionEffect = "weapons/swb/css/grenade_he/particles/grenade_he_explosion.vpcf";
            grenade.ExplosionShake = new ScreenShake
            {
                Length = 1f,
                Speed = 5f,
                Size = 5f,
                Rotation = 2f,
            };

            return grenade;
        }
    }
}
