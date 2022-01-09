using System;
using Sandbox;

namespace SWB_Base
{
    partial class ViewModelBase : BaseViewModel
    {
        public AngPos editorOffset;

        private WeaponBase weapon;

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

        public ViewModelBase(WeaponBase weapon)
        {
            this.weapon = weapon;
            FinalFOV = weapon.FOV;
        }

        public override void PostCameraSetup(ref CameraSetup camSetup)
        {
            base.PostCameraSetup(ref camSetup);
            FieldOfView = weapon.FOV;
            Rotation = camSetup.Rotation;
            Position = camSetup.Position;
            if (weapon.IsDormant) return;
            if (Owner != null && Owner.Health <= 0)
            {
                this.EnableDrawing = false;
                return;
            }

            // Smoothly transition the vectors with the target values
            FinalVectorPos = FinalVectorPos.LerpTo(TargetVectorPos, animSpeed * RealTime.Delta);
            FinalVectorRot = FinalVectorRot.LerpTo(TargetVectorRot, animSpeed * RealTime.Delta);
            FinalFOV = MathX.LerpTo(FinalFOV, TargetFOV, animSpeed * RealTime.Delta);
            animSpeed = 10 * weapon.WalkAnimationSpeedMod;

            // Change the angles and positions of the viewmodel with the new vectors
            Rotation *= Rotation.From(FinalVectorRot.x, FinalVectorRot.y, FinalVectorRot.z);
            Position += FinalVectorPos.z * Rotation.Up + FinalVectorPos.y * Rotation.Forward + FinalVectorPos.x * Rotation.Right;
            FieldOfView = FinalFOV;

            // I'm sure there's something already that does this for me, but I spend an hour
            // searching through the wiki and a bunch of other garbage and couldn't find anything...
            // So I'm doing it manually. Problem solved.
            localVel = new Vector3(Owner.Rotation.Right.Dot(Owner.Velocity), Owner.Rotation.Forward.Dot(Owner.Velocity), Owner.Velocity.z);

            // Initialize the target vectors for this frame
            TargetVectorPos = new Vector3(weapon.ViewModelOffset.Pos);
            TargetVectorRot = new Vector3(MathUtil.ToVector3(weapon.ViewModelOffset.Angle));
            TargetFOV = weapon.FOV;

            // Model editor
            if (Owner is PlayerBase player && (player.IsModelEditing() || player.IsAttachmentEditing()))
            {
                if (editorOffset != AngPos.Zero)
                {
                    TargetVectorRot += MathUtil.ToVector3(editorOffset.Angle);
                    TargetVectorPos += editorOffset.Pos;
                }
                return;
            };

            // Tucking
            float tuckDist;
            if (weapon.RunAnimData != AngPos.Zero && weapon.ShouldTuck(out tuckDist))
            {
                var animationCompletion = Math.Min(1, ((weapon.TuckRange - tuckDist) / weapon.TuckRange) + 0.5f);
                TargetVectorPos = weapon.RunAnimData.Pos * animationCompletion;
                TargetVectorRot = MathUtil.ToVector3(weapon.RunAnimData.Angle * animationCompletion);
                return;
            }

            // Handle different animations
            HandleIdleAnimation(ref camSetup);
            HandleWalkAnimation(ref camSetup);
            HandleSwayAnimation(ref camSetup);
            HandleIronAnimation(ref camSetup);
            HandleSprintAnimation(ref camSetup);
            HandleCustomizeAnimation(ref camSetup);
            HandleJumpAnimation(ref camSetup);
        }

        private void HandleIdleAnimation(ref CameraSetup camSetup)
        {
            // No swaying if aiming
            if (weapon.IsZooming)
                return;

            // Perform a "breathing" animation
            float breatheTime = RealTime.Now * 2.0f;
            TargetVectorPos -= new Vector3(MathF.Cos(breatheTime / 4.0f) / 8.0f, 0.0f, -MathF.Cos(breatheTime / 4.0f) / 32.0f);
            TargetVectorRot -= new Vector3(MathF.Cos(breatheTime / 5.0f), MathF.Cos(breatheTime / 4.0f), MathF.Cos(breatheTime / 7.0f));

            // Crouching animation
            if (Input.Down(InputButton.Duck))
                TargetVectorPos += new Vector3(-1.0f, -1.0f, 0.5f);
        }

        private void HandleWalkAnimation(ref CameraSetup camSetup)
        {
            float breatheTime = RealTime.Now * 16.0f;
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
                breatheTime = RealTime.Now * 18.0f;
                maxWalkSpeed = 100.0f;
            }

            // Check for sideways velocity to sway the gun slightly
            if (weapon.IsZooming || localVel.x > 0.0f)
                roll = -7.0f * (localVel.x / maxWalkSpeed);
            else if (localVel.x < 0.0f)
                yaw = 3.0f * (localVel.x / maxWalkSpeed);

            // Perform walk cycle
            TargetVectorPos -= new Vector3((-MathF.Cos(breatheTime / 2.0f) / 5.0f) * walkSpeed / maxWalkSpeed - yaw / 4.0f, 0.0f, 0.0f);
            TargetVectorRot -= new Vector3((Math.Clamp(MathF.Cos(breatheTime), -0.3f, 0.3f) * 2.0f) * walkSpeed / maxWalkSpeed, (-MathF.Cos(breatheTime / 2.0f) * 1.2f) * walkSpeed / maxWalkSpeed - yaw * 1.5f, roll);
        }

        private void HandleSwayAnimation(ref CameraSetup camSetup)
        {
            int swayspeed = 5;

            // Fix the sway faster if we're ironsighting
            if (weapon.IsZooming && weapon.ZoomAnimData != AngPos.Zero)
                swayspeed = 20;

            // Lerp the eye position
            LastEyeRot = Rotation.Lerp(LastEyeRot, Owner.EyeRot, swayspeed * RealTime.Delta);

            // Calculate the difference between our current eye angles and old (lerped) eye angles
            Angles angDif = Owner.EyeRot.Angles() - LastEyeRot.Angles();
            angDif = new Angles(angDif.pitch, MathX.RadianToDegree(MathF.Atan2(MathF.Sin(MathX.DegreeToRadian(angDif.yaw)), MathF.Cos(MathX.DegreeToRadian(angDif.yaw)))), 0);

            // Perform sway
            TargetVectorPos += new Vector3(Math.Clamp(angDif.yaw * 0.04f, -1.5f, 1.5f), 0.0f, Math.Clamp(angDif.pitch * 0.04f, -1.5f, 1.5f));
            TargetVectorRot += new Vector3(Math.Clamp(angDif.pitch * 0.2f, -4.0f, 4.0f), Math.Clamp(angDif.yaw * 0.2f, -4.0f, 4.0f), 0.0f);
        }

        private void HandleIronAnimation(ref CameraSetup camSetup)
        {
            if (weapon.IsZooming && weapon.ZoomAnimData != AngPos.Zero)
            {
                animSpeed = 10 * weapon.WalkAnimationSpeedMod;
                TargetVectorPos += weapon.ZoomAnimData.Pos;
                TargetVectorRot += MathUtil.ToVector3(weapon.ZoomAnimData.Angle);
                TargetFOV = weapon.ZoomFOV;
            }
        }

        private void HandleSprintAnimation(ref CameraSetup camSetup)
        {
            if (weapon.IsRunning && weapon.RunAnimData != AngPos.Zero && !weapon.IsCustomizing)
            {
                TargetVectorPos += weapon.RunAnimData.Pos;
                TargetVectorRot += MathUtil.ToVector3(weapon.RunAnimData.Angle);
            }
        }

        private void HandleCustomizeAnimation(ref CameraSetup camSetup)
        {
            if (weapon.IsCustomizing && weapon.CustomizeAnimData != AngPos.Zero)
            {
                TargetVectorPos += weapon.CustomizeAnimData.Pos;
                TargetVectorRot += MathUtil.ToVector3(weapon.CustomizeAnimData.Angle);
            }
        }

        private void HandleJumpAnimation(ref CameraSetup camSetup)
        {
            // If we're not on the ground, reset the landing animation time
            if (Owner.GroundEntity == null)
                landTime = RealTime.Now + 0.31f;

            // Reset the timers once they elapse
            if (landTime < RealTime.Now && landTime != 0.0f)
            {
                landTime = 0.0f;
                jumpTime = 0.0f;
            }

            // If we jumped, start the animation
            if (Input.Down(InputButton.Jump) && jumpTime == 0.0f)
            {
                jumpTime = RealTime.Now + 0.31f;
                landTime = 0.0f;
            }

            // If we're not ironsighting, do a fancy jump animation
            if (!weapon.IsZooming)
            {
                if (jumpTime > RealTime.Now)
                {
                    // If we jumped, do a curve upwards
                    float f = 0.31f - (jumpTime - RealTime.Now);
                    float xx = MathUtil.BezierY(f, 0.0f, -4.0f, 0.0f);
                    float yy = 0.0f;
                    float zz = MathUtil.BezierY(f, 0.0f, -2.0f, -5.0f);
                    float pt = MathUtil.BezierY(f, 0.0f, -4.36f, 10.0f);
                    float yw = xx;
                    float rl = MathUtil.BezierY(f, 0.0f, -10.82f, -5.0f);
                    TargetVectorPos += new Vector3(xx, yy, zz) / 4.0f;
                    TargetVectorRot += new Vector3(pt, yw, rl) / 4.0f;
                    animSpeed = 20.0f;
                }
                else if (Owner.GroundEntity == null)
                {
                    // Shaking while falling
                    float breatheTime = RealTime.Now * 30.0f;
                    TargetVectorPos += new Vector3(MathF.Cos(breatheTime / 2.0f) / 16.0f, 0.0f, -5.0f + (MathF.Sin(breatheTime / 3.0f) / 16.0f)) / 4.0f;
                    TargetVectorRot += new Vector3(10.0f - (MathF.Sin(breatheTime / 3.0f) / 4.0f), MathF.Cos(breatheTime / 2.0f) / 4.0f, -5.0f) / 4.0f;
                    animSpeed = 20.0f;
                }
                else if (landTime > RealTime.Now)
                {
                    // If we landed, do a fancy curve downwards
                    float f = landTime - RealTime.Now;
                    float xx = MathUtil.BezierY(f, 0.0f, -4.0f, 0.0f);
                    float yy = 0.0f;
                    float zz = MathUtil.BezierY(f, 0.0f, -2.0f, -5.0f);
                    float pt = MathUtil.BezierY(f, 0.0f, -4.36f, 10.0f);
                    float yw = xx;
                    float rl = MathUtil.BezierY(f, 0.0f, -10.82f, -5.0f);
                    TargetVectorPos += new Vector3(xx, yy, zz) / 2.0f;
                    TargetVectorRot += new Vector3(pt, yw, rl) / 2.0f;
                    animSpeed = 20.0f;
                }
            }
            else
                TargetVectorPos += new Vector3(0.0f, 0.0f, Math.Clamp(localVel.z / 1000.0f, -1.0f, 1.0f));
        }
    }
}
