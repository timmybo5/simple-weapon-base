using System;
using Sandbox;

namespace SWB_Base
{
    public abstract class PhysicalBulletBase : BulletBase
    {
        public override void FireSV(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize)
        {
            LogUtil.Info("boom");
        }
        public override void FireCL(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize)
        {
            LogUtil.Info("boom");
        }
    }

    public class SMGBullet : PhysicalBulletBase
    {

    }
}
