using Sandbox;
using System;

namespace SWB_Base
{
    public class BlastUtil
    {
        public static void Explode(Vector3 origin, float radius, float damage, float force, Entity attacker = null, Entity weapon = null, ModelEntity explodingEnt = null)
        {
            var objects = Entity.FindInSphere(origin, radius);

            foreach (var obj in objects)
            {
                // Entity check
                if (obj is not ModelEntity ent || !ent.IsValid())
                    continue;

                if (ent.LifeState != LifeState.Alive)
                    continue;

                if (!ent.PhysicsBody.IsValid())
                    continue;

                if (ent.IsWorld)
                    continue;

                // Dist check
                var targetPos = ent.PhysicsBody.MassCenter;
                var dist = Vector3.DistanceBetween(origin, targetPos);
                if (dist > radius)
                    continue;

                // Temp solution
                var tr = Trace.Ray(origin, targetPos)
                    .Ignore(explodingEnt)
                    .WorldOnly()
                    .Run();

                if (tr.Fraction < 1.0f) continue;

                var distanceMul = 1.0f - Math.Clamp(dist / radius, 0.0f, 1.0f);
                var realDamage = damage * distanceMul;
                var realForce = force * distanceMul;
                var forceDir = (targetPos - origin).Normal;

                ent.TakeDamage(DamageInfo.Explosion(origin, forceDir * realForce, realDamage)
                        .WithAttacker(attacker)
                        .WithWeapon(weapon));
            }
        }
    }
}
