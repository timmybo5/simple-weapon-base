using Sandbox;
using Sandbox.UI;
using SWB_Base.UI;

/* 
 * Weapon base for sniper based zooming
*/

namespace SWB_Base;

public partial class WeaponBaseSniper : WeaponBase
{
    /// <summary>Path to the lens texture</summary>
    public virtual string LensTexture => "/materials/swb/scopes/swb_lens_hunter.png";

    /// <summary>Path to the scope texture</summary>
    public virtual string ScopeTexture => "/materials/swb/scopes/swb_scope_hunter.png";

    /// <summary>Sound to play when zooming in</summary>
    public virtual string ZoomInSound => "swb_sniper.zoom_in";

    /// <summary>Sound to play when zooming out</summary>
    public virtual string ZoomOutSound => "";

    /// <summary>Delay between zooming and scoping in ms</summary>
    public virtual int ZoomInDelay => 200;

    /// <summary>EXPERIMENTAL - Use a render target instead of a full screen texture zoom</summary>
    public virtual bool UseRenderTarget => false;

    protected Panel SniperScopePanel;

    public override void ActiveStart(Entity ent)
    {
        base.ActiveStart(ent);
    }

    public override void ActiveEnd(Entity ent, bool dropped)
    {
        base.ActiveEnd(ent, dropped);

        SniperScopePanel?.Delete();
    }

    public override void OnZoomStart()
    {
        OnScopeStart(this, ZoomInSound, ZoomInDelay);
    }

    public override void OnZoomEnd()
    {
        OnScopeEnd(this, ZoomOutSound);
    }

    public static async void OnScopeStart(WeaponBase weapon, string zoomInSound, int zoomInDelay)
    {
        await GameTask.DelayRealtime(zoomInDelay);
        if (!weapon.IsZooming) return;

        weapon.IsScoped = true;

        var player = weapon.Owner as ISWBPlayer;
        player.OnScopeStart();

        if (weapon.IsLocalPawn)
        {
            weapon.ViewModelEntity.RenderColor = Color.Transparent;

            foreach (var child in weapon.ViewModelEntity.Children)
            {
                (child as ModelEntity).RenderColor = Color.Transparent;
            }

            if (weapon.HandsModel != null)
                weapon.HandsModel.RenderColor = Color.Transparent;

            if (!string.IsNullOrEmpty(zoomInSound))
                weapon.PlaySound(zoomInSound);
        }
    }

    public static void OnScopeEnd(WeaponBase weapon, string zoomOutSound)
    {
        if (!weapon.IsScoped) return;

        weapon.IsScoped = false;

        var player = weapon.Owner as ISWBPlayer;
        player.OnScopeEnd();

        if (weapon.IsLocalPawn)
        {
            weapon.ViewModelEntity.RenderColor = Color.White;

            foreach (var child in weapon.ViewModelEntity.Children)
            {
                (child as ModelEntity).RenderColor = Color.White;
            }

            if (weapon.HandsModel != null)
                weapon.HandsModel.RenderColor = Color.White;

            if (!string.IsNullOrEmpty(zoomOutSound))
                weapon.PlaySound(zoomOutSound);
        }
    }

    public override void CreateHudElements()
    {
        base.CreateHudElements();

        if (Game.RootPanel == null) return;

        if (UseRenderTarget)
        {
            //SniperScopePanel = new RenderScopeRT();
            //SniperScopePanel.Parent = Local.Hud;
        }
        else
        {
            SniperScopePanel = new SniperScope(LensTexture, ScopeTexture);
            SniperScopePanel.Parent = Game.RootPanel;
        }
    }
}
