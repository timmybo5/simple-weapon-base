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
		if ( shootInfo is null || !Owner.IsValid() || !Input.Down( inputButton ) || ((IsRunning || TimeSinceRunning < 0.1f) && Secondary is null) ) return false;

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
		}
		;

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
		if ( shootInfo.InfiniteAmmo != InfiniteAmmoType.clip )
			shootInfo.Ammo -= 1;

		// Animations
		var shootAnim = GetShootAnimation( shootInfo );
		if ( ViewModelRenderer is not null && !string.IsNullOrEmpty( shootAnim ) )
			ViewModelRenderer.Set( shootAnim, true );

		// Sound
		if ( shootInfo.ShootSound is not null )
			PlaySound( shootInfo.ShootSound.ResourceId );

		// Particles
		HandleShootEffects( isPrimary );

		// Barrel smoke
		barrelHeat += 1;

		// Recoil / AimPunch
		var recoilAngles = GetRecoilAngles( shootInfo );
		if ( shootInfo.UseAimPunch )
		{
			// Apply aimpunch (camera recoil with recovery)
			Owner.ApplyAimPunch( recoilAngles, shootInfo.AimPunchRecoverySpeed );
		}
		else
		{
			// Apply instant recoil (legacy behavior)
			Owner.ApplyEyeAnglesOffset( recoilAngles );
		}

		// Screenshake
		if ( shootInfo.ScreenShake is not null )
			Owner.ShakeScreen( shootInfo.ScreenShake );

		// UI
		BroadcastUIEvent( "shoot", GetRealRPM( shootInfo.RPM ) );

		// Bullet
		for ( int i = 0; i < shootInfo.Bullets; i++ )
		{
			var realSpread = GetRealSpread( shootInfo.Spread );

			if ( IsScoping )
				realSpread *= ScopeInfo.Spread;

			var spreadOffset = shootInfo.BulletType.GetRandomSpread( realSpread );
			ShootBullet( isPrimary, spreadOffset );
		}
	}

	[Rpc.Broadcast( NetFlags.Unreliable )]
	public virtual void ShootBullet( bool isPrimary, Vector3 spreadOffset )
	{
		if ( !IsValid ) return;
		var shootInfo = GetShootInfo( isPrimary );
		shootInfo?.BulletType?.Shoot( this, shootInfo, spreadOffset );
	}

	/// <summary> A single bullet trace from start to end with a certain radius.</summary>
	public static SceneTraceResult TraceBullet( GameObject toIgnoreGO, Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var startsInWater = SurfaceUtil.IsPointWater( start );
		var withoutTags = new List<string>() { TagsHelper.Trigger, TagsHelper.PlayerClip, TagsHelper.PassBullets, TagsHelper.ViewModel };

		if ( startsInWater )
			withoutTags.Add( TagsHelper.Water );

		var tr = Game.ActiveScene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithoutTags( withoutTags.ToArray() )
				.Size( radius )
				.IgnoreGameObjectHierarchy( toIgnoreGO )
				.Run();

		// Log.Info( tr.GameObject );

		return tr;
	}

	/// <summary> A single bullet trace from start to end with a certain radius.</summary>
	public virtual SceneTraceResult TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		return TraceBullet( Owner.GameObject, start, end, radius );
	}

	[Rpc.Broadcast( NetFlags.Unreliable )]
	public virtual void HandleShootEffects( bool isPrimary )
	{
		if ( !IsValid || Owner is null ) return;

		// Player
		Owner.TriggerAnimation( Shared.Animations.Attack );

		// Weapon
		var shootInfo = GetShootInfo( isPrimary );
		if ( shootInfo is null ) return;

		var muzzleObj = GetMuzzleObject();
		var scale = CanSeeViewModel ? shootInfo.VMParticleScale : shootInfo.WMParticleScale;

		// Bullet eject
		if ( shootInfo.BulletEjectParticle is not null )
		{
			if ( !BoltBack )
			{
				if ( !ShellReloading || (ShellReloading && ShellEjectDelay == 0) )
				{
					CreateParticle( shootInfo.BulletEjectParticle, "ejection_point", scale, attachmentYawOnly: true );
				}
				else
				{
					var delayedEject = async () =>
					{
						await GameTask.DelaySeconds( ShellEjectDelay );
						if ( !IsValid ) return;
						CreateParticle( shootInfo.BulletEjectParticle, "ejection_point", scale, attachmentYawOnly: true );
					};
					delayedEject();
				}
			}
			else if ( shootInfo.Ammo > 0 )
			{
				AsyncBoltBack( GetRealRPM( shootInfo.RPM ) );
			}
		}

		if ( muzzleObj is null ) return;
		var muzzleScale = CanSeeViewModel ? shootInfo.VMParticleScale : shootInfo.WMMuzzleParticleScale;

		// Muzzle flash
		if ( shootInfo.MuzzleFlashParticle is not null )
			CreateParticle( shootInfo.MuzzleFlashParticle, muzzleObj, muzzleScale );

		// Barrel smoke
		if ( !IsProxy && shootInfo.BarrelSmokeParticle is not null && barrelHeat >= shootInfo.ClipSize * 0.75 )
			CreateParticle( shootInfo.BarrelSmokeParticle, muzzleObj, muzzleScale );
	}

	/// <summary>Create a bullet impact effect</summary>
	public static GameObject CreateBulletImpact( SceneTraceResult tr )
	{
		// Sound
		SoundHandle soundHandle = null;

		if ( tr.Surface.SoundCollection.Bullet is not null )
		{
			var sound = tr.Surface.SoundCollection.Bullet;
			sound.Distance = 10000;
			soundHandle = Sound.Play( sound );
		}

		soundHandle ??= Sound.Play( "impact-bullet-generic" );
		soundHandle.Position = tr.HitPosition;

		// Decal & Particles
		var impactPrefab = tr.Surface.PrefabCollection.BulletImpact;
		if ( impactPrefab is null || !impactPrefab.IsValid ) return null;

		var cloneConfig = new CloneConfig()
		{
			Name = "bullet_decal",
			StartEnabled = true,
			Transform = new()
			{
				Position = tr.HitPosition,
				Rotation = Rotation.LookAt( -tr.Normal ),
			},
			//Parent = tr.GameObject,
		};
		var decalGO = impactPrefab.Clone( cloneConfig );
		decalGO.NetworkMode = NetworkMode.Never;
		decalGO.DestroyAsync( 30f );
		return decalGO;
	}

	/// <summary>Create a weapon particle</summary>
	public virtual void CreateParticle( GameObject particle, string attachment, float scale, Action<GameObject> OnParticleCreated = null, bool attachmentYawOnly = false )
	{
		var effectRenderer = GetEffectRenderer();
		if ( effectRenderer is null || effectRenderer.SceneModel is null ) return;

		var transform = effectRenderer.SceneModel.GetAttachment( attachment );
		if ( !transform.HasValue ) return;

		// Useful when creating model particles
		if ( attachmentYawOnly )
		{
			var pitch = CanSeeViewModel ? ViewModelHandler.WorldRotation.Pitch() : WorldRotation.Pitch();
			var yaw = transform.Value.Rotation.Yaw();
			var newRot = Rotation.From( new Angles( 0, yaw, -pitch ) );
			transform = transform.Value.WithRotation( newRot );
		}

		CreateParticle( particle, null, transform.Value, scale, OnParticleCreated );
	}

	/// <summary>Create a weapon particle</summary>
	public virtual void CreateParticle( GameObject particle, GameObject parent, float scale, Action<GameObject> OnParticleCreated = null )
	{
		CreateParticle( particle, parent, new Transform(), scale, OnParticleCreated );
	}

	/// <summary>Create a weapon particle</summary>
	public virtual void CreateParticle( GameObject particle, Transform transform, float scale, Action<GameObject> OnParticleCreated = null )
	{
		CreateParticle( particle, null, transform, scale, OnParticleCreated );
	}

	/// <summary>Create a weapon particle</summary>
	public virtual void CreateParticle( GameObject particle, GameObject parent, Transform transform, float scale, Action<GameObject> OnParticleCreated = null )
	{
		var go = particle.Clone( transform.WithScale( scale ), parent );

		if ( CanSeeViewModel )
			go.Tags.Add( TagsHelper.ViewModel );

		if ( OnParticleCreated is not null )
		{
			var p = go.GetComponentInChildren<ParticleEffect>();
			p.OnParticleCreated += ( p ) =>
			{
				OnParticleCreated.Invoke( go );
			};
		}
	}
}
