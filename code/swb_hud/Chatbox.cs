using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Player;
using SWB.Shared;
using System;

namespace SWB.HUD;

public class Chatbox : Panel
{
	IPlayerBase player;

	Panel history;
	TextEntry textEntry;

	public Chatbox( IPlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/Chatbox.cs.scss" );

		var msgArea = Add.Panel( "msgArea" );
		history = msgArea.Add.Panel( "history" );

		textEntry = Add.TextEntry();
		textEntry.AddClass( "entry" );
		textEntry.Placeholder = "Type here";
		textEntry.AddEventListener( "onsubmit", () => Submit() );
		textEntry.AcceptsFocus = true;
		textEntry.AllowEmojiReplace = true;
		textEntry.MaxLength = 60;
	}

	void Open()
	{
		AddClass( "open" );
		textEntry.Focus();
		AsyncScrollToBottom( 1 );
	}

	void Close()
	{
		RemoveClass( "open" );
	}

	void Submit()
	{
		Close();

		var msg = textEntry.Text.Trim();
		textEntry.Text = "";

		if ( string.IsNullOrWhiteSpace( msg ) )
			return;

		Say( msg );
	}

	public void AddEntry( Guid senderId, string msg )
	{
		var senderGO = Game.ActiveScene.Directory.FindByGuid( senderId );
		var sender = senderGO?.Components.Get<PlayerBase>();

		if ( sender is null ) return;

		var msgP = history.Add.Panel( "msgWrapper" );
		msgP.Add.Label( sender.Network.OwnerConnection.DisplayName + ":", "name " + (!sender.IsProxy ? "self" : "") );
		msgP.Add.Label( msg, "msg" );

		AsyncMessageHide( msgP, 5000 );
		AsyncScrollToBottom( 1 );

		Log.Info( sender.Network.OwnerConnection.DisplayName + ": " + msg );
	}

	async void AsyncScrollToBottom( int delay )
	{
		await GameTask.Delay( delay );
		history.TryScrollToBottom();
	}

	async void AsyncMessageHide( Panel msgWrapper, int delay )
	{
		await GameTask.Delay( delay );
		msgWrapper?.AddClass( "fadeOut" );
		await GameTask.Delay( 500 );
		msgWrapper?.AddClass( "fadedOut" );
	}

	public override void Tick()
	{
		if ( Input.Pressed( InputButtonHelper.Chat ) )
			Open();
	}

	[ConCmd( "say", Help = "Send a chat message" )]
	public static void Say( string msg )
	{
		var player = PlayerBase.GetLocal();
		SendMsg( player.GameObject.Id, msg );
	}

	[Broadcast]
	public static void SendMsg( Guid senderId, string msg )
	{
		var player = PlayerBase.GetLocal();
		var rootDisplay = player.RootDisplay as RootDisplay;
		rootDisplay.AddChatEntry( senderId, msg );
	}
}
