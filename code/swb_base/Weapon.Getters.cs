﻿using SWB.Player;

namespace SWB.Base;

public partial class Weapon
{
	public virtual SkinnedModelRenderer GetEffectRenderer()
	{
		SkinnedModelRenderer effectModel = WorldModelRenderer;

		if ( CanSeeViewModel )
			effectModel = ViewModelRenderer;

		return effectModel;
	}

	/// <summary>
	/// Gets the correct shoot animation
	/// </summary>
	/// <param name="shootInfo">Info used for the current attack</param>
	/// <returns></returns>
	public virtual string GetShootAnimation( ShootInfo shootInfo )
	{
		if ( IsAiming && (!string.IsNullOrEmpty( shootInfo.ShootAimedAnim )) )
		{
			return shootInfo.ShootAimedAnim;
		}
		else if ( shootInfo.Ammo == 0 && !string.IsNullOrEmpty( shootInfo.ShootEmptyAnim ) )
		{
			return shootInfo.ShootEmptyAnim;
		}

		return shootInfo.ShootAnim;
	}

	/// <summary>
	/// If there is usable ammo left
	/// </summary>
	public bool HasAmmo()
	{
		if ( Primary.InfiniteAmmo == InfiniteAmmoType.clip )
			return true;

		if ( Primary.ClipSize == -1 )
		{
			if ( Owner is PlayerBase player )
			{
				return player.AmmoCount( Primary.AmmoType ) > 0;
			}
			return true;
		}

		if ( Primary.Ammo == 0 )
			return false;

		return true;
	}

	public ShootInfo GetShootInfo( bool isPrimary )
	{
		return isPrimary ? Primary : Secondary;
	}

	public bool IsShooting()
	{
		if ( Secondary is null )
			return GetRealRPM( Primary.RPM ) > TimeSincePrimaryShoot;

		return GetRealRPM( Primary.RPM ) > TimeSincePrimaryShoot || GetRealRPM( Secondary.RPM ) > TimeSinceSecondaryShoot;
	}

	public static float GetRealRPM( int rpm )
	{
		return 60f / rpm;
	}

	public virtual float GetRealSpread( float baseSpread = -1 )
	{
		if ( !Owner.IsValid() ) return 0;

		float spread = baseSpread != -1 ? baseSpread : Primary.Spread;
		float floatMod = 1f;

		// Ducking
		if ( IsCrouching && !IsAiming )
			floatMod -= 0.25f;

		// Aiming
		if ( IsAiming /*&& this is not WeaponBaseShotty*/ )
			floatMod /= 4;

		if ( !Owner.IsOnGround )
		{
			// Jumping
			floatMod += 0.75f;
		}
		else if ( Owner.Velocity.Length > 100 )
		{
			// Moving 
			floatMod += 0.25f;
		}

		return spread * floatMod;
	}
}