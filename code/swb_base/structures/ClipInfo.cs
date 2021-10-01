using Sandbox;

namespace SWB_Base
{
    public enum FiringType
    {
        semi,
        auto
    }

    public class ClipInfo : BaseNetworkable
    {
        [Predicted]
        public int Ammo { get; set; } = 10; // Amount of ammo in the clip
        public AmmoType AmmoType { get; set; } = AmmoType.Pistol; // Type of ammo
        public int ClipSize { get; set; } = 10; // Size of the clip
        public float ReloadTime { get; set; } = 1f; // Duration of the reload animation
        public float ReloadEmptyTime { get; set; } = -1f; // Duration of the empty reload animation

        // Shooting
        public int Bullets { get; set; } = 1; // Amount of bullets per shot
        public float BulletSize { get; set; } = 0.1f; // Bullet size
        public float Damage { get; set; } = 5; // Bullet damage
        public float Force { get; set; } = 0.1f; // Bullet force
        public float Spread { get; set; } = 0.1f; // Weapon spread
        public float Recoil { get; set; } = 0.1f; // Weapon recoil
        public int RPM { get; set; } = 200; // Firing speed ( higher is faster )
        public FiringType FiringType { get; set; } = FiringType.semi; // Firing type
        public ScreenShake ScreenShake { get; set; } = new ScreenShake(); // Screenshake per shot

        // Strings
        public string ShootAnim { get; set; } = "fire"; // Shooting animation
        public string ReloadAnim { get; set; } = "reload"; // Reloading animation
        public string ReloadEmptyAnim { get; set; } = "reload_empty"; // Reloading animation when clip is empty
        public string DrawAnim { get; set; } = "deploy"; // Draw animation
        public string DrawEmptyAnim { get; set; } = ""; // Draw animation when there is no ammo
        public string DryFireSound { get; set; } // Firing sound when clip is empty
        public string ShootSound { get; set; } // Firing sound
        public string BulletEjectParticle { get; set; } // Particle that should be used for bullet ejection
        public string MuzzleFlashParticle { get; set; } // Particle that should be used for the muzzle flash

        // Extra
        public InfiniteAmmoType InfiniteAmmo { get; set; } = InfiniteAmmoType.normal; // If the weapon should have infinite ammo
    }
}
