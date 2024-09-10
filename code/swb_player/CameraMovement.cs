using SWB.Shared;

namespace SWB.Player;

[Group("SWB")]
[Title("CameraMovement")]
public class CameraMovement : Component
{
    [Property] public float Distance { get; set; } = 0f;
    [Property] public PlayerBase Player { get; set; }
    [Property] public GameObject Body { get; set; }
    [Property] public GameObject Head { get; set; }
    [Property] public SkinnedModelRenderer BodyRenderer { get; set; }
    public float InputSensitivity { get; set; } = 1f;

    // Eye Offset (resets after applying)
    public Angles EyeAnglesOffset { get; set; }

    // Camera Offsets (resets after applying)
    public Angles AnglesOffset { get; set; }
    public Vector3 PosOffset { get; set; }

    //FOV {
        public float TargetFov { get; set; } = 0;
        private float fovSpeed = 10f;
        [Property] public float baseFovSpeed { get; set; } = 10f;
    //}

    public bool IsFirstPerson => Distance == 0f;

    protected override void OnAwake() {

        if (IsProxy) return;

        fovSpeed = baseFovSpeed;
        TargetFov = Preferences.FieldOfView;
        Player.Camera.FieldOfView = TargetFov;
    }

    protected override void OnStart()
    {
        if (Player.IsBot)
            Enabled = false;
    }

    public void ApplyFov(float plusFov, float speed)
    {
        TargetFov += plusFov;

        fovSpeed = speed;
    }




    protected override void OnUpdate()
    {
        if (IsProxy) return;

        // Rotate the head based on mouse movement
        var eyeAngles = Player.EyeAngles;

        Input.AnalogLook *= InputSensitivity;
        eyeAngles.pitch += Input.AnalogLook.pitch;
        eyeAngles.yaw += Input.AnalogLook.yaw;
        eyeAngles += EyeAnglesOffset;
        EyeAnglesOffset = Angles.Zero;
        InputSensitivity = 1;

        eyeAngles.roll = 0;
        eyeAngles.pitch = eyeAngles.pitch.Clamp(-89.9f, 89.9f);

        Player.EyeAngles = eyeAngles;

        // Set the current camera offset
        var targetOffset = Vector3.Zero;
        if (Player.IsCrouching) targetOffset += Vector3.Down * 32f;
        Player.EyeOffset = Vector3.Lerp(Player.EyeOffset, targetOffset, Time.Delta * 10f);

        // Set position of the camera
        if (Scene.Camera is not null)
        {
            var camPos = Player.EyePos;
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
            Player.Camera.FieldOfView = MathX.LerpTo(Player.Camera.FieldOfView, TargetFov, fovSpeed * RealTime.Delta);

            TargetFov = Preferences.FieldOfView;
            fovSpeed = baseFovSpeed;

            //Player run fow multiply
            ApplyFov(Player.Velocity.WithZ(0).Length / 60, 10);



            // Offsets
            camPos += PosOffset;
            PosOffset = Vector3.Zero;

            eyeAngles += AnglesOffset;
            AnglesOffset = Angles.Zero;

            // Set the position of the camera to our calculated position
            Player.Camera.Transform.Position = camPos;
            Player.Camera.Transform.Rotation = eyeAngles.ToRotation();
        }
    }
}