using System.Collections.Generic;
using Sandbox;

namespace SWB_Base;

public class InventoryBase : IInventoryBase
{
    public Entity Owner { get; init; }
    public List<Entity> List = new();

    public virtual Entity Active
    {
        get
        {
            return (Owner as PlayerBase)?.ActiveChild;
        }

        set
        {
            if (Owner is PlayerBase player)
            {
                player.ActiveChild = value;
            }
        }
    }

    public InventoryBase(PlayerBase player)
    {
        Owner = player;
    }

    public virtual Entity DropActive()
    {
        if (!Host.IsServer || Owner is not PlayerBase player) return null;

        var ent = player.ActiveChild;
        if (ent == null) return null;

        if (ent is WeaponBase weapon && weapon.CanDrop && Drop(ent))
        {
            player.ActiveChild = null;
            return ent;
        }

        return null;
    }

    /// <summary>
    /// Return true if this item belongs in the inventory
    /// </summary>
    public virtual bool CanAdd(Entity ent)
    {
        if (ent is CarriableBase bc && bc.CanCarry(Owner))
            return true;

        return false;
    }

    public virtual void DeleteContents()
    {
        Host.AssertServer();

        foreach (var item in List.ToArray())
        {
            item.Delete();
        }

        List.Clear();
    }

    public virtual Entity GetSlot(int i)
    {
        if (List.Count <= i) return null;
        if (i < 0) return null;

        return List[i];
    }

    public virtual int Count() => List.Count;

    public virtual int GetActiveSlot()
    {
        var ae = Active;
        var count = Count();

        for (int i = 0; i < count; i++)
        {
            if (List[i] == ae)
                return i;
        }

        return -1;
    }

    public virtual void Pickup(Entity ent)
    {

    }

    public virtual void OnChildAdded(Entity child)
    {
        if (!CanAdd(child))
            return;

        if (List.Contains(child))
            throw new System.Exception("Trying to add to inventory multiple times. This is gated by Entity:OnChildAdded and should never happen!");

        List.Add(child);
    }

    public virtual void OnChildRemoved(Entity child)
    {
        if (List.Remove(child))
        {
            // On removed etc
        }
    }

    public virtual bool SetActiveSlot(int i, bool evenIfEmpty = false)
    {
        var ent = GetSlot(i);
        if (Active == ent)
            return false;

        if (!evenIfEmpty && ent == null)
            return false;

        Active = ent;
        return ent.IsValid();
    }

    public virtual bool SwitchActiveSlot(int idelta, bool loop)
    {
        var count = Count();
        if (count == 0) return false;

        var slot = GetActiveSlot();
        var nextSlot = slot + idelta;

        if (loop)
        {
            while (nextSlot < 0) nextSlot += count;
            while (nextSlot >= count) nextSlot -= count;
        }
        else
        {
            if (nextSlot < 0) return false;
            if (nextSlot >= count) return false;
        }

        return SetActiveSlot(nextSlot, false);
    }

    public virtual bool Drop(Entity ent)
    {
        if (!Host.IsServer)
            return false;

        if (!Contains(ent))
            return false;

        ent.Parent = null;

        if (ent is CarriableBase bc)
        {
            bc.OnCarryDrop(Owner);
        }

        return true;
    }

    public virtual bool Contains(Entity ent)
    {
        return List.Contains(ent);
    }

    public virtual bool SetActive(Entity ent)
    {
        if (Active == ent) return false;
        if (!Contains(ent)) return false;

        Active = ent;
        return true;
    }

    public virtual bool Add(Entity ent, bool makeActive = false)
    {
        Host.AssertServer();

        // Can't pickup if already owned
        if (ent.Owner != null)
            return false;

        if (!CanAdd(ent))
            return false;

        if (ent is not CarriableBase carriable)
            return false;

        if (!carriable.CanCarry(Owner))
            return false;

        ent.Parent = Owner;
        carriable.OnCarryStart(Owner);

        if (makeActive)
        {
            SetActive(ent);
        }

        return true;
    }
}
