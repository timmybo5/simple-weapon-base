﻿using Sandbox;
using SWB_Base;
using System;
using System.Collections.Generic;

namespace SWB_EXPLOSIVES
{
	[Library( "swb_explosives_rpg7", Title = "RPG-7" )]
	public class RPG7 : WeaponBaseEntity
	{
		public override int Bucket => 4;
		public override HoldType HoldType => HoldType.Rifle;
		public override string ViewModelPath => "weapons/swb/explosives/rpg-7/swb_v_rpg7.vmdl";
		public override string WorldModelPath => "weapons/swb/explosives/rpg-7/swb_w_rpg7.vmdl";
		public override string Icon => "/swb_explosives/textures/ui/icon_rpg7.png";
		public override int FOV => 40;
		public override int ZoomFOV => 30;
		public override float WalkAnimationSpeedMod => 0.7f;

		public override Func<ClipInfo, bool, FiredEntity> CreateEntity => CreateRocketEntity;
		public override string EntityModel => "weapons/swb/explosives/rpg-7/swb_w_rpg7_rocket_he.vmdl";
		public override Vector3 EntityVelocity => new Vector3( 0, 0, 3000 );
		public override Angles EntityAngles => new Angles( 0, 180, 0 );
		public override Vector3 EntitySpawnOffset => new Vector3( 10, 10, 10 );
		public override float PrimaryEntitySpeed => 30;

		public RPG7()
		{
			Primary = new ClipInfo
			{
				Ammo = 1,
				AmmoType = AmmoType.RPG,
				ClipSize = 1,
				ReloadTime = 4f,

				BulletSize = 5f,
				Damage = 15f,
				Force = 4f,
				Spread = 0.2f,
				Recoil = 0.7f,
				RPM = 800,
				FiringType = FiringType.semi,
				ScreenShake = new ScreenShake
				{
					Length = 0.5f,
					Speed = 4.0f,
					Size = 1.0f,
					Rotation = 1f
				},

				DrawEmptyAnim = "deploy_empty",
				DryFireSound = "swb_lmg.empty",
				ShootSound = "swb_explosives_rpg7.fire",

				MuzzleFlashParticle = "particles/swb/smoke/swb_smokepuff_1.vpcf",

				InfiniteAmmo = InfiniteAmmoType.reserve
			};

			ZoomAnimData = new AngPos
			{
				Angle = new Angles( -2.85f, -0.34f, 6f ),
				Pos = new Vector3( -5.245f, -1.05f, 0f )
			};

			RunAnimData = new AngPos
			{
				Angle = new Angles( 15f, 0f, 0f ),
				Pos = new Vector3( 0f, 2f, 2f )
			};
		}

		private FiredEntity CreateRocketEntity( ClipInfo clipInfo, bool isPrimary )
		{
			var rocket = new Rocket();
			rocket.Weapon = this;
			rocket.ExplosionDelay = 3f;
			rocket.ExplosionRadius = 400f;
			rocket.ExplosionDamage = 300f;
			rocket.ExplosionForce = 500f;
			rocket.ExplosionSounds = new List<string>
			{
				"swb_explosion_1",
				"swb_explosion_2",
				"swb_explosion_3",
				"swb_explosion_4",
				"swb_explosion_5"
			};
			rocket.ExplosionEffect = "particles/explosion_fireball.vpcf";
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