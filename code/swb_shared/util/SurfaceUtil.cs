using System.Collections.Generic;

/* 
 * Utility class for checking surface properties
*/

namespace SWB.Shared;

public static class SurfaceUtil
{
	public static List<string> PenetratableSurfaces = new()
	{
		"water",
		"glass",
		"glass.pane"
	};

	public static List<string> RicochetSurfaces = new()
	{
		"default",
		"metal",
		"metal.sheet",
		"ceramic",
		"plastic",
		"plastic.sheet",
		"wood",
		"wood.sheet",
	};

	public static bool CanPenetrate( Surface surface )
	{
		return PenetratableSurfaces.Contains( surface.ResourceName );
	}

	public static bool CanRicochet( Surface surface )
	{
		return RicochetSurfaces.Contains( surface.ResourceName );
	}

	/// <summary>Angle in degrees between the travel direction and the surface plane. 0 = perfectly parallel/grazing, 90 = perpendicular/direct hit.</summary>
	public static float GetGrazingAngle( Vector3 direction, Vector3 normal )
	{
		return 90f - Vector3.GetAngle( -direction, normal );
	}

	public static bool IsPointWater( Vector3 pos )
	{
		var tr = Game.SceneTrace.Ray( pos, pos + Vector3.Forward )
			.WithTag( TagsHelper.Water )
			.Run();

		return tr.Hit;
	}

	public static bool IsSkybox( Surface surface )
	{
		return surface.HasTag( TagsHelper.Sky ) || (surface.HasTag( TagsHelper.World ) && !surface.HasTag( TagsHelper.Solid ));
	}
}
