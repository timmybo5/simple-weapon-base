using System;

/* 
 * Utility class for cameras
*/

namespace SWB.Shared;

public static class CameraUtil
{
	public static Vector3 ProjectToViewSpace( Vector3 pos, CameraComponent vmCam, CameraComponent worldCam )
	{
		var to = pos - vmCam.WorldPosition;
		var x = to.Dot( vmCam.WorldRotation.Right );
		var y = to.Dot( vmCam.WorldRotation.Up );
		var z = to.Dot( vmCam.WorldRotation.Forward );

		if ( z <= 0.001f ) return pos;

		var scale = MathF.Tan( MathX.DegreeToRadian( worldCam.FieldOfView ) * 0.5f )
				/ MathF.Tan( MathX.DegreeToRadian( vmCam.FieldOfView ) * 0.5f );

		x *= scale;
		y *= scale;

		return worldCam.WorldPosition
			+ worldCam.WorldRotation.Right * x
			+ worldCam.WorldRotation.Up * y
			+ worldCam.WorldRotation.Forward * z;
	}
}
