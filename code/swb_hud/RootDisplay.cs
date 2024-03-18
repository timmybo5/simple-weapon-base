using Sandbox.UI;
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
}
