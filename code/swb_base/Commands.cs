using Sandbox;

namespace SWB_Base
{
    internal class Commands
    {
        [ServerCmd("swb_editor_model", Help = "Opens the model editor")]
        public static void OpenModelEditor()
        {
            Client client = ConsoleSystem.Caller;

            if (client != null)
            {
                var player = client.Pawn as PlayerBase;
                player.ToggleModelEditor();
            }
        }

        [ServerCmd("swb_editor_attachment", Help = "Opens the attachment editor")]
        public static void OpenAttachmentEditor()
        {
            Client client = ConsoleSystem.Caller;

            if (client != null)
            {
                var player = client.Pawn as PlayerBase;
                player.ToggleAttachmentEditor();
            }
        }
    }
}
