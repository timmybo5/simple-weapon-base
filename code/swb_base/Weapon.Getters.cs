using SWB.Base.Attachments;
using SWB.Shared;

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
	/// Gets the info on where to show the muzzle effect
	/// </summary>
	public virtual Transform? GetMuzzleTransform()
	{
		var activeAttachment = GetActiveAttachmentForCategory( AttachmentCategory.Muzzle );
		var effectRenderer = GetEffectRenderer();
		var effectAttachment = "muzzle";

		if ( activeAttachment is not null )
		{
			effectAttachment = activeAttachment.EffectAttachmentOrBone;

			// If we have custom models use the attachment from those models instead of the main weapon one
			if ( CanSeeViewModel && activeAttachment.ViewModelRenderer is not null )
			{
				effectRenderer = activeAttachment.ViewModelRenderer;
			}
			else if ( !CanSeeViewModel && activeAttachment.WorldModelRenderer is not null )
			{
				effectRenderer = activeAttachment.WorldModelRenderer;
			}
		}

		return effectRenderer?.GetAttachment( effectAttachment );
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
			return Owner.AmmoCount( Primary.AmmoType ) > 0;
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
		if ( IsAiming && Primary.Bullets == 1 )
			floatMod *= AimInfo.SpreadModifier;

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

	public virtual Angles GetRecoilAngles( ShootInfo shootInfo )
	{
		var recoilX = IsAiming ? -shootInfo.Recoil * 0.4f : -shootInfo.Recoil;
		var recoilY = Game.Random.NextFloat( -0.2f, 0.2f ) * recoilX;
		var recoilAngles = new Angles( recoilX, recoilY, 0 );
		return recoilAngles;
	}
}
