namespace SWB.Shared;

public interface IInventory
{
	public GameObject Active { get; set; }

	public void Add( GameObject gameObject, bool makeActive = false );
	public void AddClone( GameObject gamePrefab, bool makeActive = true );
	public bool Has( GameObject gameObject );
	public void SetActive( GameObject gameObject );
	public void SetActive( string name );
	public void Clear();
}

public interface IInventoryItem
{
	/// <summary>Called on the GameObject that will be the new active one (Broadcast for networked gameObjects!)</summary>
	public void OnCarryStart();

	/// <summary>Called when the GameObject changes state to inactive (Broadcast for networked gameObjects!)</summary>
	public void OnCarryStop();
}
