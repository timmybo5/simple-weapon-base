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
	Chatbox chatbox;
	Killfeed killfeed;
	Hitmarker hitmarker;

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
		Panel.AddChild( new Scoreboard() );
		Panel.AddChild( new KeyDisplay( Player ) );

		chatbox = new Chatbox( Player );
		Panel.AddChild( chatbox );

		killfeed = new Killfeed( Player );
		Panel.AddChild( killfeed );

		hitmarker = new Hitmarker();
		Panel.AddChild( hitmarker );
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

	public void AddChatEntry( Guid senderId, string msg )
	{
		chatbox.AddEntry( senderId, msg );
	}

	public void CreateHitmarker( bool isKill )
	{
		hitmarker.Create( isKill );
	}
}
