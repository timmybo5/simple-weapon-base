using Sandbox;
using SWB_Player;

namespace SWB_Editor;

internal class Commands
{
    [ConCmd.Server("swb_editor_model", Help = "Opens the model editor")]
    public static void OpenModelEditor()
    {
        var client = ConsoleSystem.Caller;

        if (client != null)
        {
            var player = client.Pawn as PlayerBase;
            player.ToggleModelEditor();
        }
    }

    [ConCmd.Server("swb_editor_attachment", Help = "Opens the attachment editor")]
    public static void OpenAttachmentEditor()
    {
        var client = ConsoleSystem.Caller;

        if (client != null)
        {
            var player = client.Pawn as PlayerBase;
            player.ToggleAttachmentEditor();
        }
    }
}
