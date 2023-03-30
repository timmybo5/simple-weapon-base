using Sandbox;
using SWB_HUD;

[Library("swb")]
public partial class ExampleGame : GameManager
{
    public HUD UI;

    public ExampleGame()
    {
        if (Game.IsServer)
        {
            UI = new HUD();
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
