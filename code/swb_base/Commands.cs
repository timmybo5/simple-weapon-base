using SWB.Player;

namespace SWB.Base;

public class Commands
{
	[ConCmd( "swb_host_customization", Help = "Enable the weapon customization menu (Q)" )]
	public static void SetWeaponCustomization( int enable )
	{
		var player = PlayerBase.GetLocal();
		if ( !player.IsHost ) return;

		WeaponSettings.Instance.Customization = enable != 0;
	}

	[ConCmd( "swb_host_autoreload", Help = "Reload weapons automatically while shooting if clip is empty" )]
	public static void SetWeaponAutoReload( int enable )
	{
		var player = PlayerBase.GetLocal();
		if ( !player.IsHost ) return;

		WeaponSettings.Instance.AutoReload = enable != 0;
	}
}
