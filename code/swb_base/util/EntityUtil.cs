using Sandbox;

namespace SWB_Base;

class EntityUtil
{
    public static Entity GetEntityByNetworkIdent(int networkIdent)
    {
        foreach (var ent in Entity.All)
        {
            if (ent.NetworkIdent == networkIdent)
            {
                return ent;
            }
        }
        return null;
    }
}
