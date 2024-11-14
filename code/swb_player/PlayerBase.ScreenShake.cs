using SWB.Shared;
using System;

namespace SWB.Player;

public partial class PlayerBase
{
	ScreenShake lastScreenShake;
	RealTimeSince timeSinceShake;
	float nextShake;

	public virtual void ShakeScreen( ScreenShake screenShake )
	{
		lastScreenShake = screenShake;
		timeSinceShake = 0;
		nextShake = 0;
	}

	public virtual void HandleScreenShake()
	{
		if ( timeSinceShake < lastScreenShake?.Duration && timeSinceShake > nextShake )
		{
			var random = new Random();
			var randomPos = new Vector3( random.Float( 0, lastScreenShake.Size ), random.Float( 0, lastScreenShake.Size ), random.Float( 0, lastScreenShake.Size ) );
			var randomRot = new Angles( random.Float( 0, lastScreenShake.Rotation ), random.Float( 0, lastScreenShake.Rotation ), 0 );

			CameraMovement.AnglesOffset += randomRot;
			CameraMovement.PosOffset += randomPos;
			nextShake = timeSinceShake + lastScreenShake.Delay;
		}
	}
}
