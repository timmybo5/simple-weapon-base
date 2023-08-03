using Sandbox;

/* 
 * Base for bullets
*/

namespace SWB_Base;

public abstract class BulletBase : BaseNetworkable
{
    /// <summary>
    /// Shoot the bullet (called on server and all clients)
    /// </summary>
    public abstract void Fire(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, float bulletTracerChance, bool isPrimary);

}
