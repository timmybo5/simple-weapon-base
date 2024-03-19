using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Shared;
using System.Collections.Generic;

namespace SWB.HUD;

public class InventoryDisplay : Panel
{
	IPlayerBase player;
	List<IInventoryItem> items = new();
	Dictionary<int, Panel> itemPanels = new();

	public InventoryDisplay( IPlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/InventoryDisplay.cs.scss" );
	}

	public override void Tick()
	{
		var isAlive = player.IsAlive;
		SetClass( "hide", !isAlive );
		if ( !isAlive )
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

		var activeItem = player.Inventory.Active?.Components.Get<IInventoryItem>();

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

		items.ForEach( item =>
		{
			if ( itemPanels.ContainsKey( item.Slot ) ) return;

			var itemP = Add.Panel( "item" );
			itemP.Add.Label( item.DisplayName, "name" );
			itemP.Add.Label( item.Slot.ToString(), "slot" );
			itemP.Add.Image( item.Icon, "icon" );

			itemPanels.Add( item.Slot, itemP );
		} );
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
		if ( Input.Pressed( InputButtonHelper.Slot0 ) ) SwitchItem( 0 );
		if ( Input.Pressed( InputButtonHelper.Slot1 ) ) SwitchItem( 1 );
		if ( Input.Pressed( InputButtonHelper.Slot2 ) ) SwitchItem( 2 );
		if ( Input.Pressed( InputButtonHelper.Slot3 ) ) SwitchItem( 3 );
		if ( Input.Pressed( InputButtonHelper.Slot4 ) ) SwitchItem( 4 );
		if ( Input.Pressed( InputButtonHelper.Slot5 ) ) SwitchItem( 5 );
		if ( Input.Pressed( InputButtonHelper.Slot6 ) ) SwitchItem( 6 );
		if ( Input.Pressed( InputButtonHelper.Slot7 ) ) SwitchItem( 7 );
		if ( Input.Pressed( InputButtonHelper.Slot8 ) ) SwitchItem( 8 );
		if ( Input.Pressed( InputButtonHelper.Slot9 ) ) SwitchItem( 9 );
	}

	void SwitchItem( int slot )
	{
		var item = items.Find( item => item.Slot == slot );
		if ( item is null ) return;

		if ( item is Component component )
			player.Inventory.SetActive( component.GameObject );
	}
}
