using SWB.Player;
using System.Linq;

namespace SWB.Demo;

internal class Commands
{
	private static PlayerBase GetPlayer()
	{
		var players = Game.ActiveScene.GetAllComponents<PlayerBase>();
		return players.First( ( player ) => player.Network.OwnerConnection == Connection.Local );
	}

	[ConCmd( "kill", Help = "Opens the offsets editor" )]
	public static void Kill()
	{
		var player = GetPlayer();
		player?.TakeDamage( Shared.DamageInfo.FromBullet( 100, Vector3.Zero ) );
	}

	[ConCmd( "respawn", Help = "Opens the offsets editor" )]
	public static void Respawn()
	{
		var player = GetPlayer();
		player?.Respawn();
	}
}
