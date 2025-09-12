using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Base;
using SWB.Player;
using SWB.Shared;
using System.Collections.Generic;

namespace SWB.HUD;

struct KeyBind
{
	public string Key { get; set; }
	public string Label { get; set; }

	public KeyBind( string key, string label )
	{
		Key = key;
		Label = label;
	}
}

public class KeyDisplay : Panel
{
	PlayerBase player;

	List<KeyBind> keys = new()
	{
		new(InputButtonHelper.Menu, "Customization"),
		new(InputButtonHelper.View, "Thirdperson"),
	};

	public KeyDisplay( PlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/KeyDisplay.cs.scss" );

		keys.ForEach( keyBind =>
		{
			var wrapper = Add.Panel( "wrapper" );
			var keyIcon = wrapper.Add.Image( "", "keyIcon" );
			var buttonTexture = Input.GetGlyph( keyBind.Key, style: GlyphStyle.Dark );
			keyIcon.Texture = buttonTexture;

			wrapper.Add.Label( keyBind.Label, "label" );
		} );
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

		var hideKey = activeWeapon.Attachments.Count == 0 || activeWeapon.IsCustomizing || !WeaponSettings.Instance.Customization;
		SetClass( "hide", hideKey );
	}
}
