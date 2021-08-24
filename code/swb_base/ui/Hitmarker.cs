using System.Threading.Tasks;
using Sandbox.UI;

namespace SWB_Base
{

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
                activeMarker.Delete();

            activeMarker = new Marker(this, isKill);
        }

        public class Marker : Panel
        {
            public Marker(Panel parent, bool isKill)
            {
                Parent = parent;

                Panel LeftTopBar = Add.Panel("leftTopBar");
                Panel LefBottomBar = Add.Panel("leftBottomBar");
                Panel RightTopBar = Add.Panel("rightTopBar");
                Panel RightBottomBar = Add.Panel("rightBottomBar");

                string sharedStyling = isKill ? "sharedBarStylingKill" : "sharedBarStyling";
                LeftTopBar.AddClass(sharedStyling);
                LefBottomBar.AddClass(sharedStyling);
                RightTopBar.AddClass(sharedStyling);
                RightBottomBar.AddClass(sharedStyling);

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
}
