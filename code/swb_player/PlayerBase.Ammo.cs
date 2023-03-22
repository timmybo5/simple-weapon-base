using System;
using System.Collections.Generic;
using Sandbox;

namespace SWB_Base;

partial class PlayerBase
{
    [Net] public IList<int> Ammo { get; set; }

    public virtual void ClearAmmo()
    {
        Ammo.Clear();
    }

    public virtual int AmmoCount(AmmoType ammoType)
    {
        if (Ammo == null) return 0;
        if (Ammo.Count <= ammoType.ID) return 0;

        return Ammo[ammoType.ID];
    }

    public virtual bool SetAmmo(AmmoType ammoType, int amount)
    {
        if (!Game.IsServer) return false;
        if (Ammo == null) return false;

        while (Ammo.Count <= ammoType.ID)
        {
            Ammo.Add(0);
        }

        Ammo[ammoType.ID] = amount;
        return true;
    }

    public virtual bool GiveAmmo(AmmoType ammoType, int amount)
    {
        if (!Game.IsServer) return false;
        if (Ammo == null) return false;

        SetAmmo(ammoType, AmmoCount(ammoType) + amount);
        return true;
    }

    public virtual int TakeAmmo(AmmoType ammoType, int amount)
    {
        if (Ammo == null) return 0;

        var available = AmmoCount(ammoType);
        amount = Math.Min(available, amount);

        SetAmmo(ammoType, available - amount);

        return amount;
    }

    public virtual bool HasAmmo(AmmoType ammoType)
    {
        return AmmoCount(ammoType) > 0;
    }
}
