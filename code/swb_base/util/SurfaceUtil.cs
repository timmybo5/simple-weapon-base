using System.Collections.Generic;
using System.Linq;
using Sandbox;

/* 
 * Util class for checking surface properties
*/

namespace SWB_Base;

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

    public static bool CanPenetrate(Surface surface)
    {
        return PenetratableSurfaces.Contains(surface.ResourceName);
    }

    public static bool CanRicochet(Surface surface)
    {
        return RicochetSurfaces.Contains(surface.ResourceName);
    }

    public static bool IsPointWater(Vector3 pos)
    {
        return Trace.TestPoint(pos, "water");
    }

    public static bool IsSkybox(string[] tags)
    {
        return tags.Contains("world") && !tags.Contains("solid");
    }
}
