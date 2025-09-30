using SWB.Shared;

namespace SWB.Player;

[Group( "SWB" )]
[Title( "CameraMovement" )]
public class CameraMovement : Component, ICameraMovement
{
	[Property] public float Distance { get; set; } = 0f;

	/// <summary>Thirdperson shoulder offset</summary>
	[Property] public float ShoulderOffset { get; set; } = 30f;

	[Property] public PlayerBase Player { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	public float InputSensitivity { get; set; } = 1f;

	// <summary> Eye Offset (resets after applying)
	public Angles EyeAnglesOffset { get; set; }

	// Camera Offsets (resets after applying)
	public Angles AnglesOffset { get; set; }
	public Vector3 PosOffset { get; set; }

	float distanceLerp;
	float shoulderOffsetLerp;

	public bool IsFirstPerson
	{
		get
		{
			return Distance == 0f;
		}
	}

	protected override void OnAwake() { }

	protected override void OnStart()
	{
		if ( Player.IsBot )
			Enabled = false;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy || Scene.Camera is null ) return;
		if ( IsFirstPerson )
			FirstPerson();
		else
			Thirdperson();
	}

	public virtual void FirstPerson()
	{
		var eyePos = Player.EyePos;
		var eyeAngles = Player.EyeAngles;

		Input.AnalogLook *= InputSensitivity;
		eyeAngles.pitch += Input.AnalogLook.pitch;
		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89.9f, 89.9f );
		eyeAngles.yaw += Input.AnalogLook.yaw;
		eyeAngles.roll = 0;
		eyeAngles += EyeAnglesOffset;

		EyeAnglesOffset = Angles.Zero;
		InputSensitivity = 1;
		Player.EyeAngles = eyeAngles;

		// Reset thirdperson offsets
		distanceLerp = 0;
		shoulderOffsetLerp = 0;

		// Set the current camera offset
		var targetOffset = Vector3.Zero;
		if ( Player.IsCrouching ) targetOffset += Vector3.Down * 32f;
		Player.EyeOffset = Vector3.Lerp( Player.EyeOffset, targetOffset, Time.Delta * 10f );

		// Offsets
		eyePos += PosOffset;
		PosOffset = Vector3.Zero;

		eyeAngles += AnglesOffset;
		AnglesOffset = Angles.Zero;

		// Set the position of the camera to our calculated position
		Player.Camera.WorldPosition = eyePos;
		Player.Camera.WorldRotation = eyeAngles;
	}

	public virtual void Thirdperson()
	{
		var eyePos = Player.EyePos;
		var camPos = Player.Camera.WorldPosition;
		var camRot = Player.Camera.WorldRotation;
		var camTrace = Scene.Trace.Ray( camPos, camPos + (camRot.Forward * 999999) )
			.WithoutTags( TagsHelper.Clothing, TagsHelper.Trigger, TagsHelper.ViewModel, TagsHelper.Weapon )
			.IgnoreGameObjectHierarchy( Player.GameObject )
			.Run();
		var traceHitPos = camTrace.Hit ? camTrace.HitPosition : camTrace.EndPosition;

		// Direction from the player's eyes to the trace hit
		var dir = (traceHitPos - Player.EyePos).Normal;
		var eyeAngles = dir.EulerAngles.Normal;

		Input.AnalogLook *= InputSensitivity;
		var camPitch = (camRot.Pitch() + Input.AnalogLook.pitch).Clamp( -89.9f, 89.9f );
		var camAngles = new Angles( camPitch, Player.Camera.WorldRotation.Yaw() + Input.AnalogLook.yaw, 0 );
		camAngles += EyeAnglesOffset;
		camRot = camAngles.ToRotation();

		EyeAnglesOffset = Angles.Zero;
		InputSensitivity = 1;
		Player.EyeAngles = eyeAngles;

		// Firstperson only camera offset
		Player.EyeOffset = Vector3.Zero;

		// Thirdperson offsets
		distanceLerp = MathUtil.FILerp( distanceLerp, Distance, 8 );
		shoulderOffsetLerp = MathUtil.FILerp( shoulderOffsetLerp, ShoulderOffset, 8 );

		// Perform a trace backwards to see where we can safely place the camera
		var camSafeTrace = Scene.Trace.Ray( eyePos, eyePos - (camRot.Forward * distanceLerp) + (camRot.Right * shoulderOffsetLerp) )
			.WithoutTags( TagsHelper.Player, TagsHelper.Trigger, TagsHelper.ViewModel, TagsHelper.Weapon )
			.Run();

		if ( camSafeTrace.Hit )
			camPos = camSafeTrace.HitPosition + camSafeTrace.Normal * 4;
		else
			camPos = camSafeTrace.EndPosition;

		// Set the position of the camera to our calculated position
		Player.Camera.WorldPosition = camPos;
		Player.Camera.WorldRotation = camRot;
	}
}
