using Deathmatch.Hud;
using Sandbox;

[Library("swb")]
public partial class ExampleGame : GameManager
{
    public DeathmatchHud UI;

    public ExampleGame()
    {
        if (Game.IsServer)
        {
            UI = new DeathmatchHud();
        }
    }

    public override void ClientJoined(IClient client)
    {
        base.ClientJoined(client);

        var player = new ExamplePlayer(client);
        player.Respawn();

        client.Pawn = player;
    }
}
