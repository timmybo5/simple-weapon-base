using SWB.Player;
using System.Linq;

namespace SWB.Demo;

internal class DemoCommands
{
	[ConCmd( "kill", Help = "Kills the player" )]
	public static void Kill()
	{
		var player = PlayerBase.GetLocal();
		var suicideDamageInfo = new Shared.DamageInfo
		{
			Attacker = player?.GameObject ?? null,
			Inflictor = null,
			Damage = 99999,
		};
		player?.TakeDamage( suicideDamageInfo );
	}

	[ConCmd( "thirdperson", Help = "Toggles thirdperson" )]
	public static void Thirdperson()
	{
		var player = PlayerBase.GetLocal();
		var cam = player.CameraMovement as CameraMovement;
		cam.Distance = cam.Distance > 0 ? 0 : 60;
	}

	[ConCmd( "thirdperson_shoulder", Help = "Switches thirdperson shoulder" )]
	public static void ThirdpersonShoulder()
	{
		var player = PlayerBase.GetLocal();
		var cam = player.CameraMovement as CameraMovement;
		cam.ShoulderOffset = -cam.ShoulderOffset;
	}

	[ConCmd( "respawn", Help = "Respawns the player (host only)" )]
	public static void Respawn()
	{
		var player = PlayerBase.GetLocal();
		if ( !player.IsHost ) return;
		player?.Respawn();
	}

	[ConCmd( "god", Help = "Toggles godmode (host only)" )]
	public static void GodMode()
	{
		var player = PlayerBase.GetLocal();
		if ( !player.IsHost ) return;
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
		if ( !player.IsHost ) return;

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
