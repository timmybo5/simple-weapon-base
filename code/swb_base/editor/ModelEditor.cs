using Sandbox;
using Sandbox.UI;
using SWB_Base.Editor;

namespace SWB_Base;

partial class PlayerBase
{
    private Panel modelEditor;

    public void ToggleModelEditor()
    {
        Game.AssertServer();

        ToggleModelEditorCL(To.Single(this));
    }

    public bool IsModelEditing()
    {
        Game.AssertClient();

        return modelEditor != null;
    }

    public void CloseModelEditor()
    {
        Game.AssertClient();

        if (modelEditor != null)
        {
            modelEditor.Delete();
            modelEditor = null;
        }
    }

    [ClientRpc]
    public void ToggleModelEditorCL()
    {
        Game.AssertClient();

        CloseAttachmentEditor();

        if (!IsModelEditing())
        {
            Log.Info("Opened model editor!");
            modelEditor = new ModelEditorMenu();
            modelEditor.Parent = Game.RootPanel;
        }
        else
        {
            Log.Info("Closed model editor!");
            modelEditor.Delete(true);
            modelEditor = null;
        }
    }
}
