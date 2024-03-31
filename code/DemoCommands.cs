using SWB.Player;

namespace SWB.Demo;

internal class DemoCommands
{
	[ConCmd( "kill", Help = "Kills the player" )]
	public static void Kill()
	{
		var player = PlayerBase.GetLocal();
		player?.TakeDamage( Shared.DamageInfo.FromBullet( player.GameObject.Id, null, 100, Vector3.Zero, System.Array.Empty<string>() ) );
	}

	[ConCmd( "respawn", Help = "Respawns the player" )]
	public static void Respawn()
	{
		var player = PlayerBase.GetLocal();
		player?.Respawn();
	}

	[ConCmd( "setactive", Help = "Changes the active inventory item" )]
	public static void ChangeWeapon( string className )
	{
		var player = PlayerBase.GetLocal();
		player.Inventory.SetActive( className );
	}
}
