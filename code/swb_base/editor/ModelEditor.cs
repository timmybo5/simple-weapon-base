using Sandbox;
using Sandbox.UI;

namespace SWB_Base
{
    partial class PlayerBase
    {
        private Panel modelEditor;

        public void ToggleModelEditor()
        {
            Host.AssertServer();

            ToggleModelEditorCL(To.Single(this));
        }

        public bool IsModelEditing()
        {
            Host.AssertClient();

            return modelEditor != null;
        }

        [ClientRpc]
        public void ToggleModelEditorCL()
        {
            Host.AssertClient();

            if (!IsModelEditing())
            {
                Log.Info("Opened editor!");
                modelEditor = new EditorMenu();
                modelEditor.Parent = Local.Hud;
            }
            else
            {
                Log.Info("Closed editor!");
                modelEditor.Delete(true);
                modelEditor = null;
            }
        }
    }
}
