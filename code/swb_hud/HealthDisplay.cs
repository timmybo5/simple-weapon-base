using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Shared;

namespace SWB.HUD;

public class HealthDisplay : Panel
{
	IPlayerBase player;
	Label healthLabel;

	public HealthDisplay( IPlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/HealthDisplay.cs.scss" );

		Add.Label( "health", "name" );
		healthLabel = Add.Label( "", "health" );
	}

	public override void Tick()
	{
		var isAlive = player.IsAlive;
		SetClass( "hide", !isAlive );

		if ( !isAlive ) return;

		var healthPer = ((float)player.Health) / 100f;

		if ( healthLabel is not null )
		{
			healthLabel.Text = player.Health.ToString();
			healthLabel.Style.FontColor = new Color( 1, 1 * healthPer, 1 * healthPer );
		}
	}
}
