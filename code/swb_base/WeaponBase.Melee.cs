using Sandbox;

/* 
 * Weapon base for melee weapons
*/

namespace SWB_Base;

public partial class WeaponBaseMelee : WeaponBase
{
    /// <summary>Animation to play for the primary attack</summary>
    public virtual string PrimaryHitAnimation => "";

    /// <summary>Animation to play when missing the primary attack</summary>
    public virtual string PrimaryMissAnimation => "";

    /// <summary>Animation to play for the secondary attack</summary>
    public virtual string SecondaryHitAnimation => "";

    /// <summary>Animation to play when missing the secondary attack</summary>
    public virtual string SecondaryMissAnimation => "";

    /// <summary>Sound to play for the primary attack</summary>
    public virtual string PrimarySound => "";

    /// <summary>Sound to play for the secondary attack</summary>
    public virtual string SecondarySound => "";

    /// <summary>Sound to play when missing an attack</summary>
    public virtual string MissSound => "";

    /// <summary>Sound to play when hitting the world</summary>
    public virtual string HitWorldSound => "";

    /// <summary>Primary attack speed (lower is faster)</summary>
    public virtual float PrimarySpeed => 1f;

    /// <summary>Secondary attack speed (lower is faster)</summary>
    public virtual float SecondarySpeed => 1f;

    /// <summary>Primary attack damage</summary>
    public virtual float PrimaryDamage => 50;

    /// <summary>Secondary attack damage</summary>
    public virtual float SecondaryDamage => 100;

    /// <summary>Primary attack force</summary>
    public virtual float PrimaryForce => 25f;

    /// <summary>Secondary attack force</summary>
    public virtual float SecondaryForce => 50f;

    /// <summary>Attack range</summary>
    public virtual float DamageDistance => 25f;

    /// <summary>Attack impact size (radius)</summary>
    public virtual float DamageSize => 10f;

    public override void Reload() { }

    [ClientRpc]
    public virtual void DoMeleeEffects(string animation, string sound)
    {
        ViewModelEntity?.SetAnimParameter(animation, true);
        PlaySound(sound);
    }

    public virtual void MeleeAttack(float damage, float force, string hitAnimation, string missAnimation, string sound)
    {
        TimeSincePrimaryAttack = 0;
        TimeSinceSecondaryAttack = 0;

        var player = Owner as ISWBPlayer;
        var hitEntity = true;
        var pos = player.EyePosition;
        var forward = player.EyeRotation.Forward;
        var trace = Trace.Ray(pos, pos + forward * DamageDistance)
            .Ignore(this)
            .Ignore(Owner)
            .Size(DamageSize)
            .Run();

        if (!trace.Entity.IsValid() || trace.Entity.IsWorld)
        {
            hitAnimation = missAnimation;
            sound = !trace.Entity.IsValid() ? MissSound : HitWorldSound;
            hitEntity = false;
        }

        DoMeleeEffects(hitAnimation, sound);
        (Owner as AnimatedEntity).SetAnimParameter("b_attack", true);

        if (!hitEntity || !Game.IsServer) return;

        using (Prediction.Off())
        {
            var damageInfo = DamageInfo.FromBullet(trace.EndPosition, forward * force, damage)
                .UsingTraceResult(trace)
                .WithAttacker(Owner)
                .WithWeapon(this);

            trace.Entity.TakeDamage(damageInfo);
        }
    }

    private bool CanMelee(TimeSince lastAttackTime, float attackSpeed, string inputButton)
    {
        if (IsAnimating) return false;
        if (!Owner.IsValid() || !Input.Down(inputButton)) return false;

        return lastAttackTime > attackSpeed;
    }

    public override bool CanPrimaryAttack()
    {
        return CanMelee(TimeSincePrimaryAttack, PrimarySpeed, InputButtonHelper.PrimaryAttack);
    }

    public override bool CanSecondaryAttack()
    {
        return CanMelee(TimeSincePrimaryAttack, SecondarySpeed, InputButtonHelper.SecondaryAttack);
    }

    public override void AttackPrimary()
    {
        MeleeAttack(PrimaryDamage, PrimaryForce, PrimaryHitAnimation, PrimaryMissAnimation, PrimarySound);
    }

    public override void AttackSecondary()
    {
        MeleeAttack(SecondaryDamage, SecondaryForce, SecondaryHitAnimation, SecondaryMissAnimation, SecondarySound);
    }
}
