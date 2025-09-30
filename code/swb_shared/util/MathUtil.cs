using System;

/* 
 * Utility class to handle framerate independent + useful calculations
*/

namespace SWB.Shared;

class MathUtil
{
	public static float FILerp( float fromF, float toF, float amount )
	{
		return fromF.LerpTo( toF, amount * RealTime.Delta );
	}

	public static Vector3 FILerp( Vector3 fromVec, Vector3 toVec, float amount )
	{
		return fromVec.LerpTo( toVec, amount * RealTime.Delta );
	}

	public static Angles FILerp( Angles fromAng, Angles toAng, float amount )
	{
		return Angles.Lerp( fromAng, toAng, amount * RealTime.Delta );
	}

	public static Vector3 RelativeAdd( Vector3 vec1, Vector3 vec2, Rotation rot )
	{
		vec1 += vec2.x * rot.Right;
		vec1 += vec2.y * rot.Up;
		vec1 += vec2.z * rot.Forward;

		return vec1;
	}


	// Helpful bezier function. Use this if you gotta: https://www.desmos.com/calculator/cahqdxeshd
	public static float BezierY( float f, float a, float b, float c )
	{
		f *= 3.2258f;
		return MathF.Pow( (1.0f - f), 2.0f ) * a + 2.0f * (1.0f - f) * f * b + MathF.Pow( f, 2.0f ) * c;
	}

	public static Vector3 ToVector3( Angles angles )
	{
		return new Vector3( angles.pitch, angles.yaw, angles.roll );
	}

	public static Angles ToAngles( Vector3 vector )
	{
		return new Angles( vector.x, vector.y, vector.z );
	}
}
