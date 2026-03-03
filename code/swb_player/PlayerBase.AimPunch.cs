namespace SWB.Player;

public partial class PlayerBase
{
	Angles aimPunchAngle;
	float aimPunchRecoverySpeed = 5f;
	RealTimeSince timeSinceLastPunch;

	/// <summary>
	/// Apply aimpunch recoil to the camera.
	/// Similar to Flinch but with configurable recovery and shooting-aware delay.
	/// </summary>
	/// <param name="punchAmount">The angular offset to apply</param>
	/// <param name="recoverySpeed">How fast the camera recovers (higher = faster)</param>
	public virtual void ApplyAimPunch( Angles punchAmount, float recoverySpeed = 5f )
	{
		aimPunchAngle += punchAmount;
		aimPunchRecoverySpeed = recoverySpeed;
		timeSinceLastPunch = 0;
		
		ApplyEyeAnglesOffset( punchAmount );
	}

	/// <summary>
	/// Reset aimpunch angle (called on weapon switch, death, etc.)
	/// </summary>
	public virtual void ResetAimPunch()
	{
		aimPunchAngle = Angles.Zero;
	}

	/// <summary>
	/// Handle aimpunch recovery over time.
	/// Recovery only starts when player stops shooting.
	/// </summary>
	public virtual void HandleAimPunch()
	{
		if ( !IsAlive )
		{
			aimPunchAngle = Angles.Zero;
			return;
		}

		bool isShooting = Inventory?.ActiveItem?.IsShooting() ?? false;

		if ( isShooting || timeSinceLastPunch < 0.1f )
			return;

		if ( aimPunchAngle.pitch != 0 || aimPunchAngle.yaw != 0 || aimPunchAngle.roll != 0 )
		{
			var decayAmount = aimPunchRecoverySpeed * Time.Delta;
			
			var oldPitch = aimPunchAngle.pitch;
			var oldYaw = aimPunchAngle.yaw;
			var oldRoll = aimPunchAngle.roll;
			
			aimPunchAngle = new Angles(
				aimPunchAngle.pitch.Approach( 0, decayAmount ),
				aimPunchAngle.yaw.Approach( 0, decayAmount ),
				aimPunchAngle.roll.Approach( 0, decayAmount )
			);
			
			var deltaAngle = new Angles(
				aimPunchAngle.pitch - oldPitch,
				aimPunchAngle.yaw - oldYaw,
				aimPunchAngle.roll - oldRoll
			);
			
			ApplyEyeAnglesOffset( deltaAngle );
		}
	}
}
