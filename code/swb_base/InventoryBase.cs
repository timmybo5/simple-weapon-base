using System;
using System.Linq;
using Sandbox;
using SWB_Base;

partial class InventoryBase : BaseInventory
{
    public InventoryBase(PlayerBase player) : base(player)
    {
    }

    public override bool Add(Entity ent, bool makeActive = false)
    {
        var player = Owner as PlayerBase;
        var weapon = ent as WeaponBase;

        var showNotice = !player.SupressPickupNotices;

        if (weapon != null && IsCarryingType(ent.GetType()))
        {
            var ammo = weapon.Primary.Ammo;
            var ammoType = weapon.Primary.AmmoType;

            if (ammo > 0)
            {
                player.GiveAmmo(ammoType, ammo);

                if (showNotice)
                {
                    Sound.FromWorld("dm.pickup_ammo", ent.Position);
                    PickupFeed.OnPickup(To.Single(player), $"+{ammo} {ammoType}");
                }
            }

            // Despawn it
            ent.Delete();
            return false;
        }

        if (weapon != null && showNotice)
        {
            Sound.FromWorld("dm.pickup_weapon", ent.Position);
        }

        return base.Add(ent, makeActive);
    }

    public bool IsCarryingType(Type t)
    {
        return List.Any(x => x.GetType() == t);
    }

    public override Entity DropActive()
    {
        if (!Host.IsServer) return null;

        var ent = Owner.ActiveChild;
        if (ent == null) return null;

        var weapon = ent as WeaponBase;

        if (weapon != null && weapon.CanDrop && Drop(ent))
        {
            Owner.ActiveChild = null;
            return ent;
        }

        return null;
    }
}
