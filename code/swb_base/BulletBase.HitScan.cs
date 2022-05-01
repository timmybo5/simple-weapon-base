using System;
using Sandbox;

/* 
 * Base for HitScan bullets, instantly hitting targets without physics calculations
*/

namespace SWB_Base
{
    public class HitScanBullet : BulletBase
    {
        public override void FireSV(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize)
        {
            foreach (var tr in weapon.TraceBullet(startPos, endPos, bulletSize))
            {
                if (!tr.Entity.IsValid()) continue;

                // We turn prediction off for this, so any exploding effects don't get culled etc
                using (Prediction.Off())
                {
                    var damageInfo = DamageInfo.FromBullet(tr.EndPosition, forward * 100 * force, damage)
                        .UsingTraceResult(tr)
                        .WithAttacker(weapon.Owner)
                        .WithWeapon(weapon);

                    tr.Entity.TakeDamage(damageInfo);
                }
            }
        }

        public override void FireCL(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize)
        {
            foreach (var tr in weapon.TraceBullet(startPos, endPos, bulletSize))
            {
                // Impact
                tr.Surface.DoBulletImpact(tr);

                // Tracer
                if (!string.IsNullOrEmpty(weapon.Primary.BulletTracerParticle))
                {
                    var random = new Random();
                    var randVal = random.Next(0, 2);

                    if (randVal == 0)
                        weapon.TracerEffects(weapon.Primary.BulletTracerParticle, tr.EndPosition);
                }
            }
        }
    }
}
