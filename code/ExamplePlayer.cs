using Sandbox;
using SWB_Base;

partial class ExamplePlayer : PlayerBase
{

	public ExamplePlayer() : base() {}

	public override void Respawn()
	{
		base.Respawn();

		SupressPickupNotices = true;

		Inventory.Add( new SWB_CSS.Knife() );
		Inventory.Add( new SWB_CSS.GrenadeHE() );

		Inventory.Add( new SWB_CSS.Deagle() );

		Inventory.Add( new SWB_CSS.Super90() );

		Inventory.Add( new SWB_CSS.AK47() );
		Inventory.Add( new SWB_CSS.M4A1() );
		Inventory.Add( new SWB_CSS.M249() );
		Inventory.Add( new SWB_CSS.AWP() );

		Inventory.Add( new SWB_CSS.AWP() );

		Inventory.Add( new SWB_EXPLOSIVES.RPG7() );
		
		//Inventory.Add( new SWB_CSS.M249HE() );
		//Inventory.Add( new SWB_CSS.DeagleDual() );
		//Inventory.Add( new SWB_CSS.AK47Dual() );

		SupressPickupNotices = false;
	}
}