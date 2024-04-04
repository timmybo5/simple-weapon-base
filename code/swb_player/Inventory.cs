using SWB.Shared;
using System.Collections.Generic;

namespace SWB.Player;

public class Inventory : IInventory
{
	public List<GameObject> Items { get; set; } = new();
	public GameObject Active { get; set; }

	PlayerBase player;

	public Inventory( PlayerBase player )
	{
		this.player = player;
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

	public void AddClone( GameObject gamePrefab, bool makeActive = true )
	{
		var gameObject = gamePrefab.Clone( player.GameObject, player.Transform.Position, player.Transform.Rotation, Vector3.One );
		gameObject.Name = gamePrefab.Name;
		gameObject.NetworkSpawn( player.Network.OwnerConnection );

		Add( gameObject, makeActive );
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
		Items.ForEach( ( item ) =>
		{
			item.Destroy();
		} );

		Items.Clear();
		Active = null;
	}
}
