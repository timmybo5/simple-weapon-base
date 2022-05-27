using Sandbox;

namespace SWB_Base
{
    public interface IInventoryBase
    {
        /// <summary>
        /// A child has been added to the Owner (player). Do we want this
        /// entity in our inventory? Yeah? Add it then.
        /// </summary>
        void OnChildAdded(Entity child);

        /// <summary>
        /// A child has been removed from our Owner. This might not even
        /// be in our inventory, if it is then we'll remove it from our list
        /// </summary>
        void OnChildRemoved(Entity child);

        /// <summary>
        /// Delete every entity we're carrying. Useful to call on death.
        /// </summary>
        void DeleteContents();

        /// <summary>
        /// Returns the number of items in the inventory
        /// </summary>
        int Count();

        /// <summary>
        /// Get the item in this slot
        /// </summary>
        Entity GetSlot(int i);

        /// <summary>
        /// Returns the index of the currently active child
        /// </summary>
        int GetActiveSlot();

        /// <summary>
        /// Set our active entity to the entity on this slot
        /// </summary>
        bool SetActiveSlot(int i, bool allowempty);

        /// <summary>
        /// Switch to the slot next to the slot we have active.
        /// </summary>
        bool SwitchActiveSlot(int idelta, bool loop);

        /// <summary>
        /// Drop the active entity. If we can't drop it, will return null
        /// </summary>
        Entity DropActive();

        /// <summary>
        /// Drop this entity. Will return true if successfully dropped.
        /// </summary>
        bool Drop(Entity ent);

        /// <summary>
        /// The active entity
        /// </summary>
        Entity Active { get; }

        /// <summary>
        /// Make this entity the active one
        /// </summary>
        bool SetActive(Entity ent);

        /// <summary>
        /// Try to add this entity to the inventory. Will return true
        /// if the entity was added successfully. 
        /// </summary>
        bool Add(Entity ent, bool makeactive = false);

        /// <summary>
        /// Returns true if this inventory contains this entity
        /// </summary>
        bool Contains(Entity ent);
    }
}
