using System;
using System.Collections.Generic;
using Sandbox;

/* 
 * High explosive grenade
*/

namespace SWB_Base;

public class Rocket : FiredEntity
{
    public float ExplosionDelay { get; set; }
    public float ExplosionRadius { get; set; }
    public float ExplosionDamage { get; set; }
    public float ExplosionForce { get; set; }
    public string ExplosionSound { get; set; }
    public string ExplosionEffect { get; set; }
    public ScreenShake ExplosionShake { get; set; }
    public string RocketSound { get; set; }
    public List<string> RocketEffects { get; set; } = new List<string>();
    public string RocketSmokeEffect { get; set; }
    public int Inaccuracy { get; set; } = 0; // How Inaccurate is the rocket (higher = less accurate)

    private TimeSince timeSince;
    private List<Particles> rocketParticles = new List<Particles>();
    private Sound rocketLoopSound;

    public override void Start()
    {
        base.Start();
        timeSince = 0;

        if (!string.IsNullOrEmpty(RocketSound))
            rocketLoopSound = PlaySound(RocketSound);

        for (int i = 0; i < RocketEffects.Count; i++)
        {
            rocketParticles.Add(Particles.Create(RocketEffects[i], this, true));
        }
    }

    protected override void OnPhysicsCollision(CollisionEventData eventData)
    {
        if (eventData.Other.Entity == Owner) return;

        Explode();
    }

    public override void Tick()
    {
        var inWater = SurfaceUtil.IsPointWater(Position);

        // Rocket flight
        if (!inWater)
        {
            var downForce = Rotation.Down * 4;
            var random = new Random();
            var timeSinceMod = (int)Math.Max(0, Inaccuracy * timeSince);
            var sideForce = Rotation.Left * (random.Next(0, timeSinceMod) * 2 - timeSinceMod);

            Velocity += (downForce + sideForce) * 20;
        }

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
        if (!string.IsNullOrEmpty(ExplosionSound))
            PlaySound(ExplosionSound).SetPosition(Position);

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
