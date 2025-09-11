using Sandbox.UI;
using SWB.Base;
using SWB.Player;
using System;

namespace SWB.HUD;

[Group( "SWB" )]
[Title( "RootDisplay" )]
public class RootDisplay : PanelComponent
{
	[Property]
	public PlayerBase Player
	{
		get { return _player as PlayerBase; }
		set
		{
			_player = value as PlayerBase;
			if ( _player is null )
			{
				Log.Warning( "RootDisplay: Assigned Player is not of type PlayerBase. Searching ancestors." );
				_player = value.Components.GetInAncestors<PlayerBase>( true );
			}
		}
	}
	private PlayerBase _player;

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

		if ( _player is null || !_player.IsValid )
		{
			// Try to find a valid player
			_player = Components.GetInAncestors<PlayerBase>( true );
		}

		if ( _player is null || !_player.IsValid )
		{
			Log.Error( "RootDisplay: No valid PlayerBase found in ancestors." );
			return;
		}

		Panel.StyleSheet.Load( "/swb_hud/RootDisplay.cs.scss" );
		Panel.AddChild( new HealthDisplay( _player ) );
		Panel.AddChild( new AmmoDisplay( _player ) );
		Panel.AddChild( new InventoryDisplay( _player ) );
		Panel.AddChild( new Scoreboard() );
		Panel.AddChild( new KeyDisplay( _player ) );

		chatbox = new Chatbox( _player );
		Panel.AddChild( chatbox );

		killfeed = new Killfeed( _player );
		Panel.AddChild( killfeed );

		hitmarker = new Hitmarker();
		Panel.AddChild( hitmarker );
	}

	protected override void OnUpdate()
	{
		if ( Player is null || !Player.IsValid ) return;

		// Hide UI when weapon is scoping
		var weapon = _player.Inventory.Active?.Components.Get<Weapon>();

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
