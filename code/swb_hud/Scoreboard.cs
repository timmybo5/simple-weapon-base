using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Player;
using SWB.Shared;
using System;
using System.Linq;

namespace SWB.HUD;

public class Scoreboard : Panel
{
	Panel playerWrapper;
	bool isOpen;

	public Scoreboard()
	{
		StyleSheet.Load( "/swb_hud/Scoreboard.cs.scss" );

		var container = Add.Panel( "container" );
		var headerP = container.Add.Panel( "header" );
		headerP.Add.Label( "Name", "title name" );
		headerP.Add.Label( "Kills", "title kills" );
		headerP.Add.Label( "Deaths", "title deaths" );
		headerP.Add.Label( "Ping", "title ping" );

		playerWrapper = container.Add.Panel( "playerWrapper" );
	}

	void Open()
	{
		if ( isOpen ) return;
		isOpen = true;
		var players = Game.ActiveScene.GetAllComponents<PlayerBase>().OrderByDescending( p => p.Kills );
		var isOdd = false;

		foreach ( var player in players )
		{
			if ( player is null || !player.IsValid || player.IsBot ) continue;

			var connection = player.Network.Owner;
			var ping = MathF.Round( (connection?.Ping.CeilToInt() ?? 0) );
			var rowP = playerWrapper.Add.Panel( "row " + (isOdd ? "odd" : "") );
			rowP.Add.Label( player.DisplayName, "value name " + (!player.IsProxy ? "self" : "") );
			rowP.Add.Label( player.Kills.ToString(), "value kills" );
			rowP.Add.Label( player.Deaths.ToString(), "value deaths" );
			rowP.Add.Label( ping.ToString(), "value ping" );

			isOdd = !isOdd;
		}
	}

	void Close()
	{
		if ( !isOpen ) return;
		isOpen = false;

		playerWrapper.DeleteChildren();
	}

	public override void Tick()
	{
		var shouldOpen = Input.Down( InputButtonHelper.Score );

		if ( shouldOpen )
			Open();
		else
			Close();

		SetClass( "open", shouldOpen );
	}
}
