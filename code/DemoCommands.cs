namespace SWB.Demo;

internal class DemoCommands
{
	[ConCmd( "kill", Help = "Kills the player" )]
	public static void Kill()
	{
		var player = Editor.Commands.GetPlayer();
		player?.TakeDamage( Shared.DamageInfo.FromBullet( 100, Vector3.Zero, System.Array.Empty<string>() ) );
	}

	[ConCmd( "respawn", Help = "Respawns the player" )]
	public static void Respawn()
	{
		var player = Editor.Commands.GetPlayer();
		player?.Respawn();
	}

	[ConCmd( "setactive", Help = "Changes the active inventory item" )]
	public static void ChangeWeapon( string className )
	{
		var player = Editor.Commands.GetPlayer();
		player.Inventory.SetActive( className );
	}
}
