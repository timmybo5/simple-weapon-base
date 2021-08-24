using SWB_Base;

partial class ExamplePlayer : PlayerBase
{

    public ExamplePlayer() : base() { }

    public override void Respawn()
    {
        base.Respawn();

        SupressPickupNotices = true;

        Inventory.Add(new SWB_CSS.Knife());
        Inventory.Add(new SWB_CSS.GrenadeHE());

        Inventory.Add(new SWB_CSS.Deagle());

        Inventory.Add(new SWB_CSS.Super90());
        Inventory.Add(new SWB_CSS.MAC10());

        Inventory.Add(new SWB_CSS.AK47());
        Inventory.Add(new SWB_CSS.M4A1());
        Inventory.Add(new SWB_CSS.M249());
        Inventory.Add(new SWB_CSS.AWP());

        Inventory.Add(new SWB_EXPLOSIVES.RPG7());

        //Inventory.Add( new SWB_CSS.M249HE() );
        //Inventory.Add( new SWB_CSS.DeagleDual() );
        //Inventory.Add( new SWB_CSS.AK47Dual() );

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
