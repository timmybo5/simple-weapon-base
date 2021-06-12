using Sandbox;
using SWB_Base;

namespace SWB_CSS
{
	[Library( "swb_css_awp", Title = "AWP" )]
	public class AWP : WeaponBaseSniper
	{
		public override int Bucket => 5;
		public override HoldType HoldType => HoldType.Rifle;
		public override string ViewModelPath => "weapons/css_awp/css_v_awp.vmdl";
		public override string WorldModelPath => "weapons/css_awp/css_w_awp.vmdl";
		public override int FOV => 75;
		public override int ZoomFOV => 75;
		public override float WalkAnimationSpeedMod => 0.8f;
		public override bool DrawCrosshair => true;
		public override float AimSensitivity => 0.25f;

		public override string LensTexture => "/swb_base/textures/scopes/swb_lens_hunter.png";
		public override string ScopeTexture => "/swb_base/textures/scopes/swb_scope_hunter.png";
		public override float ZoomAmount => 15f;

		public AWP()
		{
			Primary = new ClipInfo
			{
				Ammo = 10,
				AmmoType = AmmoType.Sniper,
				ClipSize = 10,
				ReloadTime = 3.67f,

				BulletSize = 4f,
				Damage = 100f,
				Force = 7f,
				Spread = 1f,
				Recoil = 1f,
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
				MuzzleFlashParticle = "particles/pistol_muzzleflash.vpcf",
				
				InfiniteAmmo = InfiniteAmmoType.reserve
			};

			ZoomAnimData = new AngPos
			{
				Angle = new Angles( 0f, 0.5f, -2f ),
				Pos = new Vector3( -5.5f, -2f, 4f )
			};

			RunAnimData = new AngPos
			{
				Angle = new Angles( 10, 40, 0 ),
				Pos = new Vector3( 5, 0, 0 )
			};
		}
	}
}
