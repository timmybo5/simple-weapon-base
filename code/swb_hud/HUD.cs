
using Sandbox;
using Sandbox.UI;

namespace SWB_HUD;

[Library]
public partial class HUD : HudEntity<RootPanel>
{
    KillFeed killFeed;

    public HUD()
    {
        if (!Game.IsClient)
            return;

        RootPanel.StyleSheet.Load("swb_hud/HUD.scss");

        RootPanel.AddChild<HealthDisplay>();
        RootPanel.AddChild<AmmoDisplay>();

        // Deatmatch
        RootPanel.AddChild<DamageIndicator>();

        RootPanel.AddChild<InventoryBar>();
        RootPanel.AddChild<PickupFeed>();

        RootPanel.AddChild<ChatBox>();
        killFeed = RootPanel.AddChild<KillFeed>();
        RootPanel.AddChild<Scoreboard>();
        RootPanel.AddChild<VoiceList>();
    }

    [ClientRpc]
    public void OnPlayerDied(string victim, string attacker = null)
    {
        Game.AssertClient();
    }

    [ClientRpc]
    public void ShowDeathScreen(string attackerName)
    {
        Game.AssertClient();
    }

    [ClientRpc]
    public void AddKillfeedEntry(long lsteamid, string left, long rsteamid, string right, string icon)
    {
        killFeed.AddEntry(lsteamid, left, rsteamid, right, icon);
    }
}
