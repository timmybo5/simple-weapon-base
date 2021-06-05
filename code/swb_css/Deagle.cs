using Sandbox;
using SWB_Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWB_CSS
{
	[Library( "swb_css_deagle", Title = "Desert Eagle" )]
	partial class Deagle : WeaponBase
	{
		public override int Bucket => 1;
		public override HoldType HoldType => HoldType.Pistol;
		public override string ViewModelPath => "weapons/css_deagle/css_v_pist_deagle.vmdl";
		public override string WorldModelPath => "weapons/css_deagle/css_w_pist_deagle.vmdl";
		public override int FOV => 75;
		public override int ZoomFOV => 60;

		public Deagle()
		{
			Primary = new ClipInfo
			{
				Ammo = 7,
				AmmoType = AmmoType.Revolver,
				ClipSize = 7,
				ReloadTime = 2.17f,

				BulletSize = 6f,
				Damage = 50f,
				Force = 5f,
				Spread = 0.06f,
				Recoil = 1f,
				RPM = 300,
				FiringType = FiringType.semi,
				ScreenShake = new ScreenShake
				{
					Length = 0.5f,
					Speed = 4.0f,
					Size = 1.0f,
					Rotation = 0.5f
				},

				//DryFireSound = "rust_pistol.dryfire",
				ShootSound = "css_deagle.fire",

				BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
				MuzzleFlashParticle = "particles/pistol_muzzleflash.vpcf",
				
				InfiniteAmmo = InfiniteAmmoType.reserve
			};

			ZoomAnimData = new AngPos
			{
				Angle = new Angles( 0, -0.1f, 0 ),
				Pos = new Vector3( -5.125f, 2.67f, 0 )
			};

			RunAnimData = new AngPos
			{
				Angle = new Angles( -40, 0, 0 ),
				Pos = new Vector3( 0, -8, 0 )
			};
		}
	}
}
