using Deathmatch.Hud;
using Sandbox;
using SWB_Base;

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

    public override void DoPlayerNoclip(Client player)
    {
        if (!player.HasPermission("noclip"))
            return;

        if (player.Pawn is PlayerBase basePlayer)
        {
            if (basePlayer.DevController is PlayerNoclipController)
            {
                Log.Info("Noclip Mode Off");
                basePlayer.DevController = null;
            }
            else
            {
                Log.Info("Noclip Mode On");
                basePlayer.DevController = new PlayerNoclipController();
            }
        }
    }
}
