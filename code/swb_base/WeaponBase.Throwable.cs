using Sandbox;
using System;
using System.Threading.Tasks;

/* 
 * Weapon base for weapons firing entities in an arc
*/

namespace SWB_Base
{
    // Simulate grenade
    public class ThrowableGrenade : FiredEntity
    {
        public float ExplosionDelay { get; set; }
        public float ExplosionRadius { get; set; }
        public float ExplosionDamage { get; set; }
        public float ExplosionForce { get; set; }
        public string BounceSound { get; set; }
        public string ExplosionSound { get; set; }
        public string ExplosionEffect { get; set; }

        public override void Start()
        {
            base.Start();
            _ = DelayedExplode();
        }

        protected override void OnPhysicsCollision( CollisionEventData eventData )
        {
            if ( eventData.Entity is not Player && eventData.Speed > 50 && !string.IsNullOrEmpty( BounceSound ) )
                PlaySound( BounceSound );
        }

        async Task DelayedExplode()
        {
            await GameTask.DelaySeconds( ExplosionDelay );
            Explode();
        }

        public virtual void Explode()
        {
            // Sound
            if ( !string.IsNullOrEmpty( ExplosionSound ) )
                PlaySound( ExplosionSound );

            // Effects
            Particles.Create( ExplosionEffect, PhysicsBody.MassCenter );

            // Apply damage and force
            var explosionOrigin = PhysicsBody.MassCenter;
            var objects = Physics.GetEntitiesInSphere( explosionOrigin, ExplosionRadius );

            foreach ( var obj in objects )
            {
                if ( obj is not ModelEntity ent || !ent.IsValid() )
                    continue;

                if ( ent.LifeState != LifeState.Alive )
                    continue;

                if ( !ent.PhysicsBody.IsValid() )
                    continue;

                if ( ent.IsWorld )
                    continue;

                var targetPos = ent.PhysicsBody.MassCenter;

                var dist = Vector3.DistanceBetween( explosionOrigin, targetPos );
                if ( dist > ExplosionRadius )
                    continue;

				// Check if there is a direct line ( temp solution )
				var trace = Trace.Ray( explosionOrigin, targetPos )
					.Ignore( this )
					.Run();

				if ( trace.Entity != null && trace.Entity.IsWorld ) continue;

				var distanceMul = 1.0f - Math.Clamp( dist / ExplosionRadius, 0.0f, 1.0f );
                var damage = ExplosionDamage * distanceMul;
                var force = (ExplosionForce * distanceMul) * PhysicsBody.Mass;
                var forceDir = (ent.PhysicsBody.MassCenter - explosionOrigin).Normal;

                ent.TakeDamage( DamageInfo.Explosion( explosionOrigin, forceDir * force, damage )
                        .WithAttacker( Owner )
                        .WithWeapon( Weapon ) );

                // TODO: If ent is player shake screen
            }

            // Remove entity
            Delete();
        }
    }
}
