using System.Threading.Tasks;
using Sandbox;

/* 
 * High explosive grenade
*/

namespace SWB_Base;

public class Grenade : FiredEntity
{
    public float ExplosionDelay { get; set; }
    public float ExplosionRadius { get; set; }
    public float ExplosionDamage { get; set; }
    public float ExplosionForce { get; set; }
    public string BounceSound { get; set; }
    public string ExplosionSound { get; set; }
    public string ExplosionEffect { get; set; }
    public ScreenShake ExplosionShake { get; set; }

    public override void Start()
    {
        base.Start();
        _ = DelayedExplode();
    }

    protected override void OnPhysicsCollision(CollisionEventData eventData)
    {
        if (eventData.Other.Entity is not PlayerBase && eventData.Speed > 50 && !string.IsNullOrEmpty(BounceSound))
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
        if (!string.IsNullOrEmpty(ExplosionSound))
            PlaySound(ExplosionSound).SetPosition(Position);

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
