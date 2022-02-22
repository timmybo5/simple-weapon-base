using System;
using System.Linq;
using Sandbox;
using SWB_Base;

partial class InventoryBase : BaseInventory
{
    public InventoryBase(PlayerBase player) : base(player)
    {
    }

    public override Entity DropActive()
    {
        if (!Host.IsServer || Owner is not Player player) return null;

        var ent = player.ActiveChild;
        if (ent == null) return null;

        if (ent is WeaponBase weapon && weapon.CanDrop && Drop(ent))
        {
            player.ActiveChild = null;
            return ent;
        }

        return null;
    }
}
