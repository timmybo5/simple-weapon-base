using SWB_Base;

partial class ExamplePlayer : PlayerBase
{

    public ExamplePlayer() : base() { }

    public override void Respawn()
    {
        base.Respawn();

        SupressPickupNotices = true;

        Inventory.Add(new SWB_WEAPONS.Bayonet());
        Inventory.Add(new SWB_WEAPONS.DEAGLE());
        Inventory.Add(new SWB_WEAPONS.SPAS12());
        Inventory.Add(new SWB_WEAPONS.FAL());
        Inventory.Add(new SWB_WEAPONS.L96A1());

        Inventory.Add(new SWB_EXPLOSIVES.RPG7());

        GiveAmmo(AmmoType.SMG, 100);
        GiveAmmo(AmmoType.Pistol, 60);
        GiveAmmo(AmmoType.Revolver, 60);
        GiveAmmo(AmmoType.Rifle, 60);
        GiveAmmo(AmmoType.Shotgun, 60);
        GiveAmmo(AmmoType.Sniper, 60);
        GiveAmmo(AmmoType.Grenade, 60);
        GiveAmmo(AmmoType.RPG, 60);

        SupressPickupNotices = false;
    }
}
