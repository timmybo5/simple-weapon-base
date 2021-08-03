using Sandbox;
using Sandbox.UI;
using System;
using System.Numerics;

namespace SWB_Base
{
	class ViewModelBase : BaseViewModel
	{
		private WeaponBase weapon;

		private bool isDualWieldVM = true;

        private float animSpeed;

        // Target animation values
		private Vector3 TargetVectorPos;
		private Vector3 TargetVectorRot;
		private float TargetFOV;
        
        // Finalized animation values
		private Vector3 FinalVectorPos;
		private Vector3 FinalVectorRot;
		private float FinalFOV;
        
        // Sway
		private Rotation LastEyeRot;

        // Jumping Animation
        private float jumpTime;
        private float landTime;

        // Helpful values
		private Vector3 localVel;

		public ViewModelBase(WeaponBase weapon, bool isDualWieldVM = false)
		{
			this.weapon = weapon;
			this.isDualWieldVM = isDualWieldVM;
		}

		public override void PostCameraSetup(ref CameraSetup camSetup)
		{
			base.PostCameraSetup(ref camSetup);
			FieldOfView = weapon.FOV;
			Rotation = camSetup.Rotation;
			Position = camSetup.Position;
			if (weapon.IsDormant) return;
            
			// Smoothly transition the vectors with the target values
			this.FinalVectorPos = this.FinalVectorPos.LerpTo(this.TargetVectorPos, animSpeed*RealTime.Delta);
			this.FinalVectorRot = this.FinalVectorRot.LerpTo(this.TargetVectorRot, animSpeed*RealTime.Delta);
            this.FinalFOV = MathX.LerpTo(this.FinalFOV, this.TargetFOV, animSpeed*RealTime.Delta);
            this.animSpeed = 5;

			// Change the angles and positions of the viewmodel with the new vectors
			Rotation *= Rotation.From(this.FinalVectorRot.x, this.FinalVectorRot.y, this.FinalVectorRot.z);
			Position += this.FinalVectorPos.z*Rotation.Up + this.FinalVectorPos.y*Rotation.Forward + this.FinalVectorPos.x*Rotation.Right;
            FieldOfView = this.FinalFOV;

			// I'm sure there's something already that does this for me, but I spend an hour
			// searching through the wiki and a bunch of other garbage and couldn't find anything...
			// So I'm doing it manually. Problem solved.
			this.localVel = new Vector3(Owner.Rotation.Right.Dot(Owner.Velocity), Owner.Rotation.Forward.Dot(Owner.Velocity), Owner.Velocity.z);

			// Initialize the target vectors for this frame
			this.TargetVectorPos = new Vector3(0.0f, 0.0f, 0.0f);
			this.TargetVectorRot = new Vector3(0.0f, 0.0f, 0.0f);
            this.TargetFOV = weapon.FOV;
            
            // Flip the Viewmodel
			if (isDualWieldVM)
                FlipViewModel(ref camSetup);

			// Handle different animations
			HandleIdleAnimation(ref camSetup);
			HandleWalkAnimation(ref camSetup);
            HandleSwayAnimation(ref camSetup);
            HandleIronAnimation(ref camSetup);
            HandleSprintAnimation(ref camSetup);
            HandleJumpAnimation(ref camSetup);
		}

		private void FlipViewModel( ref CameraSetup camSetup )
		{
			// Waiting for https://github.com/Facepunch/sbox-issues/issues/324

			// Temp solution: 
			var posOffset = Vector3.Zero;
			posOffset -= camSetup.Rotation.Right*10.0f;
			Position += posOffset;
		}

		private void HandleIdleAnimation(ref CameraSetup camSetup)
		{
            // No swaying if aiming
            if (weapon.IsZooming)
                return;
            
            // Perform a "breathing" animation
			float breatheTime = RealTime.Now*2.0f; 
			this.TargetVectorPos -= new Vector3(MathF.Cos(breatheTime/4.0f)/8.0f, 0.0f, -MathF.Cos(breatheTime/4.0f)/32.0f);
			this.TargetVectorRot -= new Vector3(MathF.Cos(breatheTime/5.0f), MathF.Cos(breatheTime/4.0f), MathF.Cos(breatheTime/7.0f));
            
            // Crouching animation
            if (Input.Down(InputButton.Duck))
                this.TargetVectorPos += new Vector3(-1.0f, -1.0f, 0.5f);
		}

		private void HandleWalkAnimation(ref CameraSetup camSetup)
		{
			float breatheTime = RealTime.Now*16.0f;
			float walkSpeed = new Vector3(Owner.Velocity.x, Owner.Velocity.y, 0.0f).Length;
			float maxWalkSpeed = 200.0f;
			float roll = 0.0f;
			float yaw = 0.0f;
            
            // Check if on the ground
            if (Owner.GroundEntity == null)
                return;
            
            // Check if sprinting
            if (weapon.IsRunning)
            {
                breatheTime = RealTime.Now*18.0f;
                maxWalkSpeed = 100.0f;
            }

			// Check for sideways velocity to sway the gun slightly
			if (this.localVel.x > 0.0f)
				roll = -7.0f*(this.localVel.x/maxWalkSpeed);
			else if (this.localVel.x < 0.0f)
				yaw = 3.0f*(this.localVel.x/maxWalkSpeed);

			// Perform walk cycle
			this.TargetVectorPos -= new Vector3((-MathF.Cos(breatheTime/2.0f)/5.0f)*walkSpeed/maxWalkSpeed-yaw/4.0f, 0.0f, 0.0f);
			this.TargetVectorRot -= new Vector3((Math.Clamp(MathF.Cos(breatheTime), -0.3f, 0.3f)*2.0f)*walkSpeed/maxWalkSpeed, (-MathF.Cos(breatheTime/2.0f)*1.2f)*walkSpeed/maxWalkSpeed-yaw*1.5f, roll);
		}

		private void HandleSwayAnimation(ref CameraSetup camSetup)
		{            
            int swayspeed = 5;
            
            // Fix the sway faster if we're ironsighting
            if (weapon.IsZooming && weapon.ZoomAnimData != null)
                swayspeed = 20;
            
            // Lerp the eye position
            this.LastEyeRot = Rotation.Lerp(this.LastEyeRot, Owner.EyeRot, swayspeed*RealTime.Delta);
            
            // Calculate the difference between our current eye angles and old (lerped) eye angles
            Angles angDif = Owner.EyeRot.Angles()-this.LastEyeRot.Angles();
            angDif = new Angles(angDif.pitch, MathX.RadianToDegree(MathF.Atan2(MathF.Sin(MathX.DegreeToRadian(angDif.yaw)), MathF.Cos(MathX.DegreeToRadian(angDif.yaw)))), 0);
            
            // Perform sway
			this.TargetVectorPos += new Vector3(Math.Clamp(angDif.yaw*0.04f, -1.5f, 1.5f), 0.0f, Math.Clamp(angDif.pitch*0.04f, -1.5f, 1.5f));
            this.TargetVectorRot += new Vector3(Math.Clamp(angDif.pitch*0.2f, -4.0f, 4.0f), Math.Clamp(angDif.yaw*0.2f, -4.0f, 4.0f), 0.0f);
		}
        
        private void HandleIronAnimation(ref CameraSetup camSetup)
        {
            if (weapon.IsZooming && weapon.ZoomAnimData != null)
            {
                this.animSpeed = 10;
                this.TargetVectorPos += new Vector3(weapon.ZoomAnimData.Pos.x, weapon.ZoomAnimData.Pos.z, weapon.ZoomAnimData.Pos.y);
                this.TargetVectorRot += new Vector3(weapon.ZoomAnimData.Angle.pitch, weapon.ZoomAnimData.Angle.yaw, weapon.ZoomAnimData.Angle.roll);
                this.TargetFOV = weapon.ZoomFOV;
            }
        }
        
        private void HandleSprintAnimation(ref CameraSetup camSetup)
        {
            if (weapon.IsRunning && weapon.RunAnimData != null)
            {
                this.TargetVectorPos += new Vector3(weapon.RunAnimData.Pos.x, weapon.RunAnimData.Pos.z, weapon.RunAnimData.Pos.y);
                this.TargetVectorRot += new Vector3(weapon.RunAnimData.Angle.pitch, weapon.RunAnimData.Angle.yaw, weapon.RunAnimData.Angle.roll);
            }
        }
        
        // Helpful bezier function. Use this if you gotta: https://www.desmos.com/calculator/cahqdxeshd
        private float BezierY(float f, float a, float b, float c)
        {
            f = f*3.2258f;
            return MathF.Pow((1.0f-f), 2.0f)*a + 2.0f*(1.0f-f)*f*b + MathF.Pow(f, 2.0f)*c;
        }
        
        private void HandleJumpAnimation(ref CameraSetup camSetup)
        {
            // If we're not on the ground, reset the landing animation time
            if (Owner.GroundEntity == null)
                this.landTime = RealTime.Now + 0.31f;
            
            // Reset the timers once they elapse
            if (this.landTime < RealTime.Now && this.landTime != 0.0f)
            {
                this.landTime = 0.0f;
                this.jumpTime = 0.0f;
            }

            // If we jumped, start the animation
            if (Input.Down(InputButton.Jump) && this.jumpTime == 0.0f)
            {
                this.jumpTime = RealTime.Now + 0.31f;
                this.landTime = 0.0f;
            }
            
            // If we're not ironsighting, do a fancy jump animation
            if (!weapon.IsZooming)
            {
                if (this.jumpTime > RealTime.Now)
                {                    
                    // If we jumped, do a curve upwards
                    float f = 0.31f - (this.jumpTime-RealTime.Now);
                    float xx = BezierY(f, 0.0f, -4.0f, 0.0f);
                    float yy = 0.0f;
                    float zz = BezierY(f, 0.0f, -2.0f, -5.0f);
                    float pt = BezierY(f, 0.0f, -4.36f, 10.0f);
                    float yw = xx;
                    float rl = BezierY(f, 0.0f, -10.82f, -5.0f);
                    this.TargetVectorPos += new Vector3(xx, yy, zz)/4.0f;
                    this.TargetVectorRot += new Vector3(pt, yw, rl)/4.0f;
                    this.animSpeed = 20.0f;
                }
                else if (Owner.GroundEntity == null)
                {
                    // Shaking while falling
                    float breatheTime = RealTime.Now*30.0f;
                    this.TargetVectorPos += new Vector3(MathF.Cos(breatheTime/2.0f)/16.0f, 0.0f, -5.0f+(MathF.Sin(breatheTime/3.0f)/16.0f))/4.0f;
                    this.TargetVectorRot += new Vector3(10.0f-(MathF.Sin(breatheTime/3.0f)/4.0f), MathF.Cos(breatheTime/2.0f)/4.0f,  -5.0f)/4.0f;
                    this.animSpeed = 20.0f;
                }
                else if (this.landTime > RealTime.Now)
                {
                    // If we landed, do a fancy curve downwards
                    float f = this.landTime-RealTime.Now;
                    float xx = BezierY(f, 0.0f, -4.0f, 0.0f);
                    float yy = 0.0f;
                    float zz = BezierY(f, 0.0f, -2.0f, -5.0f);
                    float pt = BezierY(f, 0.0f, -4.36f, 10.0f);
                    float yw = xx;
                    float rl = BezierY(f, 0.0f, -10.82f, -5.0f);
                    this.TargetVectorPos += new Vector3(xx, yy, zz)/2.0f;
                    this.TargetVectorRot += new Vector3(pt, yw, rl)/2.0f;
                    this.animSpeed = 20.0f;
                }
            }
            else
                this.TargetVectorPos += new Vector3(0.0f, 0.0f, Math.Clamp(localVel.z/1000.0f, -1.0f, 1.0f));
        }
	}
}
