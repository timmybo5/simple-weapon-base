using System.Threading.Tasks;
using Sandbox.UI;

namespace SWB_Base.UI;


public class Hitmarker : Panel
{
    public static Hitmarker Current;
    private Marker activeMarker;

    public Hitmarker()
    {
        StyleSheet.Load("/swb_base/ui/Hitmarker.scss");
        Current = this;
    }

    public override void Tick()
    {
        base.Tick();
        this.PositionAtCrosshair();
    }

    public void Create(bool isKill)
    {
        if (activeMarker != null)
            activeMarker.Delete(true);

        activeMarker = new Marker(this, isKill);
    }

    public class Marker : Panel
    {
        public Marker(Panel parent, bool isKill)
        {
            Parent = parent;

            Panel leftTopBar = Add.Panel("leftTopBar");
            Panel lefBottomBar = Add.Panel("leftBottomBar");
            Panel rightTopBar = Add.Panel("rightTopBar");
            Panel rightBottomBar = Add.Panel("rightBottomBar");

            string sharedStyling = isKill ? "sharedBarStylingKill" : "sharedBarStyling";
            leftTopBar.AddClass(sharedStyling);
            lefBottomBar.AddClass(sharedStyling);
            rightTopBar.AddClass(sharedStyling);
            rightBottomBar.AddClass(sharedStyling);

            _ = Lifetime();
        }

        async Task Lifetime()
        {
            await Task.Delay(100);
            AddClass("fadeOut");
            await Task.Delay(300);
            Delete();
        }
    }
}
