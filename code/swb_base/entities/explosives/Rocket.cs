using System.Collections.Generic;
using Sandbox;

/* 
 * High explosive grenade
*/

namespace SWB_Base
{
    public class Rocket : FiredEntity
    {
        public float ExplosionDelay { get; set; }
        public float ExplosionRadius { get; set; }
        public float ExplosionDamage { get; set; }
        public float ExplosionForce { get; set; }
        public List<string> ExplosionSounds { get; set; }
        public string ExplosionEffect { get; set; }
        public ScreenShake ExplosionShake { get; set; }
        public string RocketSound { get; set; }
        public List<string> RocketEffects { get; set; } = new List<string>();
        public string RocketSmokeEffect { get; set; }

        private List<Particles> rocketParticles = new List<Particles>();
        private Sound rocketLoopSound;

        public override void Start()
        {
            base.Start();

            if (!string.IsNullOrEmpty(RocketSound))
                rocketLoopSound = PlaySound(RocketSound);

            for (int i = 0; i < RocketEffects.Count; i++)
            {
                rocketParticles.Add(Particles.Create(RocketEffects[i], this, null, true));
            }
        }

        protected override void OnPhysicsCollision(CollisionEventData eventData)
        {
            if (eventData.Entity == Owner) return;

            Explode();
        }

        public override void Tick()
        {
            // Rocket flight
            var upForce = Rotation.Up * 12;
            PhysicsBody.Velocity += upForce;

            // Update sound
            rocketLoopSound.SetPosition(Position);
        }

        public virtual void Explode()
        {
            // Stop loop sound
            rocketLoopSound.Stop();

            // Stop rocket particles
            for (int i = 0; i < rocketParticles.Count; i++)
            {
                rocketParticles[i].Destroy(false);
            }

            // Explosion sound
            var explosionSound = TableUtil.GetRandom(ExplosionSounds);

            if (!string.IsNullOrEmpty(explosionSound))
                PlaySound(explosionSound);

            // Explosion effect
            if (!string.IsNullOrEmpty(ExplosionEffect))
                Particles.Create(ExplosionEffect, PhysicsBody.MassCenter);

            // Screenshake
            ScreenUtil.ShakeAt(PhysicsBody.MassCenter, ExplosionRadius * 2, ExplosionShake);

            // Damage
            BlastUtil.Explode(PhysicsBody.MassCenter, ExplosionRadius, ExplosionDamage, ExplosionForce, Owner, Weapon, this);

            // Remove entity
            RenderColor = Color.Transparent;
            DeleteAsync(0.01f);
        }
    }
}
