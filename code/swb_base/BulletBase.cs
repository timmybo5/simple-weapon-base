using System;
using Sandbox;

/* 
 * Base for bullets
*/

namespace SWB_Base
{
    public abstract class BulletBase : BaseNetworkable
    {
        /// <summary>
        /// Shoot the bullet on server
        /// </summary>
        public abstract void FireSV(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize);

        /// <summary>
        /// Shoot the bullet on client
        /// </summary>
        public abstract void FireCL(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize);
    }
}
