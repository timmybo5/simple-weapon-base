using Sandbox.UI;

namespace SWB.Base.UI;

public class RootWeaponDisplay : PanelComponent
{
	public Weapon Weapon { get; set; }

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			Enabled = false;
			return;
		}

		Panel.StyleSheet.Load( "/swb_base/ui/RootWeaponDisplay.cs.scss" );

		var crosshair = new Crosshair( Weapon );
		Panel.AddChild( crosshair );
	}
}
