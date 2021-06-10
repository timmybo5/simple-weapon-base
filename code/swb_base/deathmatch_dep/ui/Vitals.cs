
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Vitals : Panel
{
	public Label Health;

	public Vitals()
	{
		Health = Add.Label( "100", "health" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		Health.Text = $"{player.Health.CeilToInt()}";
		Health.SetClass( "danger", player.Health < 40.0f );
	}
}
