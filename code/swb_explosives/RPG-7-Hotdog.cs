using System;
using System.Collections.Generic;
using Sandbox;
using SWB_Base;

namespace SWB_EXPLOSIVES
{
    [Library("swb_explosives_rpg7_hotdog", Title = "RPG-7-Hotdog")]
    public class RPG7Hotdog : RPG7
    {
        public override string ViewModelPath => "weapons/swb/explosives/rpg-7-hotdog/swb_v_rpg7_hotdog.vmdl";
        public override string WorldModelPath => "weapons/swb/explosives/rpg-7-hotdog/swb_w_rpg7_hotdog.vmdl";
        public override string Icon => "/swb_explosives/textures/ui/icon_rpg7.png";

        public override Func<ClipInfo, bool, FiredEntity> CreateEntity => CreateHotdogEntity;
        public override Vector3 EntityVelocity => new Vector3(0, 0, 50);
        public override Angles EntityAngles => new Angles(0, 180, 0);
        public override string EntityModel => "models/citizen_props/hotdog01.vmdl";

        public RPG7Hotdog() : base() { }

        private FiredEntity CreateHotdogEntity(ClipInfo clipInfo, bool isPrimary)
        {
            var rocket = new Rocket();
            rocket.Weapon = this;
            rocket.ExplosionDelay = 3f;
            rocket.ExplosionRadius = 400f;
            rocket.ExplosionDamage = 300f;
            rocket.ExplosionForce = 500f;
            rocket.ExplosionSounds = new List<string>
            {
                "css_grenade_he.explode"
            };
            rocket.ExplosionEffect = "particles/swb/explosion/hotdogs.vpcf";
            rocket.RocketSound = "swb_explosives_rpg7.rocketloop";
            rocket.RocketEffects = new List<string>
            {
                "particles/swb/smoke/swb_smoketrail_1.vpcf",
                "particles/swb/fire/swb_fire_rocket_1.vpcf"
            };
            rocket.ExplosionShake = new ScreenShake
            {
                Length = 1f,
                Speed = 5f,
                Size = 7f,
                Rotation = 3f
            };

            return rocket;
        }
    }
}
