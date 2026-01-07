using SWB.Shared;
using System.Linq;

namespace SWB.Demo;

/*
 * Override methods here to implement custom bot behaviour.
*/

[Group( "SWB" )]
[Title( "Demo Bot" )]
public class DemoBot : DemoPlayer
{
	// Hide all the properties we don't need
	[Hide] public new CameraComponent Camera { get; set; }
	[Hide] public new CameraComponent ViewModelCamera { get; set; }
	[Hide] public new PanelComponent RootDisplay { get; set; }
	[Hide] public new Voice Voice { get; set; }
	[Hide] public new bool JumpSpamPrevention { get; set; }
	[Hide] public new float NoclipSpeed { get; set; }

	public static DemoBot AddBot()
	{
		var bots = Game.ActiveScene.GetAllComponents<DemoBot>();
		var botName = "Bot " + (bots.Count() + 1);
		var networkManager = Game.ActiveScene.Components.Get<DemoNetworkManager>( FindMode.EnabledInSelfAndChildren );
		var botGO = networkManager.BotPrefab.Clone();
		var botPlayer = botGO.Components.Get<DemoBot>();
		botPlayer.Dresser.Randomize();
		botGO.Name = botName;
		botGO.NetworkSpawn();

		return botPlayer;
	}

	protected override void OnAwake()
	{
		IsBot = true;
		base.OnAwake();
	}

	public override void Respawn( Transform? respawnAt = null )
	{
		base.Respawn( respawnAt );

		// Equip random weapon
		var rndItem = TableUtil.GetRandom( Inventory.Items );
		Inventory.SetActive( rndItem );
	}

	/*
	 * Example of how to make a bot shoot 
	*/

	//TimeSince timeSinceFire;
	//protected override void OnFixedUpdate()
	//{
	//	base.OnFixedUpdate();

	//	if ( IsProxy || timeSinceFire < 1f ) return;
	//	timeSinceFire = 0;

	//	if ( Inventory.Active is null ) return;
	//	var activeWep = Inventory.Active.GetComponent<Weapon>();
	//	activeWep.Shoot( activeWep.Primary, true );
	//}
}
