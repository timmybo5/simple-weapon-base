using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SWB_Base.UI;

public class HealthDisplay : Panel
{
    Panel healthWrapper;

    Image healthIcon;
    Label healthLabel;

    public HealthDisplay(UISettings uiSettings)
    {
        StyleSheet.Load("/swb_base/ui/HealthDisplay.scss");

        healthWrapper = Add.Panel("healthWrapper");

        if (uiSettings.ShowHealthIcon)
            healthIcon = healthWrapper.Add.Image("/materials/swb/hud/health.png", "healthIcon");

        if (uiSettings.ShowHealthCount)
            healthLabel = healthWrapper.Add.Label("", "healthLabel");
    }

    public override void Tick()
    {
        var player = Local.Pawn as PlayerBase;
        if (player == null) return;

        var isAlive = player.Alive();
        SetClass("hideHealthDisplay", !isAlive);

        if (!isAlive) return;

        var health = Math.Round(player.Health);
        var healthPer = ((float)health) / 100f;

        if (healthIcon != null)
            healthIcon.Style.Opacity = 1; // healthPer

        if (healthLabel != null)
        {
            healthLabel.SetText(health.ToString());
            healthLabel.Style.FontColor = new Color(1, 1 * healthPer, 1 * healthPer);
        }
    }
}
