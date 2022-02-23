using Sandbox;

[Library("swb")]
public partial class ExampleGame : Sandbox.Game
{
    public ExampleGame()
    {
        if (IsServer)
        {
            new DeathmatchHud();
        }
    }

    public override void ClientJoined(Client client)
    {
        base.ClientJoined(client);

        var player = new ExamplePlayer();
        client.Pawn = player;

        player.Respawn();
    }
}
