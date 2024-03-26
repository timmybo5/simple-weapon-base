using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Base;
using SWB.Shared;
using System;

namespace SWB.HUD;

public class AmmoDisplay : Panel
{
	IPlayerBase player;

	Label clipLabel;
	Label reserveLabel;

	Color reserveColor = new Color32( 200, 200, 200 ).ToColor();
	Color emptyColor = new Color32( 175, 175, 175, 200 ).ToColor();

	public AmmoDisplay( IPlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/AmmoDisplay.cs.scss" );

		Add.Label( "ammo", "name" );
		clipLabel = Add.Label( "", "clip" );
		reserveLabel = Add.Label( "", "reserve" );
	}

	public override void Tick()
	{
		var isAlive = player.IsAlive;
		var weapon = player.Inventory.Active?.Components.Get<Weapon>();
		var hide = !isAlive || weapon is null;
		SetClass( "hide", hide );
		if ( hide ) return;

		var hasClipSize = weapon.Primary.ClipSize > 0;
		var reserveAmmo = Math.Min( player.AmmoCount( weapon.Primary.AmmoType ), 999 );
		var hasAmmo = weapon.Primary.Ammo > 0;

		if ( weapon.Primary.InfiniteAmmo != InfiniteAmmoType.clip )
		{
			var clipAmmo = hasClipSize ? weapon.Primary.Ammo : reserveAmmo;
			clipAmmo = Math.Min( clipAmmo, 999 );

			clipLabel.Text = clipAmmo.ToString();
			clipLabel.Style.FontColor = clipAmmo == 0 ? emptyColor : Color.White;
		}
		else
		{
			clipLabel.Text = "∞";
			clipLabel.Style.FontColor = Color.White;
		}

		if ( hasClipSize )
		{
			if ( weapon.Primary.InfiniteAmmo == 0 )
			{
				reserveLabel.Text = "  /  " + reserveAmmo;
				reserveLabel.Style.FontColor = reserveAmmo == 0 ? emptyColor : reserveColor;
			}
			else
			{
				reserveLabel.Text = "  /  ∞";
				reserveLabel.Style.FontColor = reserveColor;
			}
		}
		else if ( hasAmmo )
		{
			clipLabel.Text = weapon.Primary.Ammo.ToString();
		}
		else
		{
			reserveLabel.Text = "";
		}
	}
}
