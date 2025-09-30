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
		"wip",
	};

	public static bool CanPenetrate( Surface surface )
	{
		return PenetratableSurfaces.Contains( surface.ResourceName );
	}

	public static bool CanRicochet( Surface surface )
	{
		return RicochetSurfaces.Contains( surface.ResourceName );
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
		return surface.HasTag( TagsHelper.World ) && !surface.HasTag( TagsHelper.Solid );
	}
}
