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

    /// <summary>EXPERIMENTAL - Use a render target instead of a full screen texture zoom</summary>
    public virtual bool UseRenderTarget => false;

    protected Panel SniperScopePanel;
    private bool switchBackToThirdP = false;
    private float oldSpread = -1;

    public override void ActiveStart(Entity ent)
    {
        base.ActiveStart(ent);
    }

    public override void ActiveEnd(Entity ent, bool dropped)
    {
        base.ActiveEnd(ent, dropped);

        SniperScopePanel?.Delete();
    }

    public virtual void OnScopedStart()
    {
        IsScoped = true;

        if (oldSpread == -1)
            oldSpread = Primary.Spread;

        Primary.Spread = 0;

        var player = Owner as ISWBPlayer;
        player.OnScopeStart();

        if (IsLocalPawn)
        {
            ViewModelEntity.RenderColor = Color.Transparent;

            foreach (var child in ViewModelEntity.Children)
            {
                (child as ModelEntity).RenderColor = Color.Transparent;
            }

            if (HandsModel != null)
            {
                HandsModel.RenderColor = Color.Transparent;
            }

            if (!string.IsNullOrEmpty(ZoomInSound))
                PlaySound(ZoomInSound);
        }
    }

    public virtual void OnScopedEnd()
    {
        IsScoped = false;
        Primary.Spread = oldSpread;

        var player = Owner as ISWBPlayer;
        player.OnScopeEnd();

        if (IsLocalPawn)
        {
            ViewModelEntity.RenderColor = Color.White;

            foreach (var child in ViewModelEntity.Children)
            {
                (child as ModelEntity).RenderColor = Color.White;
            }

            if (HandsModel != null)
            {
                HandsModel.RenderColor = Color.White;
            }

            if (!string.IsNullOrEmpty(ZoomOutSound))
                PlaySound(ZoomOutSound);
        }
    }

    public override void Simulate(IClient owner)
    {
        base.Simulate(owner);
        var shouldTuck = ShouldTuck();

        if (((Input.Pressed(InputButton.SecondaryAttack) && !IsReloading && !IsRunning) || (IsZooming && !IsScoped)) && !shouldTuck)
        {
            OnScopedStart();
        }

        if (Input.Released(InputButton.SecondaryAttack) || (IsScoped && (IsRunning || shouldTuck || IsReloading)))
        {
            OnScopedEnd();
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
