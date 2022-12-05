using Sandbox;
using SWB_Base.Editor;

namespace SWB_Base;

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

    public void CloseAttachmentEditor()
    {
        Host.AssertClient();

        if (attachmentEditor != null)
        {
            attachmentEditor.OnClose();
            attachmentEditor.Delete();
            attachmentEditor = null;
        }
    }

    [ClientRpc]
    public void ToggleAttachmentEditorCL()
    {
        Host.AssertClient();

        CloseModelEditor();

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
