using Sandbox;
using Sandbox.UI;

namespace SWB_Base
{
    partial class PlayerBase
    {
        private Panel modelEditor;

        // Server only
        public void ToggleModelEditor()
        {
            ToggleModelEditorCL(To.Single(this));
        }

        // Client only
        public bool IsModelEditing()
        {
            return modelEditor != null;
        }

        [ClientRpc]
        public void ToggleModelEditorCL()
        {
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
