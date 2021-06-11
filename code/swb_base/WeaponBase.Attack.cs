using Sandbox;
using System.Collections.Generic;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base
{
	public partial class WeaponBase
	{
		private bool CanAttack( ClipInfo clipInfo, TimeSince lastAttackTime, InputButton inputButton )
		{
			if ( IsAnimating ) return false;
			if ( clipInfo == null || !Owner.IsValid() || !Input.Down( inputButton ) ) return false;
			if ( clipInfo.RPM <= 0 ) return true;

			return lastAttackTime > (60f / clipInfo.RPM);
		}

		public virtual bool CanPrimaryAttack()
		{
			return CanAttack( Primary, TimeSincePrimaryAttack, InputButton.Attack1 );
		}

		public virtual bool CanSecondaryAttack()
		{
			return CanAttack( Secondary, TimeSinceSecondaryAttack, InputButton.Attack2 );
		}

		private void Attack( ClipInfo clipInfo )
		{
			if ( IsRunning || ShouldTuck() ) return;

			if ( clipInfo.FiringType == FiringType.semi && !Input.Pressed( InputButton.Attack1 ) )
				return;

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( !TakeAmmo( 1 ) )
			{
				DryFire( clipInfo.DryFireSound );
				return;
			}

			// Player anim
			(Owner as AnimEntity).SetAnimBool( "b_attack", true );

			// Tell the clients to play the shoot effects
			ShootEffects( clipInfo.MuzzleFlashParticle, clipInfo.BulletEjectParticle, clipInfo.ShootAnim, clipInfo.ScreenShake );

			if ( clipInfo.ShootSound != null )
				PlaySound( clipInfo.ShootSound );

			// Shoot the bullets
			float realSpread;

			if ( this is WeaponBaseShotty )
			{
				realSpread = clipInfo.Spread;
			}
			else
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

		public virtual void AttackPrimary()
		{
			// Dual wield
			if ( DualWield )
			{
				dualWieldLeftFire = !dualWieldLeftFire;
			}

			Attack( Primary );
		}

		public virtual void AttackSecondary()
		{
			if ( Secondary != null )
			{
				Attack( Secondary );
				return;
			}
		}

		/// <summary>
		/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
		/// hits, like if you're going through layers or ricocet'ing or something.
		/// </summary>
		public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
		{
			bool InWater = Physics.TestPointContents( start, CollisionLayer.Water );

			var tr = Trace.Ray( start, end )
					.UseHitboxes()
					.HitLayer( CollisionLayer.Water, !InWater )
					.Ignore( Owner )
					.Ignore( this )
					.Size( radius )
					.Run();

			yield return tr;

			//
			// Another trace, bullet going through thin material, penetrating water surface?
			//
		}

		/// Shoot a single bullet
		public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
		{

			// Spread
			var forward = Owner.EyeRot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * 5000, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				// We turn predictiuon off for this, so any exploding effects don't get culled etc
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

		[ClientRpc]
		protected virtual void ShootEffects( string muzzleFlashParticle, string bulletEjectParticle, string shootAnim, ScreenShake screenShake )
		{
			Host.AssertClient();

			var animatingViewModel = DualWield && dualWieldLeftFire ? dualWieldViewModel : ViewModelEntity;
			ModelEntity firingViewModel = animatingViewModel;

			// We don't want to change the world effect origin if we or others can see it
			if ( (IsLocalPawn && GetClientOwner().Camera is ThirdPersonCamera) || !IsLocalPawn )
			{
				firingViewModel = EffectEntity;
			}

			if ( !string.IsNullOrEmpty( muzzleFlashParticle ) )
				Particles.Create( muzzleFlashParticle, firingViewModel, "muzzle" );

			if ( !string.IsNullOrEmpty( bulletEjectParticle ) )
				Particles.Create( bulletEjectParticle, firingViewModel, "ejection_point" );

			if ( IsLocalPawn )
			{
				new Sandbox.ScreenShake.Perlin( screenShake.Length, screenShake.Speed, screenShake.Size, screenShake.Rotation );
			}

			if ( !string.IsNullOrEmpty( shootAnim ) )
			{
				animatingViewModel?.SetAnimBool( shootAnim, true );
			}

			CrosshairPanel?.OnEvent( "fire" );
		}

		[ClientRpc]
		public virtual void DryFire( string dryFireSound )
		{
			if ( !string.IsNullOrEmpty( dryFireSound ) )
				PlaySound( dryFireSound );
		}
	}
}
