using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB_Base;

public class HealthDisplay : Panel
{
    Panel healthWrapper;

    Image healthIcon;
    Label healthLabel;

    public HealthDisplay(UISettings uiSettings)
    {
        StyleSheet.Load("/swb_base/ui/HealthDisplay.scss");

        healthWrapper = Add.Panel("healthWrapper");

        healthIcon = healthWrapper.Add.Image("/materials/swb/hud/health.png", "healthIcon");
        healthLabel = healthWrapper.Add.Label("", "healthLabel");
    }

    public override void Tick()
    {
        var player = Local.Pawn as PlayerBase;
        if (player == null) return;

        var isAlive = player.Alive();
        SetClass("hideHealthDisplay", !isAlive);

        if (!isAlive) return;

        var health = player.Health;
        var healthPer = health / 100f;

        healthLabel.SetText(health.ToString());
        healthLabel.Style.FontColor = new Color(1, 1 * healthPer, 1 * healthPer);
        healthIcon.Style.Opacity = 1; // healthPer
    }
}
