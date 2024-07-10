using SWB.Shared;
using System;
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
		if ( (IsReloading && !ShellReloading) || (IsReloading && ShellReloading && !ShellReloadingShootCancel) || InBoltBack ) return false;
		if ( shootInfo is null || !Owner.IsValid() || !Input.Down( inputButton ) || (IsRunning && Secondary is null) ) return false;

		if ( !HasAmmo() )
		{
			if ( Input.Pressed( inputButton ) )
			{
				// Check for auto reloading
				if ( Settings.AutoReload && lastAttackTime > GetRealRPM( shootInfo.RPM ) )
				{
					TimeSincePrimaryShoot = 999;
					TimeSinceSecondaryShoot = 999;

					if ( ShellReloading )
						OnShellReload();
					else
						Reload();

					return false;
				}

				// Dry fire
				if ( shootInfo.DryShootSound is not null )
					PlaySound( shootInfo.DryShootSound.ResourceId );
			}

			return false;
		}

		if ( shootInfo.FiringType == FiringType.semi && !Input.Pressed( inputButton ) ) return false;
		if ( shootInfo.FiringType == FiringType.burst )
		{
			if ( burstCount > 2 ) return false;

			if ( Input.Down( inputButton ) && lastAttackTime > GetRealRPM( shootInfo.RPM ) )
			{
				burstCount++;
				return true;
			}

			return false;
		};

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
		// Ammo
		shootInfo.Ammo -= 1;

		// Animations
		var shootAnim = GetShootAnimation( shootInfo );
		if ( !string.IsNullOrEmpty( shootAnim ) )
			ViewModelRenderer.Set( shootAnim, true );

		// Sound
		if ( shootInfo.ShootSound is not null )
			PlaySound( shootInfo.ShootSound.ResourceId );

		// Particles
		HandleShootEffects( isPrimary );

		// Barrel smoke
		barrelHeat += 1;

		// Recoil
		Owner.EyeAnglesOffset += GetRecoilAngles( shootInfo );

		// UI
		BroadcastUIEvent( "shoot", GetRealRPM( shootInfo.RPM ) );

		// Bullet
		for ( int i = 0; i < shootInfo.Bullets; i++ )
		{
			var realSpread = IsScoping ? 0 : GetRealSpread( shootInfo.Spread );
			var spreadOffset = shootInfo.BulletType.GetRandomSpread( realSpread );
			ShootBullet( isPrimary, spreadOffset );
		}
	}

	[Broadcast]
	public virtual void ShootBullet( bool isPrimary, Vector3 spreadOffset )
	{
		var shootInfo = GetShootInfo( isPrimary );
		shootInfo.BulletType.Shoot( this, shootInfo, spreadOffset );
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
		var scale = CanSeeViewModel ? shootInfo.VMParticleScale : shootInfo.WMParticleScale;
		var muzzleTransform = GetMuzzleTransform();

		// Bullet eject
		if ( shootInfo.BulletEjectParticle is not null )
		{
			if ( !BoltBack )
			{
				if ( !ShellReloading || (ShellReloading && ShellEjectDelay == 0) )
				{
					CreateParticle( shootInfo.BulletEjectParticle, "ejection_point", scale );
				}
				else
				{
					var delayedEject = async () =>
					{
						await GameTask.DelaySeconds( ShellEjectDelay );
						if ( !IsValid ) return;
						CreateParticle( shootInfo.BulletEjectParticle, "ejection_point", scale );
					};
					delayedEject();
				}
			}
			else if ( shootInfo.Ammo > 0 )
			{
				AsyncBoltBack( GetRealRPM( shootInfo.RPM ) );
			}
		}

		if ( !muzzleTransform.HasValue ) return;

		// Muzzle flash
		if ( shootInfo.MuzzleFlashParticle is not null )
			CreateParticle( shootInfo.MuzzleFlashParticle, muzzleTransform.Value, scale, ( particles ) => ParticleToMuzzlePos( particles ) );

		// Barrel smoke
		if ( !IsProxy && shootInfo.BarrelSmokeParticle is not null && barrelHeat >= shootInfo.ClipSize * 0.75 )
			CreateParticle( shootInfo.BarrelSmokeParticle, muzzleTransform.Value, shootInfo.VMParticleScale, ( particles ) => ParticleToMuzzlePos( particles ) );
	}

	void ParticleToMuzzlePos( SceneParticles particles )
	{
		var transform = GetMuzzleTransform();

		if ( transform.HasValue )
		{
			// Apply velocity to prevent muzzle shift when moving fast
			particles?.SetControlPoint( 0, transform.Value.Position + Owner.Velocity * 0.03f );
			particles?.SetControlPoint( 0, transform.Value.Rotation );
		}
		else
		{
			particles?.Delete();
		}
	}

	/// <summary>Create a bullet impact effect</summary>
	public virtual void CreateBulletImpact( SceneTraceResult tr )
	{
		// Sound
		tr.Surface.PlayCollisionSound( tr.HitPosition );

		// Particles
		if ( tr.Surface.ImpactEffects.Bullet is not null )
		{
			var effectPath = Game.Random.FromList( tr.Surface.ImpactEffects.Bullet, "particles/impact.generic.smokepuff.vpcf" );

			if ( effectPath is not null )
			{
				// Surface def for flesh has wrong blood particle linked
				if ( effectPath.Contains( "impact.flesh" ) )
				{
					effectPath = "particles/impact.flesh.bloodpuff.vpcf";
				}
				else if ( effectPath.Contains( "impact.wood" ) )
				{
					effectPath = "particles/impact.generic.smokepuff.vpcf";
				}

				var p = new SceneParticles( Scene.SceneWorld, effectPath );
				p.SetControlPoint( 0, tr.HitPosition );
				p.SetControlPoint( 0, Rotation.LookAt( tr.Normal ) );
				p.PlayUntilFinished( TaskSource.Create() );
			}
		}

		// Decal
		if ( tr.Surface.ImpactEffects.BulletDecal is not null )
		{
			var decalPath = Game.Random.FromList( tr.Surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );

			if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalDef ) )
			{
				var decalEntry = Game.Random.FromList( decalDef.Decals );

				var gameObject = Scene.CreateObject();
				//gameObject.SetParent( tr.GameObject, false );
				gameObject.Transform.Position = tr.HitPosition;
				gameObject.Transform.Rotation = Rotation.LookAt( -tr.Normal );

				var decalRenderer = gameObject.Components.Create<DecalRenderer>();
				decalRenderer.Material = decalEntry.Material;
				decalRenderer.Size = new( decalEntry.Height.GetValue(), decalEntry.Height.GetValue(), decalEntry.Depth.GetValue() );
				gameObject.DestroyAsync( 30f );
			}
		}
	}

	/// <summary>Create a weapon particle</summary>
	public virtual void CreateParticle( ParticleSystem particle, string attachment, float scale, Action<SceneParticles> OnFrame = null )
	{
		var effectRenderer = GetEffectRenderer();

		if ( effectRenderer is null || effectRenderer.SceneModel is null ) return;

		var transform = effectRenderer.SceneModel.GetAttachment( attachment );

		if ( !transform.HasValue ) return;

		CreateParticle( particle, transform.Value, scale, OnFrame );
	}

	public virtual void CreateParticle( ParticleSystem particle, Transform transform, float scale, Action<SceneParticles> OnFrame = null )
	{
		SceneParticles particles = new( Scene.SceneWorld, particle );
		particles?.SetControlPoint( 0, transform.Position );
		particles?.SetControlPoint( 0, transform.Rotation );
		particles?.SetNamedValue( "scale", scale );

		if ( CanSeeViewModel )
			particles.Tags.Add( TagsHelper.ViewModel );

		particles?.PlayUntilFinished( Task, OnFrame );
	}
}
