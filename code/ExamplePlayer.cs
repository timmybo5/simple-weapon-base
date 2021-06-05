using Sandbox;
using SWB_Base;

partial class ExamplePlayer : PlayerBase
{

	public ExamplePlayer() : base() {}

	public override void Respawn()
	{
		base.Respawn();

		SupressPickupNotices = true;

		Inventory.Add( new SWB_CSS.Super90() );
		Inventory.Add( new SWB_CSS.M249() );
		Inventory.Add( new SWB_CSS.AK47() );
		Inventory.Add( new SWB_CSS.M4A1() );
		Inventory.Add( new SWB_CSS.Deagle() );
		Inventory.Add( new SWB_CSS.Knife() );

		GiveAmmo( AmmoType.SMG, 100 );
		GiveAmmo( AmmoType.Pistol, 60 );
		GiveAmmo( AmmoType.Revolver, 60 );
		GiveAmmo( AmmoType.Rifle, 60 );
		GiveAmmo( AmmoType.Shotgun, 60 );

		SupressPickupNotices = false;

	}
}