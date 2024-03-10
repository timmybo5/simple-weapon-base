namespace SWB.Base;

public partial class Weapon
{
	public virtual void Reload()
	{
		if ( IsReloading || /*IsAnimating || InBoltBack ||*/ IsShooting() )
			return;

		var maxClipSize = BulletCocking ? Primary.ClipSize + 1 : Primary.ClipSize;

		if ( Primary.Ammo >= maxClipSize || Primary.ClipSize == -1 )
			return;

		var isEmptyReload = ReloadEmptyTime > 0 && Primary.Ammo == 0;
		TimeSinceReload = -(isEmptyReload ? ReloadEmptyTime : ReloadTime);

		//if ( !isEmptyReload && Primary.Ammo == 0 && General.BoltBackTime > -1 )
		//{
		//	TimeSinceReload -= General.BoltBackTime;

		//	if ( Game.IsServer )
		//		_ = AsyncBoltBack( General.ReloadTime, General.BoltBackAnim, General.BoltBackTime, General.BoltBackEjectDelay, Primary.BulletEjectParticle );
		//}

		//if ( Owner is ISWBPlayer player )
		//{
		if ( Owner.AmmoCount( Primary.AmmoType ) <= 0 && Primary.InfiniteAmmo != InfiniteAmmoType.reserve )
			return;
		//}

		IsReloading = true;

		// Anim
		var shootAnim = ReloadAnim;
		if ( isEmptyReload && !string.IsNullOrEmpty( ReloadEmptyAnim ) )
		{
			shootAnim = ReloadEmptyAnim;
		}

		ViewModelRenderer.Set( shootAnim, true );

		// Player anim
		HandleReloadEffects();
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
		var maxClipSize = BulletCocking && Primary.Ammo > 0 ? Primary.ClipSize + 1 : Primary.ClipSize;

		if ( Primary.InfiniteAmmo == InfiniteAmmoType.reserve )
		{
			Primary.Ammo = maxClipSize;
			return;
		}

		//if ( Owner is ISWBPlayer player )
		//{
		var ammo = Owner.TakeAmmo( Primary.AmmoType, maxClipSize - Primary.Ammo );

		if ( ammo == 0 )
			return;

		Primary.Ammo += ammo;
		//}
	}

	[Broadcast]
	public virtual void HandleReloadEffects()
	{
		// Player
		Owner.BodyRenderer.Set( "b_reload", true );
	}
}
