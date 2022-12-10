using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SWB_Base.UI;

public class SniperScope : Panel
{
    Panel LensWrapper;
    Panel ScopeWrapper;

    Panel LeftBar;
    Panel RightBar;
    Panel TopBar;
    Panel BottomBar;

    Image Lens;
    Image Scope;

    float lensRotation;

    public SniperScope(string lensTexture, string scopeTexture)
    {
        StyleSheet.Load("/swb_base/ui/SniperScope.scss");

        if (scopeTexture != null)
            LeftBar = Add.Panel("leftBar");

        LensWrapper = Add.Panel("lensWrapper");
        Lens = LensWrapper.Add.Image(lensTexture, "lens");

        if (scopeTexture != null)
        {
            Scope = LensWrapper.Add.Image(scopeTexture, "scope");

            RightBar = Add.Panel("rightBar");
            //TopBar = Add.Panel("topBar");
            //BottomBar = Add.Panel("bottomBar");
        }
    }

    public override void Tick()
    {
        base.Tick();

        var player = Game.LocalPawn as PlayerBase;
        if (player == null) return;
        if (player.ActiveChild is not WeaponBase weapon) return;

        // Scope
        var scopeSize = Screen.Height * ScaleFromScreen;
        LensWrapper.Style.Width = Length.Pixels(scopeSize);
        LensWrapper.Style.Height = Length.Pixels(scopeSize);
        LensWrapper.Style.Dirty();

        // Show when zooming
        Style.Opacity = !weapon.IsScoped ? 0 : 1;

        /*
        // Movement impact
        var velocity = player.Velocity;
        var velocityMove = (velocity.y + velocity.x) / 2;
        var lensBob = 0f;

        if (velocityMove != 0)
        {
            lensBob += MathF.Sin(RealTime.Now * 20f) * 2f;
        }

        this.Style.MarginTop = Length.Percent((velocity.z * 0.05f) + lensBob);

        var targetRotation = 0f;

        if (Input.Left != 0)
        {
            targetRotation = Input.Left * -5f;
        }

        var rotateTransform = new PanelTransform();
        lensRotation = MathUtil.FILerp(lensRotation, targetRotation, 5);
        rotateTransform.AddRotation(0, 0, lensRotation);

        this.Style.Transform = rotateTransform;
        Style.Dirty();

        */
    }
}
