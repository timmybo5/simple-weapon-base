using SWB.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWB.Base;

public class ProjectileBasedBullet : IBulletBase
{
    
    public async void Shoot(Weapon weapon, ShootInfo shootInfo, Vector3 spreadOffset)
{
    if (!weapon.IsValid) return;

    var player = weapon.Owner;
    if (player is null) return;

    
    var forward = player.EyeAngles.Forward + spreadOffset;
    //disable aimcone
    //var forward = player.EyeAngles.Forward;
    forward = forward.Normal;

    var startPos = player.EyePos;
    var velocity = forward * shootInfo.BulletSpeed;
    //var gravity = new Vector3(0, 0, -0.6f); // Simulated gravity
    var gravity = new Vector3(0, 0, -50f);
    // Simulate the bullet over time
    float timeStep = 0.016f; // Use smaller time steps for smoother movement (equivalent to ~60 FPS)
    var currentPos = startPos;
    var distanceLeft = shootInfo.BulletRange;

    // Create the projectile object once
    GameObject projectileObject = SceneUtility.GetPrefabScene(ResourceLibrary.Get<PrefabFile>("bullet.prefab"));
    //Log.Info(projectileObject.Name);

    var projectile = projectileObject.Clone();
    projectile.WorldPosition = currentPos;

    // Set the projectile's rotation to match the player's facing direction
    projectile.WorldRotation = Rotation.LookAt(forward);

    // Iterate through the bullet's path over time
    while (distanceLeft > 0)
    {
        var nextPos = currentPos + velocity * timeStep;
        var bulletTr = weapon.TraceBullet(currentPos, nextPos);

        if (bulletTr.Hit)
        {
            var hitObj = bulletTr.GameObject;

            // Impact
            weapon.CreateBulletImpact(bulletTr);

            // Apply damage if we hit a valid target (like a player)
            if (!weapon.IsProxy && hitObj is not null && hitObj.Tags.Has(TagsHelper.Player))
            {
                var target = hitObj.Components.GetInAncestorsOrSelf<IPlayerBase>();
                if (target?.IsAlive == true)
                {
                    var hitTags = bulletTr.Hitbox?.Tags.TryGetAll().ToArray() ?? Array.Empty<string>();

                    var dmgInfo = Shared.DamageInfo.FromBullet(
                        weapon.Owner.Id,
                        weapon.ClassName,
                        shootInfo.Damage,
                        bulletTr.HitPosition,
                        velocity * shootInfo.Force, // Force based on velocity
                        hitTags
                    );

                    target?.TakeDamage(dmgInfo);
                }
            }
            // Apply damage to bots that implement IDamageable
            else if (hitObj is not null && hitObj.Tags.Has(TagsHelper.Bot))
            {
                var botTarget = hitObj.Components.Get<IDamageable>();

                if (botTarget != null && botTarget.IsAlive)
                {
                    var hitTags = bulletTr.Hitbox?.Tags.TryGetAll().ToArray() ?? Array.Empty<string>();

                    var dmgInfo = Shared.DamageInfo.FromBullet(
                        weapon.Owner.Id,
                        weapon.ClassName,
                        shootInfo.Damage,
                        bulletTr.HitPosition,
                        velocity * shootInfo.Force,
                        hitTags
                    );

                    botTarget.TakeDamage(dmgInfo);  // Apply damage to the bot
                }
            }


            projectile.Destroy(); // Destroy the projectile upon impact
            return; // Stop once we hit something
        }

        // Update position and apply gravity
        currentPos = nextPos;
        velocity += gravity * timeStep;
        distanceLeft -= velocity.Length * timeStep;

        // Move the projectile smoothly
        projectile.WorldPosition = currentPos;

        // Wait for the next frame (to simulate real-time movement)
        await Task.Delay((int)(timeStep * 1000)); // Convert timeStep (in seconds) to milliseconds
    }

    // Destroy the projectile after it has traveled the full distance
    projectile.Destroy();
}


    public Vector3 GetRandomSpread(float spread)
    {
        return (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
    }

    void TracerEffects(Weapon weapon, ShootInfo shootInfo, Vector3 currentPos, Vector3 endPos)
    {
       
        
    }
    
    
}