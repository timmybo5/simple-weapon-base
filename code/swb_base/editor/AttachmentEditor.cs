using Sandbox;
using SWB_Base.Editor;

namespace SWB_Base;

partial class PlayerBase
{
    private AttachmentEditorMenu attachmentEditor;

    public void ToggleAttachmentEditor()
    {
        Game.AssertServer();

        ToggleAttachmentEditorCL(To.Single(this));
    }

    public bool IsAttachmentEditing()
    {
        Game.AssertClient();

        return attachmentEditor != null;
    }

    public void CloseAttachmentEditor()
    {
        Game.AssertClient();

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
        Game.AssertClient();
        CloseModelEditor();

        if (!IsAttachmentEditing())
        {
            Log.Info("Opened attachment editor!");
            attachmentEditor = new AttachmentEditorMenu();
            attachmentEditor.Parent = Game.RootPanel;
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
