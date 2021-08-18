using Sandbox;
using System.Linq;

namespace SWB_Base
{
	partial class PlayerBase : Player
	{
		TimeSince timeSinceDropped;

		public bool SupressPickupNotices { get; set; }

		public PlayerBase()
		{
			Inventory = new DmInventory( this );
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );
			//SetModel( "playermodels/css_playermodels/css_t_arctic.vmdl" );

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Health = 100;

			ClearAmmo();

			base.Respawn();
		}
		public override void OnKilled()
		{
			base.OnKilled();

			Inventory.DropActive();
			Inventory.DeleteContents();

			BecomeRagdollOnClient( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );

			Controller = null;
			Camera = new SpectateRagdollCamera();

			EnableAllCollisions = false;
			EnableDrawing = false;
		}


		public override void Simulate( Client cl )
		{
			//if ( cl.NetworkIdent == 1 )
			//  return;

			base.Simulate( cl );

			// Input requested a weapon switch
			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( LifeState != LifeState.Alive )
				return;

			TickPlayerUse();

			if ( Input.Pressed( InputButton.View ) )
			{
				if ( Camera is ThirdPersonCamera )
				{
					Camera = new FirstPersonCamera();
				}
				else
				{
					Camera = new ThirdPersonCamera();
				}
			}

			if ( Input.Pressed( InputButton.Drop ) )
			{
				var dropped = Inventory.DropActive();
				if ( dropped != null )
				{
					if ( dropped.PhysicsGroup != null )
					{
						dropped.PhysicsGroup.Velocity = Velocity + (EyeRot.Forward + EyeRot.Up) * 300;
					}

					timeSinceDropped = 0;
					SwitchToBestWeapon();
				}
			}

			SimulateActiveChild( cl, ActiveChild );

			//
			// If the current weapon is out of ammo and we last fired it over half a second ago
			// lets try to switch to a better wepaon
			//
			if ( ActiveChild is WeaponBase weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f && weapon.TimeSinceSecondaryAttack > 0.5f )
			{
				SwitchToBestWeapon();
			}
		}

		public void SwitchToBestWeapon()
		{
			var best = Children.Select( x => x as WeaponBase )
				.Where( x => x.IsValid() && x.IsUsable() )
				.OrderByDescending( x => x.BucketWeight )
				.FirstOrDefault();

			if ( best == null ) return;

			ActiveChild = best;
		}

		public override void StartTouch( Entity other )
		{
			if ( timeSinceDropped < 1 ) return;

			base.StartTouch( other );
		}

		Rotation lastCameraRot = Rotation.Identity;

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );

			if ( lastCameraRot == Rotation.Identity )
				lastCameraRot = setup.Rotation;

			var angleDiff = Rotation.Difference( lastCameraRot, setup.Rotation );
			var angleDiffDegrees = angleDiff.Angle();
			var allowance = 20.0f;

			if ( angleDiffDegrees > allowance )
			{
				// We could have a function that clamps a rotation to within x degrees of another rotation?
				lastCameraRot = Rotation.Lerp( lastCameraRot, setup.Rotation, 1.0f - (allowance / angleDiffDegrees) );
			}
			else
			{
				//lastCameraRot = Rotation.Lerp( lastCameraRot, Camera.Rotation, Time.Delta * 0.2f * angleDiffDegrees );
			}
		}

		DamageInfo LastDamage;

		public override void TakeDamage( DamageInfo info )
		{
			LastDamage = info;

			if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
			{
				info.Damage *= 2.0f;
			}

			base.TakeDamage( info );

			if ( info.Attacker is PlayerBase attacker && attacker != this )
			{
				// Note - sending this only to the attacker!
				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, ((float)Health).LerpInverse( 100, 0 ) );

				TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
			}
		}

		[ClientRpc]
		public void DidDamage( Vector3 pos, float amount, float healthinv )
		{
			Sound.FromScreen( "dm.ui_attacker" )
				.SetPitch( 1 + healthinv * 1 );

			HitIndicator.Current?.OnHit( pos, amount );
		}

		[ClientRpc]
		public void TookDamage( Vector3 pos )
		{
			//DebugOverlay.Sphere( pos, 5.0f, Color.Red, false, 50.0f );

			DamageIndicator.Current?.OnHit( pos );
		}
	}
}
