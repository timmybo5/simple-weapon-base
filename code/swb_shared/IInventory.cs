using System.Collections.Generic;

namespace SWB.Shared;

public interface IInventory
{
	public List<GameObject> Items { get; set; }
	public GameObject Active { get; set; }

	public void Add( GameObject gameObject, bool makeActive = false );
	public GameObject AddClone( GameObject gamePrefab, bool makeActive = true );
	public bool Has( GameObject gameObject );
	public void SetActive( GameObject gameObject );
	public void SetActive( string name );
	public void Clear();
}

public interface IInventoryItem : IValid
{
	/// <summary>Inventory slot</summary>
	public int Slot { get; set; }

	/// <summary>Image that represent the item on the HUD</summary>
	public string Icon { get; set; }

	/// <summary>Name that represent the item on the HUD</summary>
	public string DisplayName { get; set; }

	/// <summary>Called on the GameObject that will be the new active one (Broadcast for networked gameObjects!)</summary>
	public void OnCarryStart();

	/// <summary>Called when the GameObject stops being the active one (Broadcast for networked gameObjects!)</summary>
	public void OnCarryStop();

	/// <summary>Can the GameObject be switched out</summary>
	public bool CanCarryStop();
}
