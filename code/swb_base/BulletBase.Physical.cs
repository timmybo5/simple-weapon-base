using System;
using System.Globalization;
using Sandbox;

/* 
 * Base for physical bullets, bullets with physics characteristics e.g. bullet drop
*/

namespace SWB_Base;

public abstract class PhysicalBulletBase : BulletBase
{
    /// <summary>Mass of the bullet in grams</summary>
    public virtual float Mass => 1;

    /// <summary>Amount of drag force</summary>
    public virtual float Drag => 0;

    /// <summary>Initial bullet velocity (m/s)</summary>
    public virtual float Velocity => 1000;

    public override void FireSV(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, float bulletTracerChance, bool isPrimary)
    {
        // Server bullet for damage (from eyePos)
        var bullet = new BulletEntity();
        bullet?.Fire(startPos, forward, weapon, this, damage, force, bulletSize, bulletTracerChance);
    }

    public override void FireCL(WeaponBase weapon, Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, float bulletTracerChance, bool isPrimary)
    {
        // Client bullet for effects (from muzzle)
        ModelEntity firingViewModel = weapon.GetEffectModel();

        if (firingViewModel == null) return;

        var tracerParticle = isPrimary ? weapon.Primary.BulletTracerParticle : weapon.Secondary.BulletTracerParticle;
        var effectData = weapon.GetMuzzleEffectData(firingViewModel);
        var effectEntity = effectData.Item1;
        var muzzleAttach = effectEntity.GetAttachment(effectData.Item2);
        var muzzlePos = muzzleAttach.GetValueOrDefault().Position;
        var posDiff = startPos - muzzlePos;

        var bullet = new BulletEntity();
        bullet?.Fire(muzzlePos, forward, weapon, this, damage, force, bulletSize, bulletTracerChance, tracerParticle, posDiff);
    }
}

public class BulletEntity : Entity
{
    public static string BulletTag = "swb_bullet";
    public static float InchesPerMeter = 39.3701f;

    /// <summary>Airdensity in kilograms per inch cubed</summary>
    public static float AirDensity = 2.007e-5f;

    /// <summary>This is the gravity we want this physics body to have in meters per second</summary>
    private static float TargetGravity = 9.8f * InchesPerMeter;

    private float speedMultiplier = 1f;

    private float damage = 0f;
    private float gravScale = 1f;
    private float maxLifeTime = 5f;
    private float bulletSize = 0f;
    private float bulletMass = 0f;
    private float force = 0f;

    private WeaponBase weapon;
    private PhysicalBulletBase ammoType;
    private TimeSince timeSinceFire;
    private Particles bulletTracer;
    private ParticleData tracerParticle;
    private float particleDelay;
    private bool createParticle;

    private Vector3 direction;
    private Vector3 startPos;
    private Vector3 lastPosition;

    private Vector3 posDiff = Vector3.Zero;
    private int posDiffMaxUpdate = 0;
    private int posDiffCurUpdate = 0;

    public override void Spawn()
    {
        // Gravity calc
        var currentGravity = Game.PhysicsWorld.Gravity;
        var gravityRatio = TargetGravity / currentGravity.Length;
        gravScale = gravityRatio;

        // Hide for all clients
        Transmit = TransmitType.Never;
    }

    public void Fire(Vector3 position, Vector3 direction, WeaponBase weapon, PhysicalBulletBase ammoType,
        float damage, float force, float bulletSize, float bulletTracerChance, ParticleData tracerParticle = null, Vector3 posDiff = new Vector3())
    {
        Position = position;
        Owner = weapon.Owner;
        var realBulletVel = ammoType.Velocity * InchesPerMeter;
        Velocity = direction * realBulletVel * weapon.BulletVelocityMod * speedMultiplier + Owner.Velocity * 0.2f;

        lastPosition = position;

        this.weapon = weapon;
        this.ammoType = ammoType;
        this.damage = damage;
        this.force = force;
        this.bulletSize = bulletSize;
        this.bulletMass = ammoType.Mass / 100;
        this.tracerParticle = tracerParticle;
        this.posDiff = posDiff;
        this.direction = direction;
        timeSinceFire = 0;

        var random = new Random();
        var randVal = random.NextDouble();
        createParticle = randVal < bulletTracerChance;

        if (createParticle)
        {
            var randDelay = random.Float(0.15f);
            particleDelay = randDelay;
        }

        if (Game.IsClient)
        {
            CompensateForCrosshair(direction);
        }

        if (WeaponBase.DebugBulletsSV > 0)
        {
            startPos = Position;
        }

        if (Owner is ISWBPlayer player)
        {
            var simulator = player.BulletSimulator;
            simulator?.Add(this);
        }

        DeleteAsync(maxLifeTime);
    }

    // Shooting a bullet from muzzle pos will make it less 'accurate'
    private void CompensateForCrosshair(Vector3 direction)
    {
        // Check initial distance
        var tr = Trace.Ray(Position, Position + direction * 9999)
            .UseHitboxes()
            .Ignore(Owner)
            .WithoutTags(BulletTag)
            .WorldAndEntities()
            .Size(bulletSize)
            .Run();

        if (tr.Distance > 120)
        {
            posDiffMaxUpdate = (int)Math.Clamp(Math.Floor(tr.Distance / (Velocity.Length * 0.02)), 1, 10);
        }
        else
        {
            // Be super accurate close to wall
            Position += posDiff;
            lastPosition = Position;
        }
    }

    private void CreateTracer()
    {
        if (!string.IsNullOrEmpty(tracerParticle?.Path))
        {
            var isViewModel = weapon.IsLocalPawn && weapon.IsFirstPersonMode;
            var scale = isViewModel ? tracerParticle.VMScale : tracerParticle.WMScale;
            bulletTracer = Particles.Create(tracerParticle.Path, this);
            bulletTracer.Set("scale", scale);
            UpdateTracer();
        }
    }

    private void UpdateTracer()
    {
        bulletTracer?.SetPosition(1, Velocity);
    }

    public void BulletPhysics()
    {
        lastPosition = Position;

        // Forward
        var dt = Time.Delta;
        Velocity += Game.PhysicsWorld.Gravity * gravScale * dt;

        // Drag
        var drag = AirDensity * MathF.Pow(Velocity.Length, 2) * ammoType.Drag;
        drag /= 2 * bulletMass;

        Velocity -= Velocity.Normal * drag * dt;

        // Update pos
        Position += Velocity * dt;

        // Center adjustment
        if (Game.IsClient && posDiffCurUpdate < posDiffMaxUpdate)
        {
            posDiffCurUpdate++;
            Position += posDiff / posDiffMaxUpdate;
        }

        if (WeaponBase.DebugBulletsSV > 0)
            DebugOverlay.Line(lastPosition, Position, Game.IsServer ? Color.Green : Color.Yellow, 0, false);

        if (createParticle && timeSinceFire > particleDelay)
        {
            if (bulletTracer == null)
            {
                CreateTracer();
            }

            UpdateTracer();
        }

        //if (timeSinceFire > 0)
        //{
        //    if (bulletTracer == null)
        //    {
        //        var random = new Random();
        //        var randVal = random.Next(0, 2);

        //        if (randVal == 0)
        //            CreateTracer();
        //    }

        //    UpdateTracer();
        //}

        DoTraceCheck();
    }

    private void BulletHit(TraceResult tr)
    {
        if (WeaponBase.DebugBulletsSV > 0)
        {
            var hitRotation = Rotation.From(new Angles(tr.Normal.z, tr.Normal.y, 0) * 90);

            DebugOverlay.Circle(tr.EndPosition, hitRotation, bulletSize, Game.IsServer ? Color.Red : Color.Blue, maxLifeTime, false);

            if (Game.IsServer)
            {
                var distance = startPos.Distance(Position) / InchesPerMeter;
                DebugOverlay.ScreenText(distance.ToString(CultureInfo.InvariantCulture), Game.Random.Int(40), maxLifeTime);
            }
        }

        var isValidEnt = tr.Entity.IsValid();
        var canPenetrate = SurfaceUtil.CanPenetrate(tr.Surface);

        if (isValidEnt || canPenetrate)
        {
            if (Game.IsClient)
            {
                tr.Surface.DoBulletImpact(tr);
            }

            if (Game.IsServer && isValidEnt)
            {
                var damageInfo = DamageInfo.FromBullet(tr.EndPosition, direction * 25 * force, damage)
                    .UsingTraceResult(tr)
                    .WithAttacker(Owner)
                    .WithWeapon(weapon);

                tr.Entity.TakeDamage(damageInfo);
            }

            if (!canPenetrate)
            {
                Delete();
            }

            return;
        }
    }

    private void DoTraceCheck()
    {
        BulletHit(weapon.TraceBullet(lastPosition, Position, bulletSize));
    }
}
