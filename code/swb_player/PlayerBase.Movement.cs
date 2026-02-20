using Sandbox.Citizen;
using SWB.Shared;
using System;
using System.Collections.Generic;
using static Sandbox.SceneModel;

namespace SWB.Player;

public partial class PlayerBase
{
	[Property] public float GroundControl { get; set; } = 4.0f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 50f;
	[Property] public float RunSpeed { get; set; } = 290f;
	[Property] public float WalkSpeed { get; set; } = 160f;
	[Property] public float CrouchSpeed { get; set; } = 90f;
	[Property] public float JumpForce { get; set; } = 350f;
	[Property] public float NoclipSpeed { get; set; } = 5f;

	/// <summary>Blocks jump when jumping quickly in succession</summary>
	[Property] public bool JumpSpamPrevention { get; set; } = true;

	[Property, Category( "Falling" )] public float SafeFallSpeed { get; set; } = 500f;
	[Property, Category( "Falling" )] public float LethalFallSpeed { get; set; } = 700f;
	[Property, Category( "Falling" )] public float MaxFallDamage { get; set; } = 100f;
	[Property, Category( "Falling" )] public float FallDamageExponent { get; set; } = 2f;

	[Sync] public Vector3 WishVelocity { get; set; } = Vector3.Zero;
	[Sync] public Vector3 FallingVelocity { get; set; } = Vector3.Zero;
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public Vector3 EyeOffset { get; set; } = Vector3.Zero;
	[Sync] public bool IsCrouching { get; set; } = false;
	[Sync] public bool IsRunning { get; set; } = false;
	[Sync] public bool CanMove { get; set; } = true;
	[Sync] public bool Noclip { get; private set; } = false;
	[Sync] public bool IsUsingController { get; set; }

	public TimeSince TimeSinceAirborne { get; set; }
	public bool IsOnGround => CharacterController?.IsOnGround ?? true;
	public Vector3 Velocity => CharacterController?.Velocity ?? Vector3.Zero;
	public Vector3 EyePos => Head.WorldPosition + EyeOffset;
	public Angles EyeAnglesNormal => EyeAngles.Normal;

	public CharacterController CharacterController { get; set; }
	public CitizenAnimationHelper AnimationHelper { get; set; }
	public HoldTypes HoldType
	{
		set { AnimationHelper.HoldType = (CitizenAnimationHelper.HoldTypes)value; }
	}

	public CapsuleCollider BodyCollider { get; set; }

	HashSet<string> stickyButtons = new( StringComparer.Ordinal );
	HashSet<string> stickyActiveButtons = new( StringComparer.Ordinal );
	TimeSince timeSinceStickyRunStart = 0;

	TimeSince timeSinceLastFootstep = 0;
	bool groundedCheck = true;

	public void ToggleNoclip()
	{
		Noclip = !Noclip;

		if ( Noclip ) Tags.Add( TagsHelper.Trigger );
		else Tags.Remove( TagsHelper.Trigger );

		BodyRenderer.Set( "b_noclip", Noclip );
	}

	public virtual void OnInputDeviceSwitch()
	{
		IsUsingController = Input.UsingController;
		stickyButtons.Clear();
		stickyActiveButtons.Clear();

		if ( IsUsingController )
		{
			stickyButtons.Add( InputButtonHelper.Duck );
			stickyButtons.Add( InputButtonHelper.Run );
		}
	}

	bool InputIsDownOrPressed( string button )
	{
		return !IsUsingController ? Input.Down( button ) : Input.Pressed( button );
	}

	void OnMovementAwake()
	{
		CharacterController = Components.Get<CharacterController>();
		AnimationHelper = Components.Get<CitizenAnimationHelper>();
		BodyCollider = Body.Components.Get<CapsuleCollider>();

		if ( BodyRenderer is not null )
			BodyRenderer.OnFootstepEvent += OnAnimEventFootstep;
	}

	public virtual void OnMovementUpdate()
	{
		if ( !IsProxy && !IsBot )
		{
			UpdateRun();

			if ( Input.Pressed( InputButtonHelper.Jump ) )
			{
				if ( IsClimbingLadder )
					LadderJump();
				else
					Jump();
			}

			UpdateCrouch();
		}

		if ( !IsOnGround )
		{
			TimeSinceAirborne = 0;
			FallingVelocity = Velocity;
		}

		if ( IsOnGround && groundedCheck != IsOnGround )
		{
			groundedCheck = IsOnGround;
			OnGrounded( FallingVelocity );
		}
		else if ( !IsOnGround )
		{
			groundedCheck = false;
		}

		RotateBody();
		UpdateAnimations();
	}

	public virtual void OnMovementFixedUpdate()
	{
		if ( IsProxy ) return;
		if ( !IsBot )
			BuildWishVelocity();

		if ( Noclip )
			NoclipMove();
		else if ( IsClimbingLadder )
			LadderMove();
		else
			Move();
	}

	void BuildWishVelocity()
	{
		WishVelocity = 0;
		if ( !CanMove ) return;

		var rot = Camera.WorldRotation; // = EyeAngles in firstperson | = Camera.WorldRotation in thirdperson
		WishVelocity += rot * Input.AnalogMove;

		if ( !Noclip )
			WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( IsCrouching ) WishVelocity *= CrouchSpeed;
		else if ( IsRunning ) WishVelocity *= RunSpeed;
		else WishVelocity *= WalkSpeed;

		// Mobility from item
		if ( Inventory.ActiveItem is not null )
			WishVelocity *= Inventory.ActiveItem.Mobility;

		// Impact from bullets, etc.
		WishVelocity *= movementImpact;
	}

	void NoclipMove()
	{
		var speedMod = 1f;

		if ( Input.Down( InputButtonHelper.Run ) )
			speedMod = 2f;
		if ( Input.Down( InputButtonHelper.Duck ) )
			speedMod = 0.5f;

		WishVelocity *= NoclipSpeed * speedMod;

		if ( Input.Down( InputButtonHelper.Jump ) )
			WishVelocity += Vector3.Up * NoclipSpeed * speedMod * 100;

		CharacterController.Velocity = WishVelocity;

		if ( !CharacterController.Velocity.IsNearZeroLength )
			CharacterController.Move();
	}

	void Move()
	{
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( IsOnGround )
		{
			// Friction / Acceleration
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
			CharacterController.Accelerate( WishVelocity );
			CharacterController.ApplyFriction( GroundControl );
		}
		else
		{
			// Air control / Gravity
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
			CharacterController.Accelerate( WishVelocity.ClampLength( MaxForce ) );
			CharacterController.ApplyFriction( AirControl );
		}

		if ( !(CharacterController.Velocity.IsNearZeroLength && WishVelocity.IsNearZeroLength) )
			CharacterController.Move();

		// Second half of gravity after movement (to stay accurate)
		if ( IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
		}
	}

	void RotateBody()
	{
		var targetRot = new Angles( 0, EyeAngles.ToRotation().Yaw(), 0 ).ToRotation();
		float rotateDiff = Body.WorldRotation.Distance( targetRot );

		if ( rotateDiff > 20f || CharacterController.Velocity.Length > 10f )
		{
			Body.WorldRotation = Rotation.Lerp( Body.WorldRotation, targetRot, Time.Delta * 2f );
		}
	}

	void Jump()
	{
		if ( !IsOnGround ) return;

		if ( JumpSpamPrevention && TimeSinceAirborne < 0.2f )
			return;

		var jumpVelocity = Vector3.Up * JumpForce;

		// Sound
		var tr = GetSurfaceTrace();
		if ( tr.Hit )
			PlayFootLaunchSound( tr.Surface, jumpVelocity );

		CharacterController.Punch( jumpVelocity );
		AnimationHelper?.TriggerJump();

		// Unstick crouch
		if ( IsCrouching )
			stickyActiveButtons.Remove( InputButtonHelper.Duck );

		// Unstick run
		if ( IsRunning )
			stickyActiveButtons.Remove( InputButtonHelper.Run );
	}

	/// <summary>Called once when the player lands</summary>
	public virtual void OnGrounded( Vector3 velocity )
	{
		var tr = GetSurfaceTrace();
		if ( tr.Hit )
			PlayFootLandSound( tr.Surface, velocity );

		if ( !IsProxy )
		{
			ShakeScreen( new()
			{
				Size = 0.2f,
				Rotation = 0.2f,
				Duration = 0.1f,
			} );

			DoFallDamage( velocity );
		}
	}

	void UpdateAnimations()
	{
		if ( AnimationHelper is null ) return;

		AnimationHelper.WithWishVelocity( WishVelocity );
		AnimationHelper.WithVelocity( CharacterController.Velocity );
		AnimationHelper.AimAngle = EyeAngles.ToRotation();
		AnimationHelper.IsGrounded = IsOnGround || IsClimbingLadder;
		AnimationHelper.WithLook( EyeAngles.ToRotation().Forward, 1f, 0.75f, 0.5f );
		AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		AnimationHelper.DuckLevel = IsCrouching ? 1 : 0;
	}

	void UpdateRun()
	{
		var runIsDownOrPressed = InputIsDownOrPressed( InputButtonHelper.Run );
		var runIsStickyActive = stickyActiveButtons.Contains( InputButtonHelper.Run );

		// Velocity unstick
		var speed = Velocity.WithZ( 0 );
		if ( !IsCrouching && timeSinceStickyRunStart > 0.2 && speed.LengthSquared < 20000 )
		{
			runIsStickyActive = false;
			stickyActiveButtons.Remove( InputButtonHelper.Run );
		}

		// Stick
		if ( IsUsingController && runIsDownOrPressed )
		{
			runIsStickyActive = !runIsStickyActive;

			if ( runIsStickyActive )
			{
				timeSinceStickyRunStart = 0;
				stickyActiveButtons.Add( InputButtonHelper.Run );
			}
			else
				stickyActiveButtons.Remove( InputButtonHelper.Run );
		}

		// Unstick crouch
		if ( runIsStickyActive && IsCrouching )
		{
			stickyActiveButtons.Remove( InputButtonHelper.Duck );
		}

		IsRunning = runIsDownOrPressed || runIsStickyActive;
	}

	void UpdateCrouch()
	{
		var duckIsDownOrPressed = InputIsDownOrPressed( InputButtonHelper.Duck );
		var duckIsStickyActive = stickyActiveButtons.Contains( InputButtonHelper.Duck );

		// Unstick
		if ( duckIsDownOrPressed && duckIsStickyActive )
		{
			duckIsStickyActive = false;
			stickyActiveButtons.Remove( InputButtonHelper.Duck );
		}

		if ( duckIsDownOrPressed && !IsCrouching && !IsRunning && IsOnGround && !IsClimbingLadder )
		{
			IsCrouching = true;
			CharacterController.Height /= 2f;
			BodyCollider.End = BodyCollider.End.WithZ( BodyCollider.End.z / 2f );

			if ( stickyButtons.Contains( InputButtonHelper.Duck ) )
			{
				stickyActiveButtons.Add( InputButtonHelper.Duck );
			}
		}

		if ( duckIsStickyActive ) return;

		if ( IsCrouching && (!duckIsDownOrPressed || !IsOnGround) )
		{
			// Check we have space to uncrouch
			var targetHeight = CharacterController.Height + 4;
			var upTrace = CharacterController.TraceDirection( Vector3.Up * targetHeight );

			if ( !upTrace.Hit )
			{
				IsCrouching = false;
				CharacterController.Height *= 2;
				BodyCollider.End = BodyCollider.End.WithZ( BodyCollider.End.z * 2f );
			}
		}
	}

	void OnAnimEventFootstep( SceneModel.FootstepEvent footstepEvent )
	{
		if ( !IsAlive || !IsOnGround ) return;

		// Walk
		var stepDelay = 0.25f;
		var speed = Velocity.WithZ( 0 );

		// Standing still
		if ( speed.IsNearlyZero( 0.01f ) && TimeSinceAirborne > 0.1f ) return;

		// Running
		if ( Velocity.WithZ( 0 ).Length >= 200 )
		{
			stepDelay = 0.2f;
		}
		// Crouching
		else if ( IsCrouching )
		{
			stepDelay = 0.4f;
		}

		if ( timeSinceLastFootstep < stepDelay )
			return;

		var tr = Scene.Trace.Ray( footstepEvent.Transform.Position, footstepEvent.Transform.Position + Vector3.Down * 20 )
			.Radius( 1 )
			.IgnoreGameObjectHierarchy( this.GameObject )
			.Run();

		if ( !tr.Hit ) return;

		PlayFootstepSound( tr.Surface, footstepEvent );
		timeSinceLastFootstep = 0;
	}

	public SceneTraceResult GetSurfaceTrace()
	{
		return Scene.Trace.Ray( WorldPosition, WorldPosition + Vector3.Down * 1000 )
			.Radius( 1 )
			.IgnoreGameObjectHierarchy( this.GameObject )
			.Run();
	}

	public void PlaySoundEvent( Sandbox.SoundEvent soundEvent, string fallback, float dist )
	{
		SoundHandle soundHandle = null;

		if ( soundEvent is not null )
			soundHandle = Sound.Play( soundEvent );

		soundHandle ??= Sound.Play( fallback );
		soundHandle.Distance = dist;
		soundHandle.Position = WorldPosition;
	}

	public virtual void PlayFootstepSound( Surface surface, FootstepEvent footstepEvent )
	{
		PlaySoundEvent( surface?.SoundCollection.FootRight, "footstep-concrete", 7500 );
	}

	[Rpc.Broadcast( NetFlags.Unreliable )]
	public virtual void PlayFootLaunchSound( Surface surface, Vector3 velocity )
	{
		PlaySoundEvent( surface?.SoundCollection.FootLaunch, "footstep-concrete-jump", 7500 );
	}

	public virtual void PlayFootLandSound( Surface surface, Vector3 velocity )
	{
		PlaySoundEvent( surface?.SoundCollection.FootLand, "footstep-concrete-land", 10000 );
	}

	public virtual void DoFallDamage( Vector3 impactVelocity )
	{
		var downSpeed = -impactVelocity.z;
		if ( downSpeed <= SafeFallSpeed ) return;

		// Normalize to 0..1 between safe and lethal
		var t = (downSpeed - SafeFallSpeed) / Math.Max( 1f, (LethalFallSpeed - SafeFallSpeed) );
		t = Math.Clamp( t, 0f, 1f );

		var curved = MathF.Pow( t, FallDamageExponent );
		var damage = curved * MaxFallDamage;
		var damageInfo = new Shared.DamageInfo
		{
			Damage = damage,
			Force = impactVelocity,
		};

		TakeDamage( damageInfo );
	}
}
