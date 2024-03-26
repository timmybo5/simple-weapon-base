using Sandbox.UI;
using SWB.Base;
using SWB.Player;

namespace SWB.HUD;

[Group( "SWB" )]
[Title( "RootDisplay" )]
public class RootDisplay : PanelComponent
{
	[Property] public PlayerBase Player { get; set; }

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			this.GameObject.Destroy();
			return;
		}

		Panel.StyleSheet.Load( "/swb_hud/RootDisplay.cs.scss" );
		Panel.AddChild( new HealthDisplay( Player ) );
		Panel.AddChild( new AmmoDisplay( Player ) );
		Panel.AddChild( new InventoryDisplay( Player ) );
	}

	protected override void OnUpdate()
	{
		if ( Player is null || !Player.IsValid ) return;

		var weapon = Player.Inventory.Active?.Components.Get<Weapon>();
		Panel.SetClass( "hide", weapon is null || weapon.IsScoping );
	}
}
