using Sandbox;
using Sandbox.UI;

namespace SWB_Base.UI
{
    public static class PanelUtils
    {
        public static void PositionAtCrosshair(this Panel panel, bool centered = true)
        {
            panel.PositionAtCrosshair(Local.Pawn, centered);
        }

        public static void PositionAtCrosshair(this Panel panel, Entity player, bool centered = true)
        {
            if (!player.IsValid()) return;

            var eyePos = player.EyePosition;
            var eyeRot = player.EyeRotation;

            var tr = Trace.Ray(eyePos, eyePos + eyeRot.Forward * 2000)
                            .Size(1.0f)
                            .Ignore(player)
                            .UseHitboxes()
                            .Run();

            panel.PositionAtWorld(tr.EndPosition, centered);
        }

        public static void PositionAtWorld(this Panel panel, Vector3 pos, bool centered = true)
        {
            var screenpos = pos.ToScreen();

            if (screenpos.z < 0)
                return;

            panel.Style.Left = Length.Fraction(screenpos.x);
            panel.Style.Top = Length.Fraction(screenpos.y);

            if (!centered)
            {
                panel.Style.MarginLeft = Screen.Width * panel.ScaleFromScreen * -0.5f;
                panel.Style.MarginTop = Screen.Height * panel.ScaleFromScreen * -0.5f;
            }

            panel.Style.Dirty();
        }
    }
}
