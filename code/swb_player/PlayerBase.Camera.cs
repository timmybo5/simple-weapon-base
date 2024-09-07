using SWB.Shared;
using System;
using static Sandbox.CursorSettings;

namespace SWB.Player;

public partial class PlayerBase
{

    [Property] public float Distance { get; set; } = 0f;
    

    public float InputSensitivity { get; set; } = 1f;

    // Eye Offset (resets after applying)
    public Angles EyeAnglesOffset { get; set; }

    // Camera Offsets (resets after applying)
    public Angles AnglesOffset { get; set; }
    public Vector3 PosOffset { get; set; }

    public bool IsFirstPerson => Distance == 0f;

    //FOV {
        public float TargetFov { get; set; } = 0;
        private float fovSpeed = 10f;
        [Property] public float baseFovSpeed { get; set; } = 10f;
    //}

    public void OnCameraAwake() {

        if (IsProxy) return;

        fovSpeed = baseFovSpeed;
        TargetFov = Preferences.FieldOfView;
        Camera.FieldOfView = TargetFov;
    }


    public void ApplyFov(float plusFov, float speed)
    {
        TargetFov += plusFov;
        
        fovSpeed = speed;
    }


    public void OnCameraUpdate()
    {
        if (IsProxy) return;

        // Rotate the head based on mouse movement
        var eyeAngles = EyeAngles;

        Input.AnalogLook *= InputSensitivity;
        eyeAngles.pitch += Input.AnalogLook.pitch;
        eyeAngles.yaw += Input.AnalogLook.yaw;
        eyeAngles += EyeAnglesOffset;
        EyeAnglesOffset = Angles.Zero;
        InputSensitivity = 1;

        eyeAngles.roll = 0;
        eyeAngles.pitch = eyeAngles.pitch.Clamp(-89.9f, 89.9f);

        EyeAngles = eyeAngles;

        // Set the current camera offset
        var targetOffset = Vector3.Zero;

        if (IsCrouching) targetOffset += Vector3.Down * 32f;
        EyeOffset = Vector3.Lerp(EyeOffset, targetOffset, Time.Delta * 10f);

        // Set position of the camera
        if (Scene.Camera is not null)
        {
            var camPos = EyePos;
            if (!IsFirstPerson)
            {
                // Perform a trace backwards to see where we can safely place the camera
                var camForward = eyeAngles.ToRotation().Forward;
                var camTrace = Scene.Trace.Ray(camPos, camPos - (camForward * Distance))
                    .WithoutTags(TagsHelper.Player, TagsHelper.Trigger, TagsHelper.ViewModel, TagsHelper.Weapon)
                    .Run();

                if (camTrace.Hit)
                {
                    // Add normal to prevent clipping
                    camPos = camTrace.HitPosition + camTrace.Normal;
                }
                else
                {
                    camPos = camTrace.EndPosition;
                }
            }

            //Fov
            Camera.FieldOfView = MathX.LerpTo(Camera.FieldOfView, TargetFov, fovSpeed * RealTime.Delta);

            TargetFov = Preferences.FieldOfView;
            fovSpeed = baseFovSpeed;

            //Player run fow multiply
            ApplyFov(Velocity.WithZ(0).Length / 60, 10);



            // Offsets
            camPos += PosOffset;
            PosOffset = Vector3.Zero;

            eyeAngles += AnglesOffset;
            AnglesOffset = Angles.Zero;


            // Set the position of the camera to our calculated position
            Camera.Transform.Position = camPos;
            Camera.Transform.Rotation = eyeAngles.ToRotation();
        }
    }
}
