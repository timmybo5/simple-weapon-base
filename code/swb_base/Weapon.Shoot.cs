using SWB.Shared;
using System.Collections.Generic;

namespace SWB.Base;

public partial class Weapon
{
	/// <summary>
	/// Checks if the weapon can do the provided attack
	/// </summary>
	/// <param name="shootInfo">Attack information</param>
	/// <param name="lastAttackTime">Time since this attack</param>
	/// <param name="inputButton">The input button for this attack</param>
	/// <returns></returns>
	public virtual bool CanShoot( ShootInfo shootInfo, TimeSince lastAttackTime, string inputButton )
	{
		if ( shootInfo is null || !Owner.IsValid() || !Input.Down( inputButton ) ) return false;

		if ( !HasAmmo() )
		{
			if ( Input.Pressed( inputButton ) )
			{
				PlaySound( shootInfo.DryShootSound.ResourceId );

				// Check for auto reloading
				//if ( AutoReloadSV > 0 )
				//{
				//	TimeSincePrimaryAttack = 999;
				//	TimeSinceSecondaryAttack = 999;
				//	timeSinceFired = 999;
				//	Reload();
				//}
			}

			return false;
		}

		if ( shootInfo.FiringType == FiringType.semi && !Input.Pressed( inputButton ) ) return false;
		//if ( shootInfo.FiringType == FiringType.burst )
		//{
		//	if ( burstCount > 2 ) return false;

		//	if ( Input.Down( inputButton ) && lastAttackTime > GetRealRPM( shootInfo.RPM ) )
		//	{
		//		burstCount++;
		//		return true;
		//	}

		//	return false;
		//};

		if ( shootInfo.RPM <= 0 ) return true;

		return lastAttackTime > GetRealRPM( shootInfo.RPM );
	}

	/// <summary>
	/// Checks if weapon can do the primary attack
	/// </summary>
	public virtual bool CanPrimaryShoot()
	{
		return CanShoot( Primary, TimeSincePrimaryShoot, InputButtonHelper.PrimaryAttack );
	}

	/// <summary>
	/// Checks if weapon can do the secondary attack
	/// </summary>
	public virtual bool CanSecondaryShoot()
	{
		return CanShoot( Secondary, TimeSinceSecondaryShoot, InputButtonHelper.SecondaryAttack );
	}

	public virtual void Shoot( ShootInfo shootInfo, bool isPrimary )
	{
		// Animations
		var shootAnim = GetShootAnimation( shootInfo );
		if ( !string.IsNullOrEmpty( shootAnim ) )
			ViewModelRenderer.Set( shootAnim, true );

		// Sound
		PlaySound( shootInfo.ShootSound.ResourceId );

		// Particles
		HandleShootEffects( isPrimary );

		// Bullet
		shootInfo.BulletType.Shoot( this, shootInfo );
	}

	/// <summary> A single bullet trace from start to end with a certain radius.</summary>
	public virtual SceneTraceResult TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var startsInWater = SurfaceUtil.IsPointWater( start );
		List<string> withoutTags = new() { TagsHelper.Trigger, TagsHelper.PlayerClip, TagsHelper.PassBullets, TagsHelper.ViewModel };

		if ( startsInWater )
			withoutTags.Add( TagsHelper.Water );

		var tr = Scene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithoutTags( withoutTags.ToArray() )
				.Size( radius )
				.IgnoreGameObjectHierarchy( Owner.GameObject )
				.Run();

		// Log.Info( tr.GameObject );

		return tr;
	}

	[Broadcast]
	public virtual void HandleShootEffects( bool isPrimary )
	{
		// Player
		Owner.BodyRenderer.Set( "b_attack", true );

		// Weapon
		var shootInfo = GetShootInfo( isPrimary );
		var scale = CanSeeViewModel() ? shootInfo.VMParticleScale : shootInfo.WMParticleScale;

		if ( shootInfo.MuzzleFlashParticle is not null )
			CreateParticle( shootInfo.MuzzleFlashParticle, "muzzle", scale );

		if ( shootInfo.BulletEjectParticle is not null )
			CreateParticle( shootInfo.BulletEjectParticle, "ejection_point", scale );
	}

	/// <summary> Create a weapon particle</summary>
	public virtual void CreateParticle( ParticleSystem particle, string attachment, float scale )
	{
		var effectRenderer = GetEffectRenderer();

		if ( effectRenderer is null ) return;

		var transform = effectRenderer.SceneModel.GetAttachment( attachment );

		if ( transform is null ) return;

		SceneParticles particles = new( Scene.SceneWorld, particle );
		particles?.SetControlPoint( 0, transform.Value.Position );
		particles?.SetControlPoint( 0, transform.Value.Rotation );
		particles?.SetNamedValue( "scale", scale );
		particles?.PlayUntilFinished( Task );
	}
}
