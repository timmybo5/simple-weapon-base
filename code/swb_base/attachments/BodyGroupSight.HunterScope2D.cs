
using Sandbox;
using Sandbox.UI;
using SWB_Base.UI;

namespace SWB_Base.Attachments;

public class HunterScope2D : BodyGroupSight
{
    public override string Name => "Hunter Scope x12";
    public override string Description => "A high magnification scope that is utilized for firing at long ranges.";
    public override string IconPath => "attachments/swb/sight/scope_hunter/ui/icon.png";
    public override string BodyGroup => "sight";
    public override int BodyGroupChoice { get; set; } = 2;

    public virtual string LensTexture => "/materials/swb/scopes/swb_lens_hunter.png";
    public virtual string ScopeTexture => "/materials/swb/scopes/swb_scope_hunter.png";
    public virtual string ZoomInSound => "swb_sniper.zoom_in";
    public virtual string ZoomOutSound => "";
    public virtual int ZoomInDelay => 200;

    Panel scopePanel;

    public override string[] Positives => new string[]
    {
        "x12 magnification",
        "100% accurate while scoped in"
    };

    public override void OnUnequip(WeaponBase weapon)
    {
        base.OnUnequip(weapon);
        OnZoomEnd(weapon);
    }

    public override void OnZoomStart(WeaponBase weapon)
    {
        WeaponBaseSniper.OnScopeStart(weapon, ZoomInSound, ZoomInDelay);
    }

    public override void OnZoomEnd(WeaponBase weapon)
    {
        WeaponBaseSniper.OnScopeEnd(weapon, ZoomOutSound);
    }

    public override void CreateHudElements()
    {
        if (Game.RootPanel == null) return;

        scopePanel = new SniperScope(LensTexture, ScopeTexture);
        scopePanel.Parent = Game.RootPanel;
    }

    public override void DestroyHudElements()
    {
        scopePanel?.Delete(true);
    }
}
