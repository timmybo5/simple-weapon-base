using Sandbox;

/* 
 * Base for bullets
*/

namespace SWB_Base;

public abstract class BulletBase : BaseNetworkable
{

    /// <summary>
    /// Shoot the bullet on server
    /// </summary>
    public abstract void FireSV(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, bool isPrimary);

    /// <summary>
    /// Shoot the bullet on all clients
    /// </summary>
    public abstract void FireCL(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, bool isPrimary);
}
