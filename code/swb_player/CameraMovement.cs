using SWB.Shared;

namespace SWB.Player;

[Group( "SWB" )]
[Title( "CameraMovement" )]
public class CameraMovement : Component
{
	[Property] public float Distance { get; set; } = 0f;
	[Property] public PlayerBase Player { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	public float InputSensitivity { get; set; } = 1f;
	public Angles EyeAnglesOffset { get; set; }
	public bool IsFirstPerson => Distance == 0f;

	protected override void OnAwake() { }

	protected override void OnStart()
	{
		if ( Player.IsBot )
			Enabled = false;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		// Rotate the head based on mouse movement
		var eyeAngles = Player.EyeAngles;

		Input.AnalogLook *= InputSensitivity;
		eyeAngles.pitch += Input.AnalogLook.pitch;
		eyeAngles.yaw += Input.AnalogLook.yaw;
		eyeAngles += EyeAnglesOffset;
		EyeAnglesOffset = Angles.Zero;
		InputSensitivity = 1;

		eyeAngles.roll = 0;
		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89.9f, 89.9f );

		Player.EyeAngles = eyeAngles;

		// Set the current camera offset
		var targetOffset = Vector3.Zero;
		if ( Player.IsCrouching ) targetOffset += Vector3.Down * 32f;
		Player.EyeOffset = Vector3.Lerp( Player.EyeOffset, targetOffset, Time.Delta * 10f );

		// Set position of the camera
		if ( Scene.Camera is not null )
		{
			var camPos = Player.EyePos;
			if ( !IsFirstPerson )
			{
				// Perform a trace backwards to see where we can safely place the camera
				var camForward = eyeAngles.ToRotation().Forward;
				var camTrace = Scene.Trace.Ray( camPos, camPos - (camForward * Distance) )
					.WithoutTags( TagsHelper.Player, TagsHelper.Trigger, TagsHelper.ViewModel, TagsHelper.Weapon )
					.Run();

				if ( camTrace.Hit )
				{
					// Add normal to prevent clipping
					camPos = camTrace.HitPosition + camTrace.Normal;
				}
				else
				{
					camPos = camTrace.EndPosition;
				}
			}

			// Set the position of the camera to our calculated position
			Player.Camera.Transform.Position = camPos;
			Player.Camera.Transform.Rotation = eyeAngles.ToRotation();
		}
	}
}
