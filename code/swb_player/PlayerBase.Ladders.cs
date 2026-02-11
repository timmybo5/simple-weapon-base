using System;

namespace SWB.Player;

public partial class PlayerBase
{
	/// <summary>Enable the ability to climb ladders</summary>
	[Property] public bool LadderClimbing { get; set; } = true;
	[Sync] public bool IsClimbingLadder { get; set; }

	Collider activeLadder;
	float activeLadderTopZ;
	float activeLadderBottomZ;
	bool isFacingLadder;
	TimeSince timesinceLastClimbSound;

	public virtual void OnLadderEnter( Collider ladder )
	{
		if ( !LadderClimbing ) return;
		IsClimbingLadder = true;
		CharacterController.IsOnGround = true; // Jumping on and off ladder
		CharacterController.Velocity = Vector3.Zero;
		WishVelocity = Vector3.Zero;
		activeLadder = ladder;
		activeLadderTopZ = activeLadder.GetWorldBounds().Maxs.z;
		activeLadderBottomZ = activeLadder.GetWorldBounds().Mins.z;

		if ( IsAtTopOfLadder() )
			SnapToLadderFarFrontFace();
	}

	public virtual void OnLadderExit( Collider ladder )
	{
		if ( !LadderClimbing || !IsClimbingLadder ) return;
		IsClimbingLadder = false;
		activeLadder = null;
	}

	void LadderJump()
	{
		if ( activeLadder is null ) return;

		var viewFwd = Camera.WorldRotation.Forward.WithZ( 0 ).Normal;
		var horizDir = isFacingLadder ? -viewFwd : viewFwd;
		var jumpVelocity = (Vector3.Up * (JumpForce / 2)) + (horizDir * 100);

		PlayLadderJumpSound( activeLadder.Surface, jumpVelocity );

		// Exit ladder first
		OnLadderExit( activeLadder );

		CharacterController.Punch( jumpVelocity );
		AnimationHelper?.TriggerJump();
	}

	[Rpc.Broadcast( NetFlags.Unreliable )]
	public virtual void PlayLadderJumpSound( Surface surface, Vector3 velocity )
	{
		PlaySoundEvent( surface?.SoundCollection.FootLaunch, "footstep-metal", 7500 );
	}

	[Rpc.Broadcast( NetFlags.Unreliable )]
	public virtual void PlayLadderClimbSound( Surface surface, Vector3 velocity )
	{
		PlaySoundEvent( surface?.SoundCollection.FootLaunch, "footstep-metal", 7500 );
	}

	void LadderMove()
	{
		if ( activeLadder is null ) return;

		isFacingLadder = IsFacingLadder();
		var rot = Camera.WorldRotation;
		var fwd = rot.Forward.WithZ( 0 );
		var right = rot.Right.WithZ( 0 );

		if ( fwd.IsNearZeroLength || right.IsNearZeroLength )
			return;

		fwd = fwd.Normal;
		right = right.Normal;

		// Decompose WishVelocity
		var forwardSpeed = WishVelocity.Dot( fwd );
		var strafeSpeed = WishVelocity.Dot( right );

		// Normalize those speeds into -1..1 intent
		var maxPlanar = MathF.Max( 1f, WalkSpeed );
		var forwardIntent = (forwardSpeed / maxPlanar).Clamp( -1f, 1f );

		// Switch direction if looking up/down
		if ( !isFacingLadder )
			forwardIntent = -forwardIntent;

		var strafeIntent = (strafeSpeed / maxPlanar).Clamp( -1f, 1f );
		var climbSpeed = WalkSpeed;
		var climbVelocity =
			(Vector3.Up * (forwardIntent * climbSpeed)) +
			(right * (strafeIntent * climbSpeed));

		// Stop if not moving
		if ( climbVelocity.IsNearZeroLength ) return;

		// Top-out check before moving
		if ( TryTopOut( forwardIntent ) ) return;

		// Bottom-out check before moving
		if ( TryBottomOut( forwardIntent ) ) return;

		// Sound
		if ( timesinceLastClimbSound >= 0.25f )
		{
			PlayLadderClimbSound( activeLadder.Surface, climbVelocity );
			timesinceLastClimbSound = 0;
		}

		// MoveTo: bypasses grounded Z-cancels
		var target = WorldPosition + climbVelocity * Time.Delta;
		CharacterController.MoveTo( target, useStep: false );

		// Keep Velocity coherent for animations/other systems
		CharacterController.Velocity = climbVelocity;
	}

	bool TryBottomOut( float downIntent )
	{
		// Only when climbing down
		if ( downIntent >= -0.1f ) return false;

		var groundTr = Scene.Trace
			.Ray( WorldPosition + Vector3.Up * 4f, WorldPosition + Vector3.Down * 10f )
			.Radius( CharacterController.Radius * 0.9f )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !groundTr.Hit ) return false;

		PlayFootLandSound( groundTr.Surface, Vector3.Zero );
		OnLadderExit( activeLadder );
		CharacterController.MoveTo( groundTr.HitPosition, useStep: true );

		// Realistic walk off velocity
		CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );

		return true;
	}

	bool TryTopOut( float forwardIntent )
	{
		if ( IsAtBottomOfLadder() || IsAtTopOfLadder() ) return false;

		// Only when climbing up
		if ( forwardIntent <= 0.1f ) return false;

		// Up + away
		var viewFwd = Camera.WorldRotation.Forward.WithZ( 0 ).Normal;
		var away = isFacingLadder ? viewFwd : -viewFwd;
		var dismount = WorldPosition + Vector3.Up * CharacterController.Height + away * CharacterController.Radius;

		// Check for free space is free (capsule-ish sweep)
		var clearTr = Scene.Trace
			.Ray( WorldPosition + Vector3.Up * 16f, dismount + Vector3.Up * 16f )
			.Radius( CharacterController.Radius )
			.IgnoreGameObjectHierarchy( this.GameObject )
			.Run();

		if ( clearTr.Hit ) return false;

		// Check for ground under dismount point
		var groundTr = Scene.Trace
			.Ray( dismount + Vector3.Up * 8f, dismount + Vector3.Down * 72f )
			.Radius( CharacterController.Radius * 0.9f )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !groundTr.Hit ) return false;

		PlayFootLandSound( groundTr.Surface, Vector3.Zero );
		OnLadderExit( activeLadder );
		CharacterController.MoveTo( groundTr.HitPosition, useStep: true );

		// Realistic walk off velocity
		CharacterController.Velocity = away * WalkSpeed * 0.5f;

		return true;
	}

	void SnapToLadderFarFrontFace( float gap = 5f )
	{
		if ( activeLadder is null ) return;

		var lb = activeLadder.GetWorldBounds();
		var pb = CharacterController.BoundingBox;
		var pc = pb.Center;
		var pe = (pb.Size * 0.5f);

		// Use view yaw to decide whether front/back is X or Y (prevents side snapping)
		var viewFwd = Camera.WorldRotation.Forward.WithZ( 0 );
		if ( viewFwd.IsNearZeroLength ) viewFwd = Vector3.Forward;
		else viewFwd = viewFwd.Normal;

		var useX = MathF.Abs( viewFwd.x ) >= MathF.Abs( viewFwd.y );
		var targetCenter = pc;

		if ( useX )
		{
			// Choose the X face that is FARTHER from the player center
			var dMin = MathF.Abs( pc.x - lb.Mins.x );
			float dMax = MathF.Abs( pc.x - lb.Maxs.x );

			if ( dMin >= dMax )
				targetCenter.x = (lb.Mins.x - gap) - pe.x; // snap to mins.x face
			else
				targetCenter.x = (lb.Maxs.x + gap) + pe.x; // snap to maxs.x face
		}
		else
		{
			// Choose the Y face that is FARTHER from the player center
			var dMin = MathF.Abs( pc.y - lb.Mins.y );
			var dMax = MathF.Abs( pc.y - lb.Maxs.y );

			if ( dMin >= dMax )
				targetCenter.y = (lb.Mins.y - gap) - pe.y; // snap to mins.y face
			else
				targetCenter.y = (lb.Maxs.y + gap) + pe.y; // snap to maxs.y face
		}

		// Delta between current and desired bounds center
		var delta = targetCenter - pc;
		CharacterController.MoveTo( WorldPosition + delta, useStep: false );
		CharacterController.Velocity = Vector3.Zero;
	}

	bool IsFacingLadder()
	{
		var viewFwd = Camera.WorldRotation.Forward.WithZ( 0 ).Normal;
		var ladderPos = activeLadder.WorldPosition;
		var toLadder = (ladderPos - WorldPosition).WithZ( 0 );

		if ( toLadder.IsNearZeroLength )
			toLadder = viewFwd;
		else
			toLadder = toLadder.Normal;

		var lookTowardThreshold = 0.25f; // 75 degrees
		var isFacingLadder = viewFwd.Dot( toLadder ) > lookTowardThreshold;

		return isFacingLadder;
	}

	bool IsAtTopOfLadder()
	{
		return (activeLadderTopZ - WorldPosition.z) < 20;
	}

	bool IsAtBottomOfLadder()
	{
		return (WorldPosition.z - activeLadderBottomZ) < 20;
	}
}
