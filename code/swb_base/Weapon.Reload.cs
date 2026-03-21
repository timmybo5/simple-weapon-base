namespace SWB.Base;

public partial class Weapon
{
	public virtual void Reload()
	{
		StartReload();
	}

	void StartReload( float? reloadTimeOverride = null )
	{
		if ( IsReloading || InBoltBack || IsShooting() )
			return;

		var maxClipSize = BulletCocking ? Primary.ClipSize + 1 : Primary.ClipSize;

		if ( Primary.Ammo >= maxClipSize || Primary.ClipSize == -1 )
			return;

		if ( Owner.AmmoCount( Primary.AmmoType ) <= 0 && Primary.InfiniteAmmo != InfiniteAmmoType.reserve )
			return;

		if ( IsScoping )
			OnScopeEnd();

		IsReloading = true;

		// Time & Speed
		var isEmptyReload = ReloadEmptyTime > 0 && Primary.Ammo == 0;
		var reloadTime = reloadTimeOverride ?? (isEmptyReload ? ReloadEmptyTime : ReloadTime);
		var reloadSpeed = ReloadSpeed > 0 ? ReloadSpeed : 1f;
		ViewModelRenderer?.PlaybackRate = reloadSpeed;
		TimeSinceReload = -(reloadTime / reloadSpeed);

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
		ViewModelRenderer?.PlaybackRate = 1;
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
		ViewModelRenderer?.PlaybackRate = 1;
		ViewModelRenderer?.Set( ReloadAnim, false );
	}

	public virtual void OnShellReload()
	{
		StartReload( ShellReloadStartTime + ShellReloadInsertTime );
	}

	public virtual void OnShellReloadFinish()
	{
		IsReloading = false;

		var hasInfiniteReserve = Primary.InfiniteAmmo == InfiniteAmmoType.reserve;
		var ammo = hasInfiniteReserve ? 1 : Owner.TakeAmmo( Primary.AmmoType, 1 );

		Primary.Ammo += 1;

		if ( ammo != 0 && Primary.Ammo < Primary.ClipSize && Owner.AmmoCount( Primary.AmmoType ) > 0 )
		{
			StartReload( ShellReloadInsertTime );
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
		if ( !IsValid ) return;
		if ( !IsProxy )
			ViewModelRenderer?.Set( BoltBackAnim, true );

		// Eject shell
		await GameTask.DelaySeconds( BoltBackEjectDelay );
		if ( !IsValid ) return;
		CreateBulletEjectParticle( Primary.BulletEjectParticle, "ejection_point" );

		// Finished
		await GameTask.DelaySeconds( BoltBackTime - BoltBackEjectDelay );
		if ( !IsValid ) return;
		InBoltBack = false;
	}

	[Rpc.Broadcast]
	public virtual void HandleReloadEffects()
	{
		// Player
		Owner?.TriggerAnimation( Shared.Animations.Reload );
	}
}
