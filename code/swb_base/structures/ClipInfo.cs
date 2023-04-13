using Sandbox;

namespace SWB_Base;

public enum FiringType
{
    /// <summary>Single fire</summary>
    semi,
    /// <summary>Automatic fire</summary>
    auto,
    /// <summary>3-Burst fire</summary>
    burst
}

public partial class ClipInfo : BaseNetworkable
{
    /// <summary>Amount of ammo in the clip</summary>
    [Net] public int Ammo { get; set; } = 10;

    /// <summary>Type of ammo</summary>
    [Net] public AmmoType AmmoType { get; set; } = AmmoTypes.Pistol;

    /// <summary>Size of the clip</summary>
    [Net] public int ClipSize { get; set; } = 10;

    // Shooting //

    /// <summary>Amount of bullets per shot</summary>
    [Net] public int Bullets { get; set; } = 1;

    /// <summary>Bullet size</summary>
    [Net] public float BulletSize { get; set; } = 0.1f;

    /// <summary>Bullet type (Hitscan/Physical)</summary>
    [Net] public BulletBase BulletType { get; set; } = new HitScanBullet();

    /// <summary>Damage per bullet</summary>  
    [Net] public float Damage { get; set; } = 5;

    /// <summary>Bullet impact force</summary>
    [Net] public float Force { get; set; } = 0.1f;

    /// <summary>Bullet hit flinch</summary>
    [Net] public float HitFlinch { get; set; } = 1.25f;

    /// <summary>Weapon spread</summary>
    [Net] public float Spread { get; set; } = 0.1f;

    /// <summary>Weapon recoil</summary>
    [Net] public float Recoil { get; set; } = 0.1f;

    /// <summary>Rate Per Minute, firing speed (higher is faster)</summary>
    [Net] public int RPM { get; set; } = 200;

    /// <summary>Weapon firing type</summary>
    [Net] public FiringType FiringType { get; set; } = FiringType.semi;

    /// <summary>Screenshake per shot</summary>
    [Net] public ScreenShake ScreenShake { get; set; }

    // Strings //

    /// <summary>Animation used for shooting</summary>
    [Net] public string ShootAnim { get; set; } = "fire";

    /// <summary>Animation used for shooting the last bullet</summary>
    [Net] public string ShootEmptyAnim { get; set; } = "";

    /// <summary>Animation used for shooting while zoooming</summary>
    [Net] public string ShootZoomedAnim { get; set; }

    /// <summary>Firing sound when clip is empty</summary>
    [Net] public string DryFireSound { get; set; }

    /// <summary>Firing sound</summary>
    [Net] public string ShootSound { get; set; }

    /// <summary>Particle used for bullet ejection</summary>
    [Net] public string BulletEjectParticle { get; set; }

    /// <summary>Particle used for the muzzle flash</summary>
    [Net] public string MuzzleFlashParticle { get; set; }

    /// <summary>Particle used for the barrel smoke</summary>
    [Net] public string BarrelSmokeParticle { get; set; } = "particles/swb/muzzle/barrel_smoke.vpcf";

    /// <summary>Particle used for the barrel smoke</summary>
    [Net] public string BulletTracerParticle { get; set; } = "particles/swb/tracer/tracer_medium.vpcf";

    // Extra // 

    /// <summary>If the weapon should have infinite ammo</summary>
    [Net] public InfiniteAmmoType InfiniteAmmo { get; set; }
}
