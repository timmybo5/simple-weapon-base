using System;
using Sandbox;

/* 
 * Base for hitscan bullets, instantly hitting targets without physics calculations
*/

namespace SWB_Base;

public class HitScanBullet : BulletBase
{
    public override void FireSV(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, float bulletTracerChance, bool isPrimary)
    {
        Fire(weapon, startPos, endPos, forward, spread, force, damage, bulletSize, bulletTracerChance, isPrimary);
    }

    public override void FireCL(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, float bulletTracerChance, bool isPrimary)
    {
        Fire(weapon, startPos, endPos, forward, spread, force, damage, bulletSize, bulletTracerChance, isPrimary);
    }

    private void Fire(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, float bulletTracerChance, bool isPrimary, int refireCount = 0)
    {
        var tr = weapon.TraceBullet(startPos, endPos, bulletSize);
        var isValidEnt = tr.Entity.IsValid();
        var canPenetrate = SurfaceUtil.CanPenetrate(tr.Surface);

        if (!isValidEnt && !canPenetrate) return;

        if (Game.IsClient)
        {
            // Impact
            if (!SurfaceUtil.IsSkybox(tr.Tags))
                tr.Surface.DoBulletImpact(tr);

            var tracerParticle = isPrimary ? weapon.Primary.BulletTracerParticle : weapon.Secondary.BulletTracerParticle;

            // Tracer
            if (!string.IsNullOrEmpty(tracerParticle?.Path))
            {
                var random = new Random();
                var randVal = random.NextDouble();

                if (randVal < bulletTracerChance)
                    TracerEffects(weapon, tracerParticle, tr.EndPosition);
            }
        }

        if (Game.IsServer && isValidEnt)
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

            Fire(weapon, tr.HitPosition + tr.Direction * 10, endPos, forward, spread, force, damage, bulletSize, bulletTracerChance, isPrimary, refireCount);
        }
    }

    private void TracerEffects(WeaponBase weapon, ParticleData tracerParticle, Vector3 endPos)
    {
        ModelEntity firingViewModel = weapon.GetEffectModel();

        if (firingViewModel == null) return;

        var isViewModel = weapon.IsLocalPawn && weapon.IsFirstPersonMode;
        var scale = isViewModel ? tracerParticle.VMScale : tracerParticle.WMScale;
        var effectData = weapon.GetMuzzleEffectData(firingViewModel);
        var effectEntity = effectData.Item1;
        var muzzleAttach = effectEntity.GetAttachment(effectData.Item2);
        var tracer = Particles.Create(tracerParticle.Path);
        tracer.Set("scale", scale);
        tracer.SetPosition(1, muzzleAttach.GetValueOrDefault().Position);
        tracer.SetPosition(2, endPos);
    }
}
