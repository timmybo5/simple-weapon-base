using SWB.Shared;

namespace SWB.Player;

public class Inventory : Component, IInventory
{
	[Sync] public NetList<GameObject> Items { get; set; } = new();
	[Sync] public new GameObject Active { get; set; }

	PlayerBase player;

	protected override void OnAwake()
	{
		player = Components.Get<PlayerBase>();
	}

	public void Add( GameObject gameObject, bool makeActive = false )
	{
		if ( !Has( gameObject ) )
		{
			Items.Add( gameObject );
		}

		if ( makeActive )
			SetActive( gameObject );
		else
		{
			if ( gameObject.Components.TryGet<IInventoryItem>( out var item ) )
			{
				item.OnCarryStop();
			}
		}
	}

	public GameObject AddClone( GameObject gamePrefab, bool makeActive = true )
	{
		CloneConfig config = new( player.WorldTransform, player.GameObject, false, gamePrefab.Name );
		var gameObject = gamePrefab.Clone( config );
		gameObject.NetworkSpawn( player.Network.Owner );

		Add( gameObject, makeActive );
		return gameObject;
	}

	public bool Has( GameObject gameObject )
	{
		return Items.Contains( gameObject );
	}

	public void SetActive( GameObject gameObject )
	{
		if ( !Has( gameObject ) || Active == gameObject ) return;

		if ( Active is not null && Active.Components.TryGet<IInventoryItem>( out var oldActive ) )
		{
			if ( !oldActive.CanCarryStop() ) return;
			oldActive.OnCarryStop();
		}

		if ( gameObject.Components.TryGet<IInventoryItem>( out var newActive, FindMode.EverythingInSelf ) )
		{
			newActive.OnCarryStart();
		}

		Active = gameObject;
	}

	public void SetActive( string name )
	{
		foreach ( var item in Items )
		{
			if ( item.Name == name )
			{
				SetActive( item );
				break;
			}
		}
	}

	public void Clear()
	{
		foreach ( var item in Items )
		{
			item.Destroy();
		}

		Items.Clear();
		Active = null;
	}
}
