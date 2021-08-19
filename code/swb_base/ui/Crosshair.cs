
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace SWB_Base
{

    public class Crosshair : Panel
    {

        Panel LeftBar;
        Panel RightBar;
        Panel TopBar;
        Panel BottomBar;

        private int spreadOffset = 400;
        private int sprintOffset = 100;
        private int fireOffset = 50;

        public Crosshair()
        {
            StyleSheet.Load("/swb_base/ui/Crosshair.scss");

            Add.Panel("centerDot");
            LeftBar = Add.Panel("leftBar");
            RightBar = Add.Panel("rightBar");
            TopBar = Add.Panel("topBar");
            BottomBar = Add.Panel("bottomBar");
        }

        private void UpdateBars()
        {
            LeftBar.Style.Dirty();
            RightBar.Style.Dirty();
            TopBar.Style.Dirty();
            BottomBar.Style.Dirty();
        }

        private void RestoreBarPositions()
        {
            LeftBar.Style.Left = -16;
            RightBar.Style.Left = 5;
            TopBar.Style.Top = -16;
            BottomBar.Style.Top = 5;
        }

        public override void Tick()
        {
            base.Tick();
            this.PositionAtCrosshair();

            // Hide when zooming in
            var player = Local.Pawn;
            if (player == null) return;

            var weapon = player.ActiveChild as WeaponBase;
            var shouldTuck = weapon.ShouldTuck();

            SetClass("hideCrosshair", weapon != null ? weapon.IsZooming && !shouldTuck : true);

            var hideCrosshairLines = weapon != null ? !weapon.DrawCrosshairLines : true;
            LeftBar.SetClass("hideCrosshair", hideCrosshairLines);
            RightBar.SetClass("hideCrosshair", hideCrosshairLines);
            TopBar.SetClass("hideCrosshair", hideCrosshairLines);
            BottomBar.SetClass("hideCrosshair", hideCrosshairLines);

            if (hideCrosshairLines) return;

            // Crosshair spread offset
            var screenOffset = spreadOffset * weapon.GetRealSpread();
            LeftBar.Style.MarginLeft = -screenOffset;
            RightBar.Style.MarginLeft = screenOffset;
            TopBar.Style.MarginTop = -screenOffset;
            BottomBar.Style.MarginTop = screenOffset;

            // Sprint spread offsets
            if (weapon.IsRunning || shouldTuck)
            {
                LeftBar.Style.Left = -sprintOffset;
                LeftBar.Style.Opacity = 0;
                RightBar.Style.Left = sprintOffset - 5;
                RightBar.Style.Opacity = 0;
                TopBar.Style.Top = -sprintOffset;
                TopBar.Style.Opacity = 0;
                BottomBar.Style.Top = sprintOffset - 5;
                BottomBar.Style.Opacity = 0;
            }
            else if (LeftBar.Style.Left == -sprintOffset)
            {
                RestoreBarPositions();
                LeftBar.Style.Opacity = 1;
                RightBar.Style.Opacity = 1;
                TopBar.Style.Opacity = 1;
                BottomBar.Style.Opacity = 1;
            }

            UpdateBars();
        }

        [PanelEvent]
        public void FireEvent(float fireDelay)
        {
            // Fire spread offsets
            LeftBar.Style.Left = -fireOffset;
            RightBar.Style.Left = fireOffset - 5;
            TopBar.Style.Top = -fireOffset;
            BottomBar.Style.Top = fireOffset - 5;

            _ = FireDelay(fireDelay);
        }

        private async Task FireDelay(float delay)
        {
            await GameTask.DelaySeconds(delay);
            RestoreBarPositions();
        }
    }
}
