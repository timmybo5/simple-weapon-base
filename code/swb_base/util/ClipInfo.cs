using Sandbox;

namespace SWB_Base
{
	public enum FiringType
	{
		semi,
		auto
	}

	public class ClipInfo
	{
		[Predicted]
		public int Ammo { get; set; } = 10;
		public AmmoType AmmoType { get; set; } = AmmoType.Pistol;
		public int ClipSize { get; set; } = 10;
		public float ReloadTime { get; set; } = 1f;

		// Shoot
		public int Bullets { get; set; } = 1;
		public float BulletSize { get; set; } = 0.1f;
		public float Damage { get; set; } = 5;
		public float Force { get; set; } = 0.1f;
		public float Spread { get; set; } = 0.1f;
		public float Recoil { get; set; } = 0.1f;
		public int RPM { get; set; } = 200;
		public FiringType FiringType { get; set; } = FiringType.semi;
		public ScreenShake ScreenShake { get; set; } = new ScreenShake();

		// Strings
		public string ShootAnim { get; set; } = "fire";
		public string ReloadAnim { get; set; } = "reload";
		public string DryFireSound { get; set; }
		public string ShootSound { get; set; }
		public string BulletEjectParticle { get; set; }
		public string MuzzleFlashParticle { get; set; }

		// Extra
		public InfiniteAmmoType InfiniteAmmo { get; set; } = InfiniteAmmoType.normal;
	}
}
