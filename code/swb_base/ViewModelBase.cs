using Sandbox;
using System;
using System.Numerics;

namespace SWB_Base
{
	class ViewModelBase : BaseViewModel
	{
		private WeaponBase weapon;

		private Vector3 lerpMovementPos;
		private Angles lerpMovementAngle;
		private Vector3 lerpSwayPos;
		private Rotation lerpSwayRotation;
		private Vector3 lerpZoomPos;
		private Angles lerpZoomAngle;
		private Vector3 lerpRunPos;
		private Angles lerpRunAngle;
		private Vector3 lerpTuckPos;
		private Angles lerpTuckAngle;

		private float lerpZoomFOV;

		private float walkBob = 0;
		private float tuckDist = -1;

		private int maxSideSway = 2;
		private int maxJumpSway = 4;
		private int maxMoveSway = 5;
		private int maxTiltSway = 5;

		private bool isInJump = false;
		private bool isDualWieldVM = true;
		private bool liveEditing = false;

		/* 
		 * Add realistic viewbobbing!
		 */

		public ViewModelBase( WeaponBase weapon, bool isDualWieldVM = false )
		{
			this.weapon = weapon;
			this.isDualWieldVM = isDualWieldVM;
		}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( weapon.IsDormant ) return;
			
			FieldOfView = weapon.FOV;
			tuckDist = weapon.GetTuckDist();

			if (isDualWieldVM)
			{
				FlipViewModel( ref camSetup );
			}

			AddIdleAnimations( ref camSetup );
			AddViewbobAnimations( ref camSetup );
			AddMovementAnimations( ref camSetup );
			AddWeaponSwayAnimations( ref camSetup );
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

		// Idle animations
		private void AddIdleAnimations( ref CameraSetup camSetup )
		{
			if ( liveEditing ) return;
			if ( weapon.IsZooming && !weapon.ShouldTuck( tuckDist ) ) return;

			var left = camSetup.Rotation.Left;
			var up = camSetup.Rotation.Up;
			var realTime = RealTime.Now;
			var sideSwingMod = 0.1f;
			var upSwingMod = -0.1f;

			Position += up * MathF.Sin( realTime ) * upSwingMod;
			Position += left * MathF.Sin( realTime * sideSwingMod ) * -0.5f;
		}

		// Viewbob animations
		private void AddViewbobAnimations( ref CameraSetup camSetup )
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

		// Movement animations
		private void AddMovementAnimations( ref CameraSetup camSetup )
		{
			// Angles
			// var xSpeed = Math.Clamp( Owner.Velocity.x * 0.1f, -maxSideSway, maxSideSway ); // Is not predicted yet so looks weird
			var zSpeed = Math.Clamp(Owner.Velocity.z*0.1f, -maxJumpSway, maxJumpSway);
			var movAngle = new Angles( zSpeed, 0, 0 );

			lerpMovementAngle = MathZ.FILerp( lerpMovementAngle, movAngle, 10f * weapon.WalkAnimationSpeedMod );
			Rotation *= Rotation.From( lerpMovementAngle );

			// Pos
			var up = camSetup.Rotation.Up;
			var targPos = Vector3.Zero;
			targPos -= up * zSpeed * 0.1f;

			lerpMovementPos = MathZ.FILerp( lerpMovementPos, targPos, 10f * weapon.WalkAnimationSpeedMod );
			Position += lerpMovementPos;
		}

		// Sway animations
		private void AddWeaponSwayAnimations( ref CameraSetup camSetup )
		{
			var swayAmountMod = 1f;

			if ( weapon.IsZooming )
				swayAmountMod = 0.25f;

			// Move sway
			var right = camSetup.Rotation.Right;
			var up = camSetup.Rotation.Up;
			var targPos = Vector3.Zero;
			targPos -= right * Math.Clamp( Mouse.Delta.x * swayAmountMod * 0.5f, -maxMoveSway, maxMoveSway );
			targPos += up * Math.Clamp( Mouse.Delta.y * swayAmountMod * 0.5f, -maxMoveSway, maxMoveSway );

			lerpSwayPos = MathZ.FILerp( lerpSwayPos, targPos, 10f * weapon.WalkAnimationSpeedMod );
			Position += lerpSwayPos;

			// Tilt sway
			var tiltX = Math.Clamp( Mouse.Delta.x * swayAmountMod * 1, -maxTiltSway, maxTiltSway );
			var tiltY = Math.Clamp( Mouse.Delta.y * swayAmountMod * 2, -maxTiltSway, maxTiltSway );
			var targRotation = Rotation.From( tiltY, -tiltX, tiltX );

			if ( lerpSwayRotation.w == 0)
				lerpSwayRotation.w = 1;

			lerpSwayRotation = Rotation.Slerp( lerpSwayRotation, targRotation, RealTime.Delta * 10f * weapon.WalkAnimationSpeedMod );
			Rotation *= lerpSwayRotation;
		}

		private void LerpToPosition( Angles angles, Vector3 pos, ref Angles lerpTargAngle, ref Vector3 lerpTargPos, ref CameraSetup camSetup )
		{
			// Angles
			lerpTargAngle = MathZ.FILerp( lerpTargAngle, angles, 10f );
			var targAngles = Rotation.Angles() + lerpTargAngle;

			Rotation = Rotation.From( targAngles );

			// Position
			lerpTargPos = MathZ.FILerp( lerpTargPos, pos, 10f );

			var right = camSetup.Rotation.Right;
			var up = camSetup.Rotation.Up;
			var forward = camSetup.Rotation.Forward;

			var posOffset = Vector3.Zero;
			posOffset += right * lerpTargPos.x;
			posOffset += up * lerpTargPos.y;
			posOffset += forward * lerpTargPos.z;

			Position += posOffset;
		}

		// Action animations
		private void AddActionAnimations( ref CameraSetup camSetup )
		{

			if ( weapon.ShouldTuck(tuckDist) && weapon.RunAnimData != null )
			{
				var animationCompletion = Math.Min(1, ( (weapon.TuckRange-tuckDist) / weapon.TuckRange ) + 0.5f );
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
				lerpZoomFOV = MathZ.FILerp( lerpZoomFOV, weapon.ZoomFOV, 10f );
				FieldOfView = lerpZoomFOV;

				// Freeze viewmodel (temp solution)
				if ( !string.IsNullOrEmpty(weapon.FreezeViewModelOnZoom))
					SetAnimBool( weapon.FreezeViewModelOnZoom, true );

			} else if ( !lerpZoomPos.IsNearlyEqual( Vector3.Zero, 0.01f ) ) 
			{
				// Zoom out
				LerpToPosition( Angles.Zero, Vector3.Zero, ref lerpZoomAngle, ref lerpZoomPos, ref camSetup );

				// FOV (restore)
				lerpZoomFOV = MathZ.FILerp( lerpZoomFOV, weapon.FOV, 10f );
				FieldOfView = lerpZoomFOV;

			} else
			{
				if ( lerpZoomFOV != weapon.FOV )
				{
					// Reset zoom vars
					lerpZoomPos = Vector3.Zero;
					lerpZoomAngle = Angles.Zero;
					lerpZoomFOV = weapon.FOV;
				}
			}

			// DEBUG (set true to do live edits)
			// liveEditing = true;

			if ( weapon.IsRunning && weapon.RunAnimData != null || liveEditing )
			{
				var testAngles = weapon.RunAnimData.Angle;
				var testPos = weapon.RunAnimData.Pos;

				if ( liveEditing )
				{
					FieldOfView = weapon.ZoomFOV;
					testAngles = new Angles( 0f, 0.5f, 0f );
					testPos = new Vector3( -5.5f, -2f, 4f );
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
