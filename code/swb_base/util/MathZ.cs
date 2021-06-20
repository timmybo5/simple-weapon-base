using Sandbox;

/* 
 * Utility class to handle framerate independent animations
*/

namespace SWB_Base
{
    class MathZ
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
    }
}
