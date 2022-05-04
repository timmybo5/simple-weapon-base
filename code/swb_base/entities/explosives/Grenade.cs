using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

/* 
 * High explosive grenade
*/

namespace SWB_Base
{
    public class Grenade : FiredEntity
    {
        public float ExplosionDelay { get; set; }
        public float ExplosionRadius { get; set; }
        public float ExplosionDamage { get; set; }
        public float ExplosionForce { get; set; }
        public string BounceSound { get; set; }
        public List<string> ExplosionSounds { get; set; }
        public string ExplosionEffect { get; set; }
        public ScreenShake ExplosionShake { get; set; }

        public override void Start()
        {
            base.Start();
            _ = DelayedExplode();
        }

        protected override void OnPhysicsCollision(CollisionEventData eventData)
        {
            if (eventData.Entity is not Player && eventData.Speed > 50 && !string.IsNullOrEmpty(BounceSound))
                PlaySound(BounceSound);
        }

        async Task DelayedExplode()
        {
            await GameTask.DelaySeconds(ExplosionDelay);
            Explode();
        }

        public virtual void Explode()
        {
            // Explosion sound
            var explosionSound = TableUtil.GetRandom(ExplosionSounds);

            if (!string.IsNullOrEmpty(explosionSound))
                PlaySound(explosionSound).SetPosition(Position);

            // Effects
            Particles.Create(ExplosionEffect, PhysicsBody.MassCenter);

            // Screenshake
            ScreenUtil.ShakeAt(PhysicsBody.MassCenter, ExplosionRadius * 1.5f, ExplosionShake);

            // Damage
            BlastUtil.Explode(PhysicsBody.MassCenter, ExplosionRadius, ExplosionDamage, ExplosionForce, Owner, Weapon, this);

            // Remove entity
            Delete();
        }
    }
}
