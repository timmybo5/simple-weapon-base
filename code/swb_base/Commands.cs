using Sandbox;

namespace SWB_Base
{
    internal class Commands
    {
        [ServerCmd("swb_editor", Help = "Opens the model editor")]
        public static void OpenEditor()
        {
            Client client = ConsoleSystem.Caller;

            if (client != null)
            {
                var player = client.Pawn as PlayerBase;
                player.ToggleModelEditor();
            }
        }
    }
}
