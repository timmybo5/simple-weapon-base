
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace SWB_Base
{

    public class Crosshair : Panel
    {

        Panel CenterDot;
        Panel LeftBar;
        Panel RightBar;
        Panel TopBar;
        Panel BottomBar;

        private int spreadOffset = 400;
        private int sprintOffset = 100;
        private int fireOffset = 50;

        private bool wasZooming = false;

        public Crosshair()
        {
            StyleSheet.Load("/swb_base/ui/Crosshair.scss");

            CenterDot = Add.Panel("centerDot");
            LeftBar = Add.Panel("leftBar");
            RightBar = Add.Panel("rightBar");
            TopBar = Add.Panel("topBar");
            BottomBar = Add.Panel("bottomBar");
        }

        private void UpdateCrosshair()
        {
            CenterDot.Style.Dirty();
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

        private void RestoreCrosshairOpacity()
        {
            CenterDot.Style.Opacity = 1;
            LeftBar.Style.Opacity = 1;
            RightBar.Style.Opacity = 1;
            TopBar.Style.Opacity = 1;
            BottomBar.Style.Opacity = 1;
        }

        private void HideBarLines()
        {
            LeftBar.Style.Opacity = 0;
            RightBar.Style.Opacity = 0;
            TopBar.Style.Opacity = 0;
            BottomBar.Style.Opacity = 0;
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
                RightBar.Style.Left = sprintOffset - 5;
                TopBar.Style.Top = -sprintOffset;
                BottomBar.Style.Top = sprintOffset - 5;

                HideBarLines();
            }
            else if (weapon.IsZooming)
            {
                wasZooming = true;
                CenterDot.Style.Opacity = 0;
                HideBarLines();
            }
            else if (LeftBar.Style.Left == -sprintOffset || wasZooming)
            {
                wasZooming = false;
                RestoreBarPositions();
                RestoreCrosshairOpacity();
            }

            UpdateCrosshair();
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
