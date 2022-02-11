
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Scoreboard : Sandbox.UI.Scoreboard<ScoreboardEntry>
{

    public Scoreboard()
    {
        StyleSheet.Load("deathmatch_dep/ui/Scoreboard.scss");
    }

    protected override void AddHeader()
    {
        Header = Add.Panel("header");
        Header.Add.Label("player", "name");
        Header.Add.Label("kills", "kills");
        Header.Add.Label("deaths", "deaths");
        Header.Add.Label("ping", "ping");
    }
}

public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
{
}
