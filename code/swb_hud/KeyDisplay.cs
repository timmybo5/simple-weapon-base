using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Base;
using SWB.Shared;

namespace SWB.HUD;

public class KeyDisplay : Panel
{
	IPlayerBase player;

	public KeyDisplay( IPlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/KeyDisplay.cs.scss" );

		var keyIcon = this.Add.Image( "", "keyIcon" );
		var buttonTexture = Input.GetGlyph( InputButtonHelper.Menu, style: GlyphStyle.Dark );
		keyIcon.Texture = buttonTexture;

		this.Add.Label( "Customization", "label" );
	}

	public override void Tick()
	{
		var activeGO = player.Inventory.Active;

		if ( activeGO is null )
		{
			SetClass( "hide", true );
			return;
		}

		var activeWeapon = activeGO.Components.Get<Weapon>();
		var isValidWeapon = activeWeapon is not null;
		SetClass( "hide", !isValidWeapon );
		if ( !isValidWeapon ) return;

		var showKey = activeWeapon.Attachments.Count == 0 || activeWeapon.IsCustomizing;
		SetClass( "hide", showKey );
	}
}
