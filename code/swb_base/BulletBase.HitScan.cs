using System;
using Sandbox;

/* 
 * Base for hitscan bullets, instantly hitting targets without physics calculations
*/

namespace SWB_Base
{
    public class HitScanBullet : BulletBase
    {
        public override void FireSV(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, bool isPrimary)
        {
            foreach (var tr in weapon.TraceBullet(startPos, endPos, bulletSize))
            {
                if (!tr.Entity.IsValid()) continue;

                // We turn prediction off for this, so any exploding effects don't get culled etc
                using (Prediction.Off())
                {
                    var damageInfo = DamageInfo.FromBullet(tr.EndPosition, forward * 25 * force, damage)
                        .UsingTraceResult(tr)
                        .WithAttacker(weapon.Owner)
                        .WithWeapon(weapon);

                    tr.Entity.TakeDamage(damageInfo);
                }
            }
        }

        public override void FireCL(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, bool isPrimary)
        {
            foreach (var tr in weapon.TraceBullet(startPos, endPos, bulletSize))
            {
                // Impact
                tr.Surface.DoBulletImpact(tr);

                var tracerParticle = isPrimary ? weapon.Primary.BulletTracerParticle : weapon.Secondary.BulletTracerParticle;

                // Tracer
                if (!string.IsNullOrEmpty(tracerParticle))
                {
                    var random = new Random();
                    var randVal = random.Next(0, 2);

                    if (randVal == 0)
                        TracerEffects(weapon, tracerParticle, tr.EndPosition);
                }
            }
        }

        private void TracerEffects(WeaponBase weapon, string tracerParticle, Vector3 endPos)
        {
            ModelEntity firingViewModel = weapon.GetEffectModel();

            if (firingViewModel == null) return;

            var effectData = weapon.GetMuzzleEffectData(firingViewModel);
            var effectEntity = effectData.Item1;
            var muzzleAttach = effectEntity.GetAttachment(effectData.Item2);
            var tracer = Particles.Create(tracerParticle);
            tracer.SetPosition(1, muzzleAttach.GetValueOrDefault().Position);
            tracer.SetPosition(2, endPos);
        }
    }
}
