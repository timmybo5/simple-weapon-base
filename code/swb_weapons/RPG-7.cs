using System;
using System.Collections.Generic;
using Sandbox;
using SWB_Base;

namespace SWB_WEAPONS;

[Library("swb_rpg7", Title = "RPG-7")]
public class RPG7 : WeaponBaseEntity
{
    public override int Bucket => 4;
    public override HoldType HoldType => HoldType.Rifle;
    public override string ViewModelPath => "weapons/swb/explosives/rpg-7/swb_v_rpg7.vmdl";
    public override AngPos ViewModelOffset => new()
    {
        Angle = new Angles(0, 0, 0),
        Pos = new Vector3(0, -7, 0)
    };
    public override string WorldModelPath => "weapons/swb/explosives/rpg-7/swb_w_rpg7.vmdl";
    public override string Icon => "code/swb_weapons/textures/rpg7.png";
    public override bool BulletCocking => false;

    public override Func<ClipInfo, bool, FiredEntity> CreateEntity => CreateRocketEntity;
    public override string EntityModel => "weapons/swb/explosives/rpg-7/swb_w_rpg7_rocket_he.vmdl";
    public override Vector3 EntityVelocity => new Vector3(0, 0, 3000);
    public override Angles EntityAngles => new Angles(0, 180, 0);
    public override Vector3 EntitySpawnOffset => new Vector3(0, 5, 42);
    public override float PrimaryEntitySpeed => 30;
    public override bool UseGravity => false;

    public RPG7()
    {
        UISettings = new UISettings
        {
            ShowCrosshair = false,
            ShowFireMode = false,
        };

        General = new WeaponInfo
        {
            FOV = 75,
            WalkAnimationSpeedMod = 0.7f,

            ReloadTime = 4f,
            DrawEmptyAnim = "deploy_empty",
        };

        Primary = new ClipInfo
        {
            Ammo = 1,
            AmmoType = AmmoTypes.RPG,
            ClipSize = 1,

            BulletSize = 5f,
            Damage = 15f,
            Force = 4f,
            Spread = 0.7f,
            Recoil = 1.2f,
            RPM = 750,
            FiringType = FiringType.semi,
            ScreenShake = new ScreenShake
            {
                Length = 0.13f,
                Delay = 0.03f,
                Size = 1.94f,
                Rotation = 0.22f
            },

            DryFireSound = "swb_lmg.empty",
            ShootSound = "swb_explosives_rpg7.fire",

            MuzzleFlashParticle = "particles/swb/smoke/swb_smokepuff_1.vpcf",

            InfiniteAmmo = InfiniteAmmoType.reserve
        };

        ZoomAnimData = new AngPos
        {
            Angle = new Angles(-3.1f, -0.23f, 6f),
            Pos = new Vector3(-5.103f, -4.504f, -1.052f)
        };

        RunAnimData = new AngPos
        {
            Angle = new Angles(15f, 0f, 0f),
            Pos = new Vector3(0f, 2f, 2f)
        };
    }

    private FiredEntity CreateRocketEntity(ClipInfo clipInfo, bool isPrimary)
    {
        var rocket = new Rocket();
        rocket.Weapon = this;
        rocket.ExplosionDelay = 3f;
        rocket.ExplosionRadius = 320f;
        rocket.ExplosionDamage = 350f;
        rocket.ExplosionForce = 500f;
        rocket.Inaccuracy = 70;
        rocket.ExplosionSound = "swb_explosion_random";
        rocket.ExplosionEffect = "weapons/swb/explosives/rpg-7/temp_particles/grenade_he_explosion.vpcf";
        rocket.RocketSound = "swb_explosives_rpg7.rocketloop";
        rocket.RocketEffects = new List<string>
        {
            "particles/swb/smoke/swb_smoketrail_1.vpcf",
            "particles/swb/fire/swb_fire_rocket_1.vpcf"
        };
        rocket.ExplosionShake = new ScreenShake
        {
            Length = 0.5f,
            Delay = 0.05f,
            Size = 10f,
            Rotation = 1f
        };

        return rocket;
    }
}
