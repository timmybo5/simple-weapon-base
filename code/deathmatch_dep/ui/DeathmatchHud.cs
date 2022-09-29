
using Sandbox;
using Sandbox.UI;

[Library]
public partial class DeathmatchHud : HudEntity<RootPanel>
{
    KillFeed killFeed;

    public DeathmatchHud()
    {
        if (!IsClient)
            return;

        RootPanel.StyleSheet.Load("deathmatch_dep/ui/DeathmatchHud.scss");

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
        Host.AssertClient();
    }

    [ClientRpc]
    public void ShowDeathScreen(string attackerName)
    {
        Host.AssertClient();
    }

    [ClientRpc]
    public void AddKillfeedEntry(long lsteamid, string left, long rsteamid, string right, string icon)
    {
        killFeed.AddEntry(lsteamid, left, rsteamid, right, icon);
    }
}
