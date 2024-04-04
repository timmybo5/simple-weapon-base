using SWB.Base;
using SWB.Player;

namespace SWB.Editor;

internal class Commands
{
	static OffsetEditor offsetEditor;

	[ConCmd( "swb_editor_offsets", Help = "Opens the offsets editor" )]
	public static void OpenOffsetsEditor()
	{
		var player = PlayerBase.GetLocal();
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
			var screenPanel = player.RootDisplay;
			offsetEditor = new OffsetEditor( weapon );
			screenPanel.Panel.AddChild( offsetEditor );

			if ( weapon.ViewModelHandler is not null )
				weapon.ViewModelHandler.EditorMode = true;
		}
	}
}
