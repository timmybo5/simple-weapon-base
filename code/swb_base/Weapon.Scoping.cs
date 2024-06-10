namespace SWB.Base;

public partial class Weapon
{
	public async void OnScopeStart()
	{
		await GameTask.DelaySeconds( ScopeInfo.ScopeInDelay );
		if ( !IsAiming || IsScoping || IsReloading ) return;

		IsScoping = true;
		ViewModelHandler.ShouldDraw = false;

		if ( ScopeInfo.ScopeInSound is not null )
			PlaySound( ScopeInfo.ScopeInSound.ResourceId );
	}

	public void OnScopeEnd()
	{
		if ( !IsScoping ) return;

		IsScoping = false;
		ViewModelHandler.ShouldDraw = true;

		if ( ScopeInfo.ScopeOutSound is not null )
			PlaySound( ScopeInfo.ScopeOutSound.ResourceId );
	}
}
