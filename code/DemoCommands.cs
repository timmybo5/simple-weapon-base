using SWB.Player;
using System.Linq;

namespace SWB.Demo;

internal class DemoCommands
{
	[ConCmd( "kill", Help = "Kills the player" )]
	public static void Kill()
	{
		var player = PlayerBase.GetLocal();
		player?.TakeDamage( Shared.DamageInfo.FromBullet( player.GameObject.Id, null, 99999, Vector3.Zero, Vector3.Zero, System.Array.Empty<string>() ) );
	}

	[ConCmd( "respawn", Help = "Respawns the player (host only)" )]
	public static void Respawn()
	{
		var player = PlayerBase.GetLocal();
		if ( !player.Network.OwnerConnection.IsHost ) return;
		player?.Respawn();
	}

	[ConCmd( "god", Help = "Toggles godmode (host only)" )]
	public static void GodMode()
	{
		var player = PlayerBase.GetLocal();
		if ( !player.Network.OwnerConnection.IsHost ) return;
		player.GodMode = !player.GodMode;
		Log.Info( (player.GodMode ? "Enabled" : "Disabled") + " Godmode" );
	}

	[ConCmd( "setactive", Help = "Changes the active inventory item" )]
	public static void ChangeWeapon( string className )
	{
		var player = PlayerBase.GetLocal();
		player.Inventory.SetActive( className );
	}

	[ConCmd( "bot", Help = "Adds a bot" )]
	public static void Bot()
	{
		var player = PlayerBase.GetLocal();
		if ( !player.Network.OwnerConnection.IsHost ) return;

		var networkManager = Game.ActiveScene.Components.Get<DemoNetworkManager>( FindMode.EnabledInSelfAndChildren );
		var botGO = networkManager.PlayerPrefab.Clone();
		var botPlayer = botGO.Components.Get<PlayerBase>();

		var players = Game.ActiveScene.GetAllComponents<PlayerBase>();
		var bots = players.Count( ( player ) => player.IsBot );
		var botName = "Bot " + (bots + 1);

		botPlayer.IsBot = true;
		botGO.Name = botName;
		botGO.NetworkSpawn();

		Log.Info( botName + " joined the game!" );
	}
}
