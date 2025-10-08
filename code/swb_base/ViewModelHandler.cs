using SWB.Shared;
using System;

namespace SWB.Base;

public class ViewModelHandler : Component
{
	public ModelRenderer ViewModelRenderer { get; set; }
	public SkinnedModelRenderer ViewModelHandsRenderer { get; set; }
	public Weapon Weapon { get; set; }
	public CameraComponent Camera { get; set; }
	public bool ShouldDraw { get; set; }

	// Editor
	public bool EditorMode { get; set; }
	public AngPos EditorOffset { get; set; }
	public float EditorFOV { get; set; }

	IPlayerBase player => Weapon.Owner;

	float animSpeed = 1;
	float weaponFOVSpeed = 1;

	// Target animation values
	Vector3 targetVectorPos;
	Vector3 targetVectorRot;
	float targetWeaponFOV = -1;

	// Finalized animation values
	Vector3 finalVectorPos;
	Vector3 finalVectorRot;
	float finalWeaponFOV;

	// Sway
	Rotation lastEyeRot;

	// Jumping Animation
	float jumpTime;
	float landTime;

	// Aim animation
	float aimTime;

	// Helpful values
	Vector3 localVel;
	bool isAiming;

	protected override void OnDestroy()
	{
		if ( !player.IsFirstPerson ) return;
		player.FieldOfView = Screen.CreateVerticalFieldOfView( Preferences.FieldOfView );
	}

	protected override void OnDisabled()
	{
		// Reinitialize all target values when enabled
		targetWeaponFOV = -1;
	}

	protected override void OnUpdate()
	{
		var renderType = ShouldDraw ? ModelRenderer.ShadowRenderType.Off : ModelRenderer.ShadowRenderType.ShadowsOnly;
		ViewModelRenderer.Enabled = player.IsFirstPerson;
		ViewModelRenderer.RenderType = renderType;

		if ( ViewModelHandsRenderer is not null )
		{
			ViewModelHandsRenderer.Enabled = player.IsFirstPerson;
			ViewModelHandsRenderer.RenderType = renderType;
		}

		if ( !player.IsFirstPerson ) return;

		// For particles & lighting
		Camera.WorldPosition = Scene.Camera.WorldPosition;
		Camera.WorldRotation = Scene.Camera.WorldRotation;

		if ( targetWeaponFOV == -1 )
		{
			targetWeaponFOV = Weapon.ViewModelFOV;
			finalWeaponFOV = Weapon.ViewModelFOV;
		}

		WorldPosition = Camera.WorldPosition;
		WorldRotation = Camera.WorldRotation;

		// Smoothly transition the vectors with the target values
		finalVectorPos = finalVectorPos.LerpTo( targetVectorPos, animSpeed * RealTime.Delta );
		finalVectorRot = finalVectorRot.LerpTo( targetVectorRot, animSpeed * RealTime.Delta );
		finalWeaponFOV = MathX.LerpTo( finalWeaponFOV, targetWeaponFOV, weaponFOVSpeed * animSpeed * RealTime.Delta );
		animSpeed = 10 * Weapon.AnimSpeed;

		// Change the angles and positions of the viewmodel with the new vectors
		WorldRotation *= Rotation.From( finalVectorRot.x, finalVectorRot.y, finalVectorRot.z );
		// Position has to be set after rotation!
		WorldPosition += finalVectorPos.z * WorldRotation.Up + finalVectorPos.y * WorldRotation.Forward + finalVectorPos.x * WorldRotation.Right;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( finalWeaponFOV );

		// Initialize the target vectors for this frame
		targetVectorPos = Vector3.Zero;
		targetVectorRot = Vector3.Zero;
		targetWeaponFOV = Weapon.ViewModelFOV;

		// Editor mode
		if ( EditorMode )
		{
			targetVectorRot += MathUtil.ToVector3( EditorOffset.Angle );
			targetVectorPos += EditorOffset.Pos;
			targetWeaponFOV = EditorFOV;
			return;
		}

		// I'm sure there's something already that does this for me, but I spend an hour
		// searching through the wiki and a bunch of other garbage and couldn't find anything...
		// So I'm doing it manually. Problem solved.
		var eyeRot = player.EyeAngles.ToRotation();
		localVel = new Vector3( eyeRot.Right.Dot( player.Velocity ), eyeRot.Forward.Dot( player.Velocity ), player.Velocity.z );

		HandleIdleAnimation();
		HandleWalkAnimation();
		HandleJumpAnimation();

		// Tucking
		isAiming = !Weapon.ShouldTuckVar && Weapon.IsAiming;
		if ( Weapon.RunAnimData != AngPos.Zero && Weapon.ShouldTuckVar )
		{
			var animationCompletion = Math.Min( 1, ((Weapon.TuckRange - Weapon.TuckDist) / Weapon.TuckRange) + 0.5f );
			targetVectorPos += Weapon.RunAnimData.Pos * animationCompletion;
			targetVectorRot += MathUtil.ToVector3( Weapon.RunAnimData.Angle * animationCompletion );
			return;
		}

		HandleSwayAnimation();
		HandleIronAnimation();
		HandleSprintAnimation();
		HandleCustomizeAnimation();
	}

	void HandleIdleAnimation()
	{
		// No swaying if aiming
		if ( isAiming )
			return;

		// Perform a "breathing" animation
		var breatheTime = RealTime.Now * 2.0f;
		targetVectorPos -= new Vector3( MathF.Cos( breatheTime / 4.0f ) / 8.0f, 0.0f, -MathF.Cos( breatheTime / 4.0f ) / 32.0f );
		targetVectorRot -= new Vector3( MathF.Cos( breatheTime / 5.0f ), MathF.Cos( breatheTime / 4.0f ), MathF.Cos( breatheTime / 7.0f ) );

		// Crouching animation
		if ( Input.Down( InputButtonHelper.Duck ) && player.IsOnGround )
			targetVectorPos += new Vector3( -1.0f, -1.0f, 0.5f );
	}


	void HandleWalkAnimation()
	{
		var breatheTime = RealTime.Now * 16.0f;
		var walkSpeed = new Vector3( player.Velocity.x, player.Velocity.y, 0.0f ).Length;
		var maxWalkSpeed = 200.0f;
		var roll = 0.0f;
		var yaw = 0.0f;

		// Check if on the ground
		if ( !player.IsOnGround )
			return;

		// Check if sprinting
		if ( player.IsRunning )
		{
			breatheTime = RealTime.Now * 18.0f;
			maxWalkSpeed = 100.0f;
		}

		// Check for sideways velocity to sway the gun slightly
		if ( isAiming || localVel.x > 0.0f )
			roll = -7.0f * (localVel.x / maxWalkSpeed);
		else if ( localVel.x < 0.0f )
			yaw = 3.0f * (localVel.x / maxWalkSpeed);

		// Check if ADS & firing
		if ( isAiming && Weapon.TimeSincePrimaryShoot < 0.1f )
		{
			targetVectorRot -= new Vector3( 0, 0, roll );
			return;
		}

		// Perform walk cycle
		targetVectorPos -= new Vector3( (-MathF.Cos( breatheTime / 2.0f ) / 5.0f) * walkSpeed / maxWalkSpeed - yaw / 4.0f, 0.0f, 0.0f );
		targetVectorRot -= new Vector3( (Math.Clamp( MathF.Cos( breatheTime ), -0.3f, 0.3f ) * 2.0f) * walkSpeed / maxWalkSpeed, (-MathF.Cos( breatheTime / 2.0f ) * 1.2f) * walkSpeed / maxWalkSpeed - yaw * 1.5f, roll );
	}


	void HandleSwayAnimation()
	{
		var swayspeed = 5;

		// Fix the sway faster if we're ironsighting
		if ( isAiming )
			swayspeed = 20;

		// Lerp the eye position
		lastEyeRot = Rotation.Lerp( lastEyeRot, player.Camera.WorldRotation, swayspeed * RealTime.Delta );

		// Calculate the difference between our current eye angles and old (lerped) eye angles
		var angDif = player.Camera.WorldRotation.Angles() - lastEyeRot.Angles();
		angDif = new Angles( angDif.pitch, MathX.RadianToDegree( MathF.Atan2( MathF.Sin( MathX.DegreeToRadian( angDif.yaw ) ), MathF.Cos( MathX.DegreeToRadian( angDif.yaw ) ) ) ), 0 );

		// Perform sway
		targetVectorPos += new Vector3( Math.Clamp( angDif.yaw * 0.04f, -1.5f, 1.5f ), 0.0f, Math.Clamp( angDif.pitch * 0.04f, -1.5f, 1.5f ) );
		targetVectorRot += new Vector3( Math.Clamp( angDif.pitch * 0.2f, -4.0f, 4.0f ), Math.Clamp( angDif.yaw * 0.2f, -4.0f, 4.0f ), 0.0f );
	}


	void HandleIronAnimation()
	{
		if ( isAiming && !Weapon.IsReloading && Weapon.AimAnimData != AngPos.Zero )
		{
			var speedMod = 1f;
			if ( aimTime == 0 )
			{
				aimTime = RealTime.Now;
			}

			var timeDiff = RealTime.Now - aimTime;

			// Mod only while actively scoping
			if ( Weapon.IsScoping || (!Weapon.IsScoping && timeDiff < 0.2f) )
			{
				speedMod = timeDiff * 10;
			}

			animSpeed = 10 * Weapon.AnimSpeed * speedMod;
			targetVectorPos += Weapon.AimAnimData.Pos;
			targetVectorRot += MathUtil.ToVector3( Weapon.AimAnimData.Angle );

			if ( Weapon.AimInfo.ViewModelFOV > 0 )
				targetWeaponFOV = Weapon.AimInfo.ViewModelFOV;

			weaponFOVSpeed = Weapon.AimInfo.AimInFOVSpeed;
		}
		else
		{
			aimTime = 0;
			targetWeaponFOV = Weapon.ViewModelFOV;
		}
	}

	void HandleSprintAnimation()
	{
		if ( Weapon.IsRunning && Weapon.RunAnimData != AngPos.Zero && !Weapon.IsCustomizing )
		{
			targetVectorPos += Weapon.RunAnimData.Pos;
			targetVectorRot += MathUtil.ToVector3( Weapon.RunAnimData.Angle );
		}
	}


	void HandleCustomizeAnimation()
	{
		if ( Weapon.IsCustomizing && Weapon.CustomizeAnimData != AngPos.Zero )
		{
			targetVectorPos += Weapon.CustomizeAnimData.Pos;
			targetVectorRot += MathUtil.ToVector3( Weapon.CustomizeAnimData.Angle );
		}
	}

	void HandleJumpAnimation()
	{
		// If we're not on the ground, reset the landing animation time
		if ( !player.IsOnGround )
			landTime = RealTime.Now + 0.31f;

		// Reset the timers once they elapse
		if ( landTime < RealTime.Now && landTime != 0.0f )
		{
			landTime = 0.0f;
			jumpTime = 0.0f;
		}

		// If we jumped, start the animation
		if ( Input.Down( InputButtonHelper.Jump ) && jumpTime == 0.0f )
		{
			jumpTime = RealTime.Now + 0.31f;
			landTime = 0.0f;
		}

		// If we're not ironsighting, do a fancy jump animation
		if ( !isAiming )
		{
			if ( jumpTime > RealTime.Now )
			{
				// If we jumped, do a curve upwards
				var f = 0.31f - (jumpTime - RealTime.Now);
				var xx = MathUtil.BezierY( f, 0.0f, -4.0f, 0.0f );
				var yy = 0.0f;
				var zz = MathUtil.BezierY( f, 0.0f, -2.0f, -5.0f );
				var pt = MathUtil.BezierY( f, 0.0f, -4.36f, 10.0f );
				var yw = xx;
				var rl = MathUtil.BezierY( f, 0.0f, -10.82f, -5.0f );
				targetVectorPos += new Vector3( xx, yy, zz ) / 4.0f;
				targetVectorRot += new Vector3( pt, yw, rl ) / 4.0f;
				animSpeed = 20.0f;
			}
			else if ( !player.IsOnGround )
			{
				// Shaking while falling
				var breatheTime = RealTime.Now * 30.0f;
				targetVectorPos += new Vector3( MathF.Cos( breatheTime / 2.0f ) / 16.0f, 0.0f, -5.0f + (MathF.Sin( breatheTime / 3.0f ) / 16.0f) ) / 4.0f;
				targetVectorRot += new Vector3( 10.0f - (MathF.Sin( breatheTime / 3.0f ) / 4.0f), MathF.Cos( breatheTime / 2.0f ) / 4.0f, -5.0f ) / 4.0f;
				animSpeed = 20.0f;
			}
			else if ( landTime > RealTime.Now )
			{
				// If we landed, do a fancy curve downwards
				var f = landTime - RealTime.Now;
				var xx = MathUtil.BezierY( f, 0.0f, -4.0f, 0.0f );
				var yy = 0.0f;
				var zz = MathUtil.BezierY( f, 0.0f, -2.0f, -5.0f );
				var pt = MathUtil.BezierY( f, 0.0f, -4.36f, 10.0f );
				var yw = xx;
				var rl = MathUtil.BezierY( f, 0.0f, -10.82f, -5.0f );
				targetVectorPos += new Vector3( xx, yy, zz ) / 2.0f;
				targetVectorRot += new Vector3( pt, yw, rl ) / 2.0f;
				animSpeed = 20.0f;
			}
		}
		else
			targetVectorPos += new Vector3( 0.0f, 0.0f, Math.Clamp( localVel.z / 1000.0f, -1.0f, 1.0f ) );
	}

}
