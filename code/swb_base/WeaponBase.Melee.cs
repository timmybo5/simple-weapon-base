using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* 
 * Weapon base for melee weapons
*/

namespace SWB_Base
{
	public partial class WeaponBaseMelee : WeaponBase
	{
		public virtual string SwingAnimationHit => "";
		public virtual string SwingAnimationMiss => "";
		public virtual string StabAnimationHit => "";
		public virtual string StabAnimationMiss => "";
		public virtual string SwingSound => "";
		public virtual string StabSound => "";
		public virtual string MissSound => "";
		public virtual string HitWorldSound => "";
		public virtual float SwingSpeed => 1f;
		public virtual float StabSpeed => 1f;
		public virtual float SwingDamage => 50;
		public virtual float StabDamage => 100;
		public virtual float SwingForce => 25f;
		public virtual float StabForce => 50f;
		public virtual float DamageDistance => 25f;
		public virtual float ImpactSize => 10f;

		public override void Reload() { }

		[ClientRpc]
		public virtual void DoMeleeEffects( string animation, string sound )
		{
			ViewModelEntity?.SetAnimBool( animation, true );
			PlaySound( sound );
		}

		public virtual void MeleeAttack( float damage, float force, string hitAnimation, string missAnimation, string sound )
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			var hitEntity = true;
			var pos = Owner.EyePos;
			var forward = Owner.EyeRot.Forward;
			var trace = Trace.Ray( pos, pos + forward * DamageDistance )
				.Ignore( this )
				.Ignore( Owner )
				.Size( ImpactSize )
				.Run();

			if ( !trace.Entity.IsValid() || trace.Entity.IsWorld )
			{
				hitAnimation = missAnimation;
				sound = !trace.Entity.IsValid() ? MissSound : HitWorldSound;
				hitEntity = false;
			}

			DoMeleeEffects( hitAnimation, sound );

			if ( !hitEntity || !IsServer ) return;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( trace.EndPos, forward * force, damage )
					.UsingTraceResult( trace )
					.WithAttacker( Owner )
					.WithWeapon( this );

				trace.Entity.TakeDamage( damageInfo );
			}
		}

		private bool CanMelee( TimeSince lastAttackTime, float attackSpeed, InputButton inputButton )
		{
			if ( IsAnimating ) return false;
			if ( !Owner.IsValid() || !Input.Down( inputButton ) ) return false;

			return lastAttackTime > attackSpeed;
		}

		public override bool CanPrimaryAttack()
		{
			return CanMelee( TimeSincePrimaryAttack, SwingSpeed, InputButton.Attack1 );
		}

		public override bool CanSecondaryAttack()
		{
			return CanMelee( TimeSincePrimaryAttack, StabSpeed, InputButton.Attack2 );
		}

		public override void AttackPrimary()
		{
			MeleeAttack( SwingDamage, SwingForce, SwingAnimationHit, SwingAnimationMiss, SwingSound );
		}

		public override void AttackSecondary()
		{
			MeleeAttack( StabDamage, StabForce, StabAnimationHit, StabAnimationMiss, StabSound );
		}
	}
}
