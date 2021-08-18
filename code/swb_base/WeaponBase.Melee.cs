using Sandbox;

/* 
 * Weapon base for melee weapons
*/

namespace SWB_Base
{
    public partial class WeaponBaseMelee : WeaponBase
    {
        public virtual string SwingAnimationHit => ""; // Animation to play for the primary attack
        public virtual string SwingAnimationMiss => ""; // Animation to play when missing the primary attack
        public virtual string StabAnimationHit => ""; // Animation to play for the secondary attack
        public virtual string StabAnimationMiss => ""; // Animation to play when missing the secondary attack
        public virtual string SwingSound => ""; // Sound to play for the primary attack
        public virtual string StabSound => ""; // Sound to play for the secondary attack
        public virtual string MissSound => ""; // Sound to play when missing an attack
        public virtual string HitWorldSound => ""; // Sound to play when hitting the world
        public virtual float SwingSpeed => 1f; // Primary attack speed ( lower is faster )
        public virtual float StabSpeed => 1f; // Secondary attack speed ( lower is faster )
        public virtual float SwingDamage => 50; // Primary attack damage
        public virtual float StabDamage => 100; // Secondary attack damage
        public virtual float SwingForce => 25f; // Primary attack force
        public virtual float StabForce => 50f; // Secondary attack force
        public virtual float DamageDistance => 25f; // Attack range
        public virtual float ImpactSize => 10f; // Attack impact size

        public override void Reload() { }

        [ClientRpc]
        public virtual void DoMeleeEffects(string animation, string sound)
        {
            ViewModelEntity?.SetAnimBool(animation, true);
            PlaySound(sound);
        }

        public virtual void MeleeAttack(float damage, float force, string hitAnimation, string missAnimation, string sound)
        {
            TimeSincePrimaryAttack = 0;
            TimeSinceSecondaryAttack = 0;

            var hitEntity = true;
            var pos = Owner.EyePos;
            var forward = Owner.EyeRot.Forward;
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

            if (!hitEntity || !IsServer) return;

            using (Prediction.Off())
            {
                var damageInfo = DamageInfo.FromBullet(trace.EndPos, forward * force, damage)
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
            return CanMelee(TimeSincePrimaryAttack, SwingSpeed, InputButton.Attack1);
        }

        public override bool CanSecondaryAttack()
        {
            return CanMelee(TimeSincePrimaryAttack, StabSpeed, InputButton.Attack2);
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
