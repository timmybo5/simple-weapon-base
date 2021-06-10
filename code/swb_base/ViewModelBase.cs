using Sandbox;
using System;

namespace SWB_Base
{
	class ViewModelBase : BaseViewModel
	{

		private WeaponBase weapon;

		private Vector3 lerpZoomPos;
		private Angles lerpZoomAngle;
		private Vector3 lerpRunPos;
		private Angles lerpRunAngle;
		private Vector3 lerpTuckPos;
		private Angles lerpTuckAngle;
		private float lerpZoomFOV;

		private float walkBob = 0;
		private float tuckDist = -1;
		private bool isDualWieldVM = true;

		private bool liveEditing = false;

		public ViewModelBase( WeaponBase weapon, bool isDualWieldVM = false )
		{
			this.weapon = weapon;
			this.isDualWieldVM = isDualWieldVM;
		}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );
			
			FieldOfView = weapon.FOV;
			tuckDist = weapon.GetTuckDist();

			if (isDualWieldVM)
			{
				FlipViewModel( ref camSetup );
			}

			AddWalkingAnimations( ref camSetup );
			AddIdleAnimations( ref camSetup );
			AddActionAnimations( ref camSetup );
		}

		private void FlipViewModel( ref CameraSetup camSetup )
		{
			// Waiting for https://github.com/Facepunch/sbox-issues/issues/324

			// Temp solution: 
			var posOffset = Vector3.Zero;
			posOffset -= camSetup.Rotation.Right * 10;
			Position += posOffset;
		}

		// Walking animations
		private void AddWalkingAnimations( ref CameraSetup camSetup )
		{
			var speed = Owner.Velocity.Length.LerpInverse( 0, 320 );
			speed = speed * weapon.WalkAnimationSpeedMod;

			var left = camSetup.Rotation.Left;
			var up = camSetup.Rotation.Up;

			if ( Owner.GroundEntity != null )
			{
				walkBob += Time.Delta * 25.0f * speed;
			}

			var sideSwingMod = -1f;
			var upSwingMod = 0.6f;

			if ( weapon.IsZooming )
			{
				speed = speed * 0.25f;
				sideSwingMod = sideSwingMod * 0.4f;
				upSwingMod = upSwingMod * 0.4f;
			}

			Position += up * MathF.Sin( walkBob ) * speed * sideSwingMod;
			Position += left * MathF.Sin( walkBob * upSwingMod ) * speed * -0.5f;
		}

		// Idle animations
		private void AddIdleAnimations( ref CameraSetup camSetup )
		{
			if ( liveEditing ) return;
			if ( weapon.IsZooming && !weapon.ShouldTuck( tuckDist ) ) return;

			// Idle animation
			var left = camSetup.Rotation.Left;
			var up = camSetup.Rotation.Up;
			var realTime = RealTime.Now;
			var sideSwingMod = -0.1f;
			var upSwingMod = 0.1f;

			Position += up * MathF.Sin( realTime ) * sideSwingMod;
			Position += left * MathF.Sin( realTime * upSwingMod ) * -0.5f;
		}

		private void LerpToPosition( Angles angles, Vector3 pos, ref Angles lerpTargAngle, ref Vector3 lerpTargPos, ref CameraSetup camSetup )
		{
			// Angles
			lerpTargAngle = Angles.Lerp( lerpTargAngle, angles, 0.1f );
			var targAngles = Rotation.Angles() + lerpTargAngle;

			//Rotation rotation = Rotation;
			//rotation += Rotation.FromAxis( Rotation.Right, 10);
			//Rotation.
			//rotation += Rotation.FromAxis( Rotation.Up, 0 );
			//rotation += Rotation.FromAxis( camSetup.Rotation.Forward, lerpTargAngle.roll );
			//Rotation = rotation;
			//ViewOffset = new Vector3( 10, 0, 100 );

			Rotation = Rotation.From( targAngles );

			// Position
			lerpTargPos = lerpTargPos.LerpTo( pos, 0.1f );

			var right = camSetup.Rotation.Right;
			var up = camSetup.Rotation.Up;
			var forward = camSetup.Rotation.Forward;

			var posOffset = Vector3.Zero;
			posOffset += right * lerpTargPos.x;
			posOffset += up * lerpTargPos.y;
			posOffset += forward * lerpTargPos.z;

			Position += posOffset;
		}

		private void AddActionAnimations( ref CameraSetup camSetup )
		{

			if ( weapon.ShouldTuck(tuckDist) && weapon.RunAnimData != null )
			{
				var animationCompletion = Math.Min(1, ((weapon.TuckRange-tuckDist) / weapon.TuckRange)+0.5f);
				LerpToPosition( weapon.RunAnimData.Angle * animationCompletion, weapon.RunAnimData.Pos * animationCompletion, ref lerpTuckAngle , ref lerpTuckPos, ref camSetup );

				// Prevent other animations while tucking
				return;

			} else if ( tuckDist == -1 && lerpTuckPos != Vector3.Zero )
			{
				if ( !weapon.IsZooming && !weapon.IsRunning )
				{
					LerpToPosition( Angles.Zero, Vector3.Zero, ref lerpTuckAngle, ref lerpTuckPos, ref camSetup );
				} else
				{
					lerpTuckPos = Vector3.Zero;
					lerpTuckAngle = Angles.Zero;
				}
			}

			if ( weapon.IsZooming && weapon.ZoomAnimData != null )
			{
				// Zoom in
				LerpToPosition( weapon.ZoomAnimData.Angle, weapon.ZoomAnimData.Pos, ref lerpZoomAngle, ref lerpZoomPos, ref camSetup );

				// FOV
				lerpZoomFOV = MathX.LerpTo( lerpZoomFOV, weapon.ZoomFOV, 0.1f );
				FieldOfView = lerpZoomFOV;

				// Freeze viewmodel (temp solution)
				if ( !string.IsNullOrEmpty(weapon.FreezeViewModelOnZoom))
					SetAnimBool( weapon.FreezeViewModelOnZoom, true );

			} else if ( !lerpZoomPos.IsNearlyEqual( Vector3.Zero, 0.01f ) ) 
			{
				// Zoom out
				LerpToPosition( Angles.Zero, Vector3.Zero, ref lerpZoomAngle, ref lerpZoomPos, ref camSetup );

				// FOV (restore)
				lerpZoomFOV = MathX.LerpTo( lerpZoomFOV, weapon.FOV, 0.1f );
				FieldOfView = lerpZoomFOV;

			} else
			{
				if ( lerpZoomFOV != weapon.FOV )
				{
					// Reset zoom vars
					lerpZoomPos = Vector3.Zero;
					lerpZoomAngle = Angles.Zero;
					lerpZoomFOV = weapon.FOV;

					// UnFreeze viewmodel (temp solution)
					//if ( !string.IsNullOrEmpty( weapon.FreezeViewModelOnZoom ) )
						//SetAnimBool( weapon.FreezeViewModelOnZoom, false );

				}
			}

			// DEBUG (set true to do live edits)
			//liveEditing = false;

			if ( weapon.IsRunning && weapon.RunAnimData != null || liveEditing )
			{
				var testAngles = weapon.RunAnimData.Angle;
				var testPos = weapon.RunAnimData.Pos;

				if ( liveEditing )
				{
					//FieldOfView = weapon.ZoomFOV;
					testAngles = new Angles( 0f, 0f, 0f );
					testPos = new Vector3( 0f, 0f, 0f );
				}

				LerpToPosition( testAngles, testPos, ref lerpRunAngle, ref lerpRunPos, ref camSetup );

			} else if ( !lerpRunPos.IsNearlyEqual(Vector3.Zero, 0.01f ) ) 
			{
				LerpToPosition( Angles.Zero, Vector3.Zero, ref lerpRunAngle, ref lerpRunPos, ref camSetup );

			} else
			{
				if ( lerpRunPos != Vector3.Zero )
				{
					lerpRunPos = Vector3.Zero;
					lerpRunAngle = Angles.Zero;
				}	
			}
		}
	}
}
