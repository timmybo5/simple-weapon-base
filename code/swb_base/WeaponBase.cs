using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base
{
	public enum HoldType
	{
		Pistol = 1,
		Rifle = 2,
		Shotgun = 3
	}

	public partial class WeaponBase : Sandbox.BaseWeapon
	{
		public virtual int Bucket => 1;
		public virtual int BucketWeight => 100;
		public virtual bool DrawCrosshair => true;
		public virtual bool DropWeaponOnDeath => true;
		// Some weapons have looping idle animations -> force spam another animation to "freeze" it
		public virtual string FreezeViewModelOnZoom => null;
		public virtual int FOV => 65;
		public virtual int ZoomFOV => 65;
		// Set to -1 to disable tucking
		public virtual float TuckRange => 30;
		public virtual HoldType HoldType => HoldType.Pistol;
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
		public virtual string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
		public virtual float WalkAnimationSpeedMod => 1;

		public List<AnimatedAction> AnimatedActions { get; set; }
		public AngPos ZoomAnimData { get; set; }
		public AngPos RunAnimData { get; set; }

		[Net]
		public ClipInfo Primary { get; set; } = new ClipInfo();

		[Net]
		public ClipInfo Secondary { get; set; } = null;

		[Net, Predicted]
		public TimeSince TimeSinceReload { get; set; }

		[Net, Predicted]
		public bool IsReloading { get; set; }

		[Net, Predicted]
		public bool IsZooming { get; set; }

		[Net, Predicted]
		public bool IsRunning { get; set; }

		[Net, Predicted]
		public bool IsAnimating { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceDeployed { get; set; }

		public PickupTrigger PickupTrigger { get; protected set; }

		private bool doRecoil = false;

		public int AvailableAmmo()
		{
			var owner = Owner as PlayerBase;
			if ( owner == null ) return 0;

			// Show clipsize as the available ammo
			if (Primary.InfiniteAmmo == InfiniteAmmoType.reserve)
				return Primary.ClipSize;

			return owner.AmmoCount( Primary.AmmoType );
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );
			
			TimeSinceDeployed = 0;

			// Animated activity status will reset when weapon is switched out
			if ( AnimatedActions != null )
			{
				for ( int i = 0; i < AnimatedActions.Count; i++ )
				{
					if ( AnimatedActions[i].isToggled )
						AnimatedActions[i].HandleOnDeploy( this );
				}
			}

		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );

			PickupTrigger = new PickupTrigger();
			PickupTrigger.Parent = this;
			PickupTrigger.Position = Position;
		}

		public override void Reload()
		{
			if ( IsReloading || IsAnimating )
				return;

			if ( Primary.Ammo >= Primary.ClipSize )
				return;

			TimeSinceReload = 0;

			if ( Owner is PlayerBase player )
			{
				if ( player.AmmoCount( Primary.AmmoType ) <= 0 && Primary.InfiniteAmmo != InfiniteAmmoType.reserve )
					return;
			}

			IsReloading = true;

			// Player anim
			(Owner as AnimEntity).SetAnimBool( "b_reload", true );

			StartReloadEffects();
		}

		public virtual float GetTuckDist()
		{

			if ( TuckRange == -1 )
				return -1;

			var player = Owner as Player;
			if ( player == null ) return -1;

			var pos = player.EyePos;
			var rot = player.Rotation;
			var forward = Owner.EyeRot.Forward;
			var trace = Trace.Ray( pos, pos + forward * TuckRange )
				.Ignore( this )
				.Ignore( player )
				.Run();

			if ( trace.Entity == null )
				return -1;

			return trace.Distance;
		}

		public bool ShouldTuck( float dist = -1 )
		{
			return dist != -1 || GetTuckDist() != -1;
		}

		// Shared
		public override void Simulate( Client owner )
		{

			if ( IsAnimating ) return;

			// Handle custom animation actions
			if (AnimatedActions != null && !IsReloading)
			{
				for (int i=0; i< AnimatedActions.Count; i++ )
				{
					if ( AnimatedActions[i].Handle( owner, this ) )
						return;
				}
			}

			IsRunning = owner.Input.Down( InputButton.Run ) && Owner.Velocity.Length >= 300;

			if ( Secondary == null && !(this is WeaponBaseMelee) )
				IsZooming = owner.Input.Down( InputButton.Attack2 ) && !IsRunning && !IsReloading;

			if ( TimeSinceDeployed < 0.6f )
				return;

			if ( !IsReloading )
			{
				base.Simulate( owner );
			}

			if ( IsReloading && TimeSinceReload > Primary.ReloadTime )
			{
				OnReloadFinish();
			}
		}

		public virtual void OnReloadFinish()
		{
			IsReloading = false;

			if ( Primary.InfiniteAmmo == InfiniteAmmoType.reserve )
			{
				Primary.Ammo = Primary.ClipSize;
				return;
			}

			if ( Owner is PlayerBase player )
			{
				var ammo = player.TakeAmmo( Primary.AmmoType, Primary.ClipSize - Primary.Ammo );
				if ( ammo == 0 )
					return;

				Primary.Ammo += ammo;
			}
		}

		[ClientRpc]
		public virtual void StartReloadEffects()
		{
			if ( Primary.ReloadAnim != null )
				ViewModelEntity?.SetAnimBool( Primary.ReloadAnim, true );

			// TODO - player third person model reload
		}

		private bool CanAttack(ClipInfo clipInfo, TimeSince lastAttackTime, InputButton inputButton)
		{
			if ( IsAnimating ) return false;
			if ( clipInfo == null || !Owner.IsValid() || !Input.Down( inputButton ) ) return false;
			if ( clipInfo.RPM <= 0 ) return true;

			return lastAttackTime > (60f / clipInfo.RPM);
		}

		public override bool CanPrimaryAttack()
		{
			return CanAttack( Primary, TimeSincePrimaryAttack, InputButton.Attack1 );
		}

		public override bool CanSecondaryAttack()
		{
			return CanAttack( Secondary, TimeSinceSecondaryAttack, InputButton.Attack2 );
		}

		private void Attack( ClipInfo clipInfo )
		{
			if ( IsRunning || ShouldTuck() ) return;

			if ( clipInfo.FiringType == FiringType.semi && !Owner.Input.Pressed( InputButton.Attack1 ) )
				return;

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( !TakeAmmo( 1 ) )
			{
				DryFire( clipInfo );
				return;
			}

			// Player anim
			(Owner as AnimEntity).SetAnimBool( "b_attack", true );

			// Tell the clients to play the shoot effects
			ShootEffects( clipInfo );

			if ( clipInfo.ShootSound != null )
				PlaySound( clipInfo.ShootSound );

			// Shoot the bullets
			float realSpread;

			if ( this is WeaponBaseShotty )
			{
				realSpread = clipInfo.Spread;
			} else
			{
				realSpread = IsZooming ? clipInfo.Spread / 4 : clipInfo.Spread;
			}

			for ( int i = 0; i < clipInfo.Bullets; i++ )
			{
				ShootBullet( realSpread, clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize );
			}

			// Recoil
			doRecoil = true;
		}

		public override void AttackPrimary()
		{
			Attack( Primary );
		}

		public override void AttackSecondary()
		{ 
			if (Secondary != null)
			{
				Attack( Secondary );
				return;
			}

			// Extra functionality
		}

		public override void BuildInput( InputBuilder input )
		{
			if ( doRecoil )
			{
				doRecoil = false;
				// Less recoil when zooming
				var recoilAngles = new Angles( IsZooming ? -Primary.Recoil*0.4f : -Primary.Recoil, 0, 0 );
				input.ViewAngles += recoilAngles;
			}
		}

		[ClientRpc]
		protected virtual void ShootEffects( ClipInfo clipInfo )
		{
			Host.AssertClient();

			//Log.Info( "[DEBUG] Multiplayer Error: " + clipInfo.ToString() + " -  " + clipInfo.MuzzleFlashParticle + " - " + EffectEntity.ToString() );
			Log.Info( "[DEBUG] Multiplayer Error: " + clipInfo.ToString() );

			if ( !string.IsNullOrEmpty( clipInfo.MuzzleFlashParticle ) )
				Particles.Create( clipInfo.MuzzleFlashParticle, EffectEntity, "muzzle" );

			if ( !string.IsNullOrEmpty( clipInfo.BulletEjectParticle ) )
				Particles.Create( clipInfo.BulletEjectParticle, EffectEntity, "ejection_point" );

			if ( IsLocalPawn )
			{
				var screenShake = clipInfo.ScreenShake;
				new Sandbox.ScreenShake.Perlin( screenShake.Length, screenShake.Speed, screenShake.Size, screenShake.Rotation );
			}

			// Weapon anim
			if ( clipInfo.ShootAnim != null )
				ViewModelEntity?.SetAnimBool( clipInfo.ShootAnim, true );

			CrosshairPanel?.OnEvent( "fire" );
		}

		/// Shoot a single bullet
		public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
		{

			// Spread
			var forward = Owner.EyeRot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;
			
			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * 5000, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				//
				// We turn predictiuon off for this, so any exploding effects don't get culled etc
				//
				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		public bool TakeAmmo( int amount )
		{
			if ( Primary.InfiniteAmmo == InfiniteAmmoType.clip )
				return true;

			if ( Primary.Ammo < amount )
				return false;

			Primary.Ammo -= amount;
			return true;
		}

		[ClientRpc]
		public virtual void DryFire( ClipInfo clipInfo )
		{
			if ( !string.IsNullOrEmpty( clipInfo.DryFireSound ) )
				PlaySound( clipInfo.DryFireSound );
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new ViewModelBase(this);
			//ViewModelEntity.Position = Position; // --> Does not seem to do anything
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void CreateHudElements()
		{
			if ( Local.Hud == null ) return;

			if ( DrawCrosshair )
			{
				CrosshairPanel = new Crosshair();
				CrosshairPanel.Parent = Local.Hud;
				CrosshairPanel.AddClass( ClassInfo.Name );
			}
		}

		public bool IsUsable()
		{
			if ( Primary.Ammo > 0 ) return true;
			return AvailableAmmo() > 0;
		}

		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );

			if ( PickupTrigger.IsValid() )
			{
				PickupTrigger.EnableTouch = false;
			}
		}

		public override void OnCarryDrop( Entity dropper )
		{

			if ( !DropWeaponOnDeath )
			{
				this.Delete();
				return;
			}

			base.OnCarryDrop( dropper );

			if ( PickupTrigger.IsValid() )
			{
				PickupTrigger.EnableTouch = true;
			}
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", (int)HoldType );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		[ClientRpc]
		public virtual void SendWeaponAnim( string anim, bool value = true )
		{
			ViewModelEntity?.SetAnimBool( anim, value );
		}

	}
}
