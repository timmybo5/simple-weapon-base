using SWB.Base;
using SWB.Player;
using System.Linq;

namespace SWB.Editor;

internal class Commands
{
	static OffsetEditor offsetEditor;

	private static PlayerBase GetPlayer()
	{
		var players = Game.ActiveScene.GetAllComponents<PlayerBase>();
		return players.First( ( player ) => player.Network.OwnerConnection == Connection.Local );
	}

	[ConCmd( "swb_editor_offsets", Help = "Opens the offsets editor" )]
	public static void OpenOffsetsEditor()
	{
		var player = GetPlayer();
		var weaponGO = player.Inventory.Active;
		var weapon = weaponGO.Components.Get<Weapon>();

		if ( offsetEditor is not null )
		{
			offsetEditor.Delete();
			offsetEditor = null;

			if ( weapon.ViewModelHandler is not null )
				weapon.ViewModelHandler.EditorMode = false;

			return;
		}

		if ( weapon is not null )
		{
			var screenPanel = weapon.RootPanel;
			offsetEditor = new OffsetEditor( weapon );
			screenPanel.Panel.AddChild( offsetEditor );

			if ( weapon.ViewModelHandler is not null )
				weapon.ViewModelHandler.EditorMode = true;
		}
	}
}
