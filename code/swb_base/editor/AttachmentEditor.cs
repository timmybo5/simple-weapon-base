using Sandbox;
using Sandbox.UI;

namespace SWB_Base
{
    partial class PlayerBase
    {
        private AttachmentEditorMenu attachmentEditor;

        public void ToggleAttachmentEditor()
        {
            Host.AssertServer();

            ToggleAttachmentEditorCL(To.Single(this));
        }

        public bool IsAttachmentEditing()
        {
            Host.AssertClient();

            return attachmentEditor != null;
        }

        [ClientRpc]
        public void ToggleAttachmentEditorCL()
        {
            Host.AssertClient();

            if (IsModelEditing())
            {
                ToggleModelEditorCL();
            }

            if (!IsAttachmentEditing())
            {
                Log.Info("Opened attachment editor!");
                attachmentEditor = new AttachmentEditorMenu();
                attachmentEditor.Parent = Local.Hud;
            }
            else
            {
                Log.Info("Closed attachment editor!");
                attachmentEditor.OnClose();
                attachmentEditor.Delete(true);
                attachmentEditor = null;
            }
        }
    }
}
