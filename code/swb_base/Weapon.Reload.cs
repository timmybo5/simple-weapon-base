namespace SWB.Base;

public partial class Weapon
{
	public virtual void Reload()
	{
		if ( IsReloading || InBoltBack || IsShooting() )
			return;

		var maxClipSize = BulletCocking ? Primary.ClipSize + 1 : Primary.ClipSize;

		if ( Primary.Ammo >= maxClipSize || Primary.ClipSize == -1 )
			return;

		var isEmptyReload = ReloadEmptyTime > 0 && Primary.Ammo == 0;
		TimeSinceReload = -(isEmptyReload ? ReloadEmptyTime : ReloadTime);

		if ( Owner.AmmoCount( Primary.AmmoType ) <= 0 && Primary.InfiniteAmmo != InfiniteAmmoType.reserve )
			return;

		if ( IsScoping )
			OnScopeEnd();

		IsReloading = true;

		// Anim
		var reloadAnim = ReloadAnim;
		if ( isEmptyReload && !string.IsNullOrEmpty( ReloadEmptyAnim ) )
		{
			reloadAnim = ReloadEmptyAnim;
		}

		ViewModelRenderer?.Set( reloadAnim, true );

		// Player anim
		HandleReloadEffects();

		//Boltback
		if ( !isEmptyReload && Primary.Ammo == 0 && BoltBack )
		{
			TimeSinceReload -= BoltBackTime;
			AsyncBoltBack( ReloadTime );
		}
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

		var ammo = Owner.TakeAmmo( Primary.AmmoType, maxClipSize - Primary.Ammo );

		if ( ammo == 0 )
			return;

		Primary.Ammo += ammo;
	}

	public virtual void CancelShellReload()
	{
		IsReloading = false;
		ViewModelRenderer.Set( ReloadAnim, false );
	}

	public virtual void OnShellReload()
	{
		ReloadTime = ShellReloadStartTime + ShellReloadInsertTime;
		Reload();
	}

	public virtual void OnShellReloadFinish()
	{
		IsReloading = false;

		var hasInfiniteReserve = Primary.InfiniteAmmo == InfiniteAmmoType.reserve;
		var ammo = hasInfiniteReserve ? 1 : Owner.TakeAmmo( Primary.AmmoType, 1 );

		Primary.Ammo += 1;

		if ( ammo != 0 && Primary.Ammo < Primary.ClipSize )
		{
			ReloadTime = ShellReloadInsertTime;
			Reload();
		}
		else
		{
			CancelShellReload();
		}
	}

	async void AsyncBoltBack( float boltBackDelay )
	{
		InBoltBack = true;

		// Start boltback
		await GameTask.DelaySeconds( boltBackDelay );
		if ( !IsProxy )
			ViewModelRenderer?.Set( BoltBackAnim, true );

		// Eject shell
		await GameTask.DelaySeconds( BoltBackEjectDelay );
		var scale = CanSeeViewModel ? Primary.VMParticleScale : Primary.WMParticleScale;
		CreateParticle( Primary.BulletEjectParticle, "ejection_point", scale );

		// Finished
		await GameTask.DelaySeconds( BoltBackTime - BoltBackEjectDelay );
		InBoltBack = false;
	}

	[Broadcast]
	public virtual void HandleReloadEffects()
	{
		// Player
		Owner.BodyRenderer.Set( "b_reload", true );
	}
}
