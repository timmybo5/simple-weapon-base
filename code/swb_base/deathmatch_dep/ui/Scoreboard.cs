
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Scoreboard : Sandbox.UI.Scoreboard<ScoreboardEntry>
{

    public Scoreboard()
    {
        StyleSheet.Load( "swb_base/deathmatch_dep/ui/scss/Scoreboard.scss" );
    }

    protected override void AddHeader()
    {
        Header = Add.Panel( "header" );
        Header.Add.Label( "player", "name" );
        Header.Add.Label( "kills", "kills" );
        Header.Add.Label( "deaths", "deaths" );
        Header.Add.Label( "ping", "ping" );
        Header.Add.Label( "fps", "fps" );
    }
}

public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
{
    public Label Fps;

    public ScoreboardEntry()
    {
        Fps = Add.Label( "", "fps" );
    }

    public override void UpdateFrom( PlayerScore.Entry entry )
    {
        base.UpdateFrom( entry );

        Fps.Text = entry.Get<int>( "fps", 0 ).ToString();
    }
}
