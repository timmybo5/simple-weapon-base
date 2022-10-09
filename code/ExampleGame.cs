using Deathmatch.Hud;
using Sandbox;

[Library("swb")]
public partial class ExampleGame : Sandbox.Game
{
    public DeathmatchHud UI;

    public ExampleGame()
    {
        if (IsServer)
        {
            UI = new DeathmatchHud();
        }
    }

    public override void ClientJoined(Client client)
    {
        base.ClientJoined(client);

        var player = new ExamplePlayer(client);
        player.Respawn();

        client.Pawn = player;
    }
}
