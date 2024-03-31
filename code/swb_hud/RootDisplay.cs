using Sandbox.UI;
using SWB.Base;
using SWB.Player;
using System;

namespace SWB.HUD;

[Group( "SWB" )]
[Title( "RootDisplay" )]
public class RootDisplay : PanelComponent
{
	[Property] public PlayerBase Player { get; set; }
	Killfeed killfeed;

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			this.GameObject.Destroy();
			return;
		}

		Panel.StyleSheet.Load( "/swb_hud/RootDisplay.cs.scss" );
		Panel.AddChild( new HealthDisplay( Player ) );
		Panel.AddChild( new AmmoDisplay( Player ) );
		Panel.AddChild( new InventoryDisplay( Player ) );

		killfeed = new Killfeed( Player );
		Panel.AddChild( killfeed );
	}

	protected override void OnUpdate()
	{
		if ( Player is null || !Player.IsValid ) return;

		// Hide UI when weapon is scoping
		var weapon = Player.Inventory.Active?.Components.Get<Weapon>();

		if ( weapon is not null )
			Panel.SetClass( "hide", weapon.IsScoping );
	}

	public void AddToKillFeed( Guid attackerId, Guid victimId, string inflictor )
	{
		killfeed?.AddKillEntry( attackerId, victimId, inflictor );
	}
}
