using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB_Player;

namespace SWB_HUD;

public class HealthDisplay : Panel
{
    Panel healthWrapper;

    Image healthIcon;
    Label healthLabel;

    public HealthDisplay()
    {
        StyleSheet.Load("/swb_hud/HealthDisplay.scss");

        healthWrapper = Add.Panel("healthWrapper");
        healthIcon = healthWrapper.Add.Image("/materials/swb/hud/health.png", "healthIcon");
        healthLabel = healthWrapper.Add.Label("", "healthLabel");
    }

    public override void Tick()
    {
        if (Game.LocalPawn is not PlayerBase player) return;

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
