using Sandbox;
using Sandbox.UI;
using SWB_Base.Editor;

namespace SWB_Base;

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

    public void CloseModelEditor()
    {
        Host.AssertClient();

        if (modelEditor != null)
        {
            modelEditor.Delete();
            modelEditor = null;
        }
    }

    [ClientRpc]
    public void ToggleModelEditorCL()
    {
        Host.AssertClient();

        CloseAttachmentEditor();

        if (!IsModelEditing())
        {
            Log.Info("Opened model editor!");
            modelEditor = new ModelEditorMenu();
            modelEditor.Parent = Local.Hud;
        }
        else
        {
            Log.Info("Closed model editor!");
            modelEditor.Delete(true);
            modelEditor = null;
        }
    }
}
