using System.Collections.Generic;
using Sandbox;

/* 
 * Util class for checking surface properties
*/

namespace SWB_Base
{
    public static class SurfaceUtil
    {
        public static List<string> penetratableSurfaces = new()
        {
            "water",
            "glass.pane"
        };

        public static List<string> ricochetSurfaces = new()
        {
            "wip",
        };

        public static bool CanPenetrate(Surface surface)
        {
            return penetratableSurfaces.Contains(surface.ResourceName);
        }

        public static bool CanRicochet(Surface surface)
        {
            return ricochetSurfaces.Contains(surface.ResourceName);
        }

        public static bool IsPointWater(Vector3 pos)
        {
            var tr = Trace.Ray(pos, pos + Vector3.Forward)
                     .UseHitboxes()
                     .HitLayer(CollisionLayer.Water, true)
                     .HitLayer(CollisionLayer.Player, false)
                     .HitLayer(CollisionLayer.CARRIED_WEAPON, false)
                     .Size(1)
                     .Run();

            return tr.Surface.ResourceName == "water";
        }
    }
}
