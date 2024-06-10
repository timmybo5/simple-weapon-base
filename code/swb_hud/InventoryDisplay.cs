using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Base;
using SWB.Shared;
using System.Collections.Generic;
using System.Linq;

namespace SWB.HUD;

public class InventoryDisplay : Panel
{
	IPlayerBase player;
	List<IInventoryItem> items = new();
	Dictionary<int, Panel> itemPanels = new();
	IInventoryItem activeItem;

	public InventoryDisplay( IPlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/InventoryDisplay.cs.scss" );
	}

	public override void Tick()
	{
		var isCustomizing = activeItem is Weapon weapon && weapon.IsCustomizing;
		var hide = !player.IsAlive || isCustomizing;
		SetClass( "hide", hide );

		if ( !player.IsAlive )
		{
			if ( items.Count > 0 )
				items.Clear();

			return;
		}

		if ( items.Count != player.Inventory.Items.Count )
		{
			GetInventoryItems();
			Rebuild();
		}

		activeItem = player.Inventory.Active?.Components.Get<IInventoryItem>();

		if ( activeItem is not null )
		{
			foreach ( var entry in itemPanels )
			{
				entry.Value.SetClass( "active", entry.Key == activeItem.Slot );
			}
		}

		CheckInput();
	}

	void Rebuild()
	{
		DeleteChildren();
		itemPanels.Clear();
		var sortedItems = items.OrderBy( i => i.Slot );

		foreach ( var item in sortedItems )
		{
			if ( itemPanels.ContainsKey( item.Slot ) ) return;

			var itemP = Add.Panel( "item" );
			itemP.Add.Label( item.DisplayName, "name" );
			itemP.Add.Label( item.Slot.ToString(), "slot" );
			itemP.Add.Image( item.Icon, "icon" );

			itemPanels.Add( item.Slot, itemP );
		}
	}

	void GetInventoryItems()
	{
		items.Clear();

		player.Inventory.Items.ForEach( itemGO =>
		{
			var item = itemGO.Components.Get<IInventoryItem>( true );

			if ( item is not null )
				items.Add( item );
		} );
	}

	void CheckInput()
	{
		if ( activeItem is null || !activeItem.CanCarryStop() ) return;
		if ( Input.Pressed( InputButtonHelper.Slot0 ) ) SwitchItem( 0 );
		else if ( Input.Pressed( InputButtonHelper.Slot1 ) ) SwitchItem( 1 );
		else if ( Input.Pressed( InputButtonHelper.Slot2 ) ) SwitchItem( 2 );
		else if ( Input.Pressed( InputButtonHelper.Slot3 ) ) SwitchItem( 3 );
		else if ( Input.Pressed( InputButtonHelper.Slot4 ) ) SwitchItem( 4 );
		else if ( Input.Pressed( InputButtonHelper.Slot5 ) ) SwitchItem( 5 );
		else if ( Input.Pressed( InputButtonHelper.Slot6 ) ) SwitchItem( 6 );
		else if ( Input.Pressed( InputButtonHelper.Slot7 ) ) SwitchItem( 7 );
		else if ( Input.Pressed( InputButtonHelper.Slot8 ) ) SwitchItem( 8 );
		else if ( Input.Pressed( InputButtonHelper.Slot9 ) ) SwitchItem( 9 );
		else if ( Input.MouseWheel.y > 0 ) SwitchToNext();
		else if ( Input.MouseWheel.y < 0 ) SwitchToPrev();
	}

	void SwitchToNext()
	{
		var sortedItems = items.OrderBy( i => i.Slot );
		var maxSlotItem = sortedItems.Last();
		var currSlot = maxSlotItem.Slot == activeItem.Slot ? -1 : activeItem.Slot;

		foreach ( var item in sortedItems )
		{
			if ( item.Slot > currSlot )
			{
				SwitchItem( item.Slot );
				break;
			}
		}
	}

	void SwitchToPrev()
	{
		var sortedItems = items.OrderByDescending( i => i.Slot );
		var minSlotItem = sortedItems.Last();
		var currSlot = minSlotItem.Slot == activeItem.Slot ? 10 : activeItem.Slot;

		foreach ( var item in sortedItems )
		{
			if ( item.Slot < currSlot )
			{
				SwitchItem( item.Slot );
				break;
			}
		}
	}

	void SwitchItem( int slot )
	{
		var item = items.Find( item => item.Slot == slot );
		if ( item is null ) return;

		if ( item is Component component )
		{
			player.Inventory.SetActive( component.GameObject );
		}
	}
}
