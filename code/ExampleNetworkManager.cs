using Sandbox.Network;
using SWB.Player;

namespace SWB.Demo;

[Group( "SWB" )]
[Title( "Example NetworkManager" )]
public class NetworkManager : Component, Component.INetworkListener
{
	[Property] public PrefabScene PlayerPrefab { get; set; }

	protected override void OnStart()
	{
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}

		base.OnStart();
	}

	// Called on host
	void INetworkListener.OnActive( Connection connection )
	{
		var player = PlayerPrefab.Clone();
		player.Name = "Player";
		player.NetworkSpawn( connection );


		var botGO = PlayerPrefab.Clone();
		var botPlayer = botGO.Components.Get<PlayerBase>();
		botPlayer.IsBot = true;
		botGO.Name = "Bot";
		botGO.NetworkSpawn();
	}
}
