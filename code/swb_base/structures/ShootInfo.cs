using SWB.Shared;

namespace SWB.Base;

public enum FiringType
{
	/// <summary>Single fire</summary>
	semi,
	/// <summary>Automatic fire</summary>
	auto,
	/// <summary>3-Burst fire</summary>
	burst
}

public enum InfiniteAmmoType
{
	/// <summary>No infinite ammo</summary>
	disabled = 0,
	/// <summary>Infinite clip ammo, no need to reload</summary>
	clip = 1,
	/// <summary>Infinite reserve ammo, can always reload</summary>
	reserve = 2
}

[Group( "SWB" )]
[Title( "ShootInfo" )]
public class ShootInfo : Component
{
	/// <summary>Bullet type (Hitscan/Physical)</summary>
	[Property, Group( "Bullets" )] public BulletInfo BulletType { get; set; }

	/// <summary>Type of ammo</summary>
	[Property, Group( "Ammo" )] public string AmmoType { get; set; } = "pistol";

	/// <summary>Amount of ammo in the clip</summary>
	[Property, Group( "Ammo" ), Sync] public int Ammo { get; set; } = 10;

	/// <summary>Size of the clip</summary>
	[Property, Group( "Ammo" )] public int ClipSize { get; set; } = 10;

	/// <summary>If the weapon should have infinite ammo</summary>
	[Property, Group( "Ammo" )] public InfiniteAmmoType InfiniteAmmo { get; set; } = InfiniteAmmoType.disabled;

	// Shooting //

	/// <summary>Amount of bullets per shot</summary>
	[Property, Group( "Bullets" )] public int Bullets { get; set; } = 1;

	/// <summary>Bullet size</summary>
	[Property, Group( "Bullets" )] public float BulletSize { get; set; } = 0.1f;

	/// <summary>Chance the BulletTracerParticle is created (0-1)</summary>
	[Property, Group( "Bullets" )] public float BulletTracerChance { get; set; } = 0.33f;

	/// <summary>Damage per bullet</summary>  
	[Property, Group( "Bullets" )] public float Damage { get; set; } = 5;

	/// <summary>Bullet impact force</summary>
	[Property, Group( "Bullets" )] public float Force { get; set; } = 0.1f;

	/// <summary>Bullet hit flinch</summary>
	[Property, Group( "Bullets" )] public float HitFlinch { get; set; } = 1.25f;

	/// <summary>Weapon spread</summary>
	[Property, Group( "Bullets" )] public float Spread { get; set; } = 0.1f;

	/// <summary>Weapon recoil</summary>
	[Property, Group( "Bullets" )] public float Recoil { get; set; } = 0.1f;

	/// <summary>Rate Per Minute, firing speed (higher is faster)</summary>
	[Property, Group( "Bullets" )] public int RPM { get; set; } = 200;

	/// <summary>Screenshake per shot</summary>
	[Property, Group( "Bullets" )] public ScreenShake ScreenShake { get; set; }

	/// <summary>Weapon firing type</summary>
	[Property, Group( "Bullets" )] public FiringType FiringType { get; set; } = FiringType.semi;

	// Animations //

	/// <summary>Animation used for shooting</summary>
	[Property, Group( "Animations" )] public string ShootAnim { get; set; } = "fire";

	/// <summary>Animation used for shooting the last bullet</summary>
	[Property, Group( "Animations" )] public string ShootEmptyAnim { get; set; } = "";

	/// <summary>Animation used for shooting while aiming</summary>
	[Property, Group( "Animations" )] public string ShootAimedAnim { get; set; }

	// Sounds //

	/// <summary>Firing sound when clip is empty</summary>
	[Property, Group( "Sounds" )] public SoundEvent DryShootSound { get; set; }

	/// <summary>Firing sound</summary>
	[Property, Group( "Sounds" )] public SoundEvent ShootSound { get; set; }

	// Particles //

	/// <summary> View Model particle scale</summary>
	[Property, Title( "View Model Scale" ), Group( "Particles" )] public float VMParticleScale { get; set; } = 1f;

	/// <summary> World Model particle scale</summary>
	[Property, Title( "World Model Scale" ), Group( "Particles" )] public float WMParticleScale { get; set; } = 1f;

	/// <summary>Particle used for bullet ejection</summary>
	[Property, Group( "Particles" )] public PrefabScene BulletEjectParticle { get; set; } = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( "prefabs/particles/shell/shelleject_9mm.prefab" ) );

	/// <summary>Particle used for the muzzle flash</summary>
	[Property, Group( "Particles" )] public PrefabScene MuzzleFlashParticle { get; set; } = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( "prefabs/particles/muzzle/muzzleflash.prefab" ) );

	/// <summary>Particle used for the barrel smoke</summary>
	[Property, Group( "Particles" )] public PrefabScene BarrelSmokeParticle { get; set; } = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( "prefabs/particles/muzzle/barrelsmoke.prefab" ) );

	/// <summary>Particle used for the barrel smoke</summary>
	[Property, Group( "Particles" )] public PrefabScene BulletTracerParticle { get; set; } = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( "prefabs/particles/tracer/tracer.prefab" ) );
}
