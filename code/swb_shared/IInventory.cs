namespace SWB.Shared;

public interface IInventory
{
	public GameObject Active { get; set; }

	public void Add( GameObject gameObject, bool makeActive = false );
	public void AddClone( GameObject gamePrefab, bool makeActive = true );
	public bool Has( GameObject gameObject );
	public void SetActive( GameObject gameObject );
}
