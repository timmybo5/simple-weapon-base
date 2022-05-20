using Sandbox;

/* 
 * Weapon base for melee weapons
*/

namespace SWB_Base
{
    public partial class WeaponBaseMelee : WeaponBase
    {
        /// <summary>Animation to play for the primary attack</summary>
        public virtual string SwingAnimationHit => "";

        /// <summary>Animation to play when missing the primary attack</summary>
        public virtual string SwingAnimationMiss => "";

        /// <summary>Animation to play for the secondary attack</summary>
        public virtual string StabAnimationHit => "";

        /// <summary>Animation to play when missing the secondary attack</summary>
        public virtual string StabAnimationMiss => "";

        /// <summary>Sound to play for the primary attack</summary>
        public virtual string SwingSound => "";

        /// <summary>Sound to play for the secondary attack</summary>
        public virtual string StabSound => "";

        /// <summary>Sound to play when missing an attack</summary>
        public virtual string MissSound => "";

        /// <summary>Sound to play when hitting the world</summary>
        public virtual string HitWorldSound => "";

        /// <summary>Primary attack speed (lower is faster)</summary>
        public virtual float SwingSpeed => 1f;

        /// <summary>Secondary attack speed (lower is faster)</summary>
        public virtual float StabSpeed => 1f;

        /// <summary>Primary attack damage</summary>
        public virtual float SwingDamage => 50;

        /// <summary>Secondary attack damage</summary>
        public virtual float StabDamage => 100;

        /// <summary>Primary attack force</summary>
        public virtual float SwingForce => 25f;

        /// <summary>Secondary attack force</summary>
        public virtual float StabForce => 50f;

        /// <summary>Attack range</summary>
        public virtual float DamageDistance => 25f;

        /// <summary>Attack impact size</summary>
        public virtual float ImpactSize => 10f;

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

            var hitEntity = true;
            var pos = Owner.EyePosition;
            var forward = Owner.EyeRotation.Forward;
            var trace = Trace.Ray(pos, pos + forward * DamageDistance)
                .Ignore(this)
                .Ignore(Owner)
                .Size(ImpactSize)
                .Run();

            if (!trace.Entity.IsValid() || trace.Entity.IsWorld)
            {
                hitAnimation = missAnimation;
                sound = !trace.Entity.IsValid() ? MissSound : HitWorldSound;
                hitEntity = false;
            }

            DoMeleeEffects(hitAnimation, sound);
            (Owner as AnimatedEntity).SetAnimParameter("b_attack", true);

            if (!hitEntity || !IsServer) return;

            using (Prediction.Off())
            {
                var damageInfo = DamageInfo.FromBullet(trace.EndPosition, forward * force, damage)
                    .UsingTraceResult(trace)
                    .WithAttacker(Owner)
                    .WithWeapon(this);

                trace.Entity.TakeDamage(damageInfo);
            }
        }

        private bool CanMelee(TimeSince lastAttackTime, float attackSpeed, InputButton inputButton)
        {
            if (IsAnimating) return false;
            if (!Owner.IsValid() || !Input.Down(inputButton)) return false;

            return lastAttackTime > attackSpeed;
        }

        public override bool CanPrimaryAttack()
        {
            return CanMelee(TimeSincePrimaryAttack, SwingSpeed, InputButton.PrimaryAttack);
        }

        public override bool CanSecondaryAttack()
        {
            return CanMelee(TimeSincePrimaryAttack, StabSpeed, InputButton.SecondaryAttack);
        }

        public override void AttackPrimary()
        {
            MeleeAttack(SwingDamage, SwingForce, SwingAnimationHit, SwingAnimationMiss, SwingSound);
        }

        public override void AttackSecondary()
        {
            MeleeAttack(StabDamage, StabForce, StabAnimationHit, StabAnimationMiss, StabSound);
        }
    }
}
