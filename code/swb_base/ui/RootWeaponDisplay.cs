using Sandbox.UI;
using SWB.Shared;

namespace SWB.Base.UI;

public class RootWeaponDisplay : PanelComponent
{
	public IPlayerBase Player { get; set; }
	public Weapon Weapon { get; set; }

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			Enabled = false;
			return;
		}

		Panel.StyleSheet.Load( "/swb_base/ui/RootWeaponDisplay.cs.scss" );

		var crosshair = new Crosshair( Player, Weapon );
		Panel.AddChild( crosshair );
	}
}
