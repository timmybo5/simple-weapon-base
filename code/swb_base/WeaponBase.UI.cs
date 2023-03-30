using Sandbox;
using Sandbox.UI;
using SWB_Base.UI;

namespace SWB_Base;

public partial class WeaponBase
{
    protected Panel customizationMenu;
    protected Panel hitmarker;
    protected Panel crosshair;

    public override void CreateHudElements()
    {
        if (UISettings.HideAll) return;

        var showHUDCL = GetSetting<bool>("swb_cl_showhud", true);
        var showHUDSV = GetSetting<bool>("swb_sv_showhud", true);

        if (Game.RootPanel == null || !showHUDCL || !showHUDSV) return;

        if (UISettings.ShowCrosshair)
        {
            crosshair = CreateCrosshair();
            crosshair.Parent = Game.RootPanel;
        }

        if (UISettings.ShowHitmarker)
        {
            hitmarker = CreateHitmarker();
            hitmarker.Parent = Game.RootPanel;
        }
    }

    /// <summary>Override to use custom crosshair</summary>
    public virtual Panel CreateCrosshair()
    {
        return new Crosshair();
    }

    /// <summary>Override to use custom hitmarkers</summary>
    public virtual Panel CreateHitmarker()
    {
        return new Hitmarker();
    }

    public override void DestroyHudElements()
    {
        hitmarker?.Delete(true);
        crosshair?.Delete(true);
        customizationMenu?.Delete();
    }

    public virtual void UISimulate(IClient player)
    {
        // Customization menu
        if (EnableCustomizationSV > 0 && Input.Pressed(InputButton.Menu) && AttachmentCategories != null)
        {
            if (customizationMenu == null)
            {
                customizationMenu = new CustomizationMenu();
                customizationMenu.Parent = Game.RootPanel;
            }
            else
            {
                customizationMenu.Delete();
                customizationMenu = null;
            }
        }

        IsCustomizing = customizationMenu != null;
    }
}
