using Sandbox;
using Sandbox.UI;

namespace SWB_Base.Editor;

/// <summary>
/// A button that opens a popup panel.
/// Useless on its own - you need to implement Open
/// </summary>
[Library("swb_PopupButton")]
public abstract class PopupButton : Button
{
    protected Popup Popup;

    public PopupButton()
    {
        AddClass("popupbutton");
    }

    protected override void OnClick(MousePanelEvent e)
    {
        base.OnClick(e);

        Open();
    }

    public abstract void Open();

    public override void Tick()
    {
        base.Tick();

        SetClass("open", Popup != null && !Popup.IsDeleting);
        SetClass("active", Popup != null && !Popup.IsDeleting);

        if (Popup != null)
        {
            Popup.Style.Width = Box.Rect.Width;
        }
    }
}
