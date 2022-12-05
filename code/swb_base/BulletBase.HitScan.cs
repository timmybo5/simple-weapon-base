using System;
using Sandbox;

/* 
 * Base for hitscan bullets, instantly hitting targets without physics calculations
*/

namespace SWB_Base;

public class HitScanBullet : BulletBase
{
    public override void FireSV(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, bool isPrimary)
    {
        Fire(weapon, startPos, endPos, forward, spread, force, damage, bulletSize, isPrimary);
    }

    public override void FireCL(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, bool isPrimary)
    {
        Fire(weapon, startPos, endPos, forward, spread, force, damage, bulletSize, isPrimary);
    }

    private void Fire(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, bool isPrimary, int refireCount = 0)
    {
        var tr = weapon.TraceBullet(startPos, endPos, bulletSize);
        var isValidEnt = tr.Entity.IsValid();
        var canPenetrate = SurfaceUtil.CanPenetrate(tr.Surface);

        if (!isValidEnt && !canPenetrate) return;

        if (Host.IsClient)
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

        if (Host.IsServer && isValidEnt)
        {
            using (Prediction.Off())
            {
                // Damage
                var damageInfo = DamageInfo.FromBullet(tr.EndPosition, forward * 25 * force, damage)
                    .UsingTraceResult(tr)
                    .WithAttacker(weapon.Owner)
                    .WithWeapon(weapon);

                tr.Entity.TakeDamage(damageInfo);
            }
        }

        // Re-run the trace if we can penetrate
        if (canPenetrate)
        {
            if (refireCount > 100) return;
            refireCount++;

            Fire(weapon, tr.HitPosition + tr.Direction * 10, endPos, forward, spread, force, damage, bulletSize, isPrimary, refireCount);
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
