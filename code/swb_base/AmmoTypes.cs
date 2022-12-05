using Sandbox;

namespace SWB_Base;

public partial class AmmoType : BaseNetworkable
{
    [Net] public int ID { get; set; }
    [Net] public string Name { get; set; }

    public AmmoType() { }

    public AmmoType(int ID, string name)
    {
        this.ID = ID;
        Name = name;
    }

    public bool Equals(AmmoType ammoType)
    {
        return ID == ammoType.ID && Name == ammoType.Name;
    }

    public static bool operator ==(AmmoType x, AmmoType y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(AmmoType x, AmmoType y)
    {
        return !x.Equals(y);
    }
}

public partial class AmmoTypes
{
    public static AmmoType Pistol => new(0, "Pistol");
    public static AmmoType Revolver => new(1, "Revolver");
    public static AmmoType Shotgun => new(2, "Shotgun");
    public static AmmoType SMG => new(3, "SMG");
    public static AmmoType Rifle => new(4, "Rifle");
    public static AmmoType Sniper => new(5, "Sniper");
    public static AmmoType LMG => new(6, "LMG");
    public static AmmoType Crossbow => new(7, "Crossbow");
    public static AmmoType RPG => new(8, "RPG");
    public static AmmoType Explosive => new(9, "Explosive");
    public static AmmoType Grenade => new(10, "Grenade");
}

public enum InfiniteAmmoType
{
    /// <summary>Infinite clip ammo, no need to reload</summary>
    clip = 1,
    /// <summary>Infinite reserve ammo, can always reload</summary>
    reserve = 2
}
