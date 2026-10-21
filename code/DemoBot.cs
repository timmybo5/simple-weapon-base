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
		RandomizeAppearance( botPlayer.Dresser );
		botGO.Name = botName;
		botGO.NetworkSpawn();

		return botPlayer;
	}

	static void RandomizeAppearance( Dresser dresser )
	{
		// Randomize() never picks a skin; on the human model the skin decides the body (male/female) and skin tone
		var skins = ResourceLibrary.GetAll<Clothing>().Where( c => c.Category == Clothing.ClothingCategory.Skin ).ToList();
		var skin = TableUtil.GetRandom( skins );
		if ( skin is null ) return;

		var isFemale = skin.HumanSkinTags?.Contains( "female" ) ?? false;

		dresser.Clothing.RemoveAll( e => e.Clothing?.Category == Clothing.ClothingCategory.Skin
			|| (isFemale && e.Clothing?.Category.ToString().StartsWith( "FacialHair" ) == true) );
		dresser.Clothing.Add( new ClothingContainer.ClothingEntry( skin ) );

		dresser.EyeColor = Game.Random.Float( 0f, 1f );
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
