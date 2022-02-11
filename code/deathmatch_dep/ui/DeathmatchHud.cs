
using Sandbox;
using Sandbox.UI;

[Library]
public partial class DeathmatchHud : HudEntity<RootPanel>
{
    public DeathmatchHud()
    {
        if (!IsClient)
            return;

        RootPanel.StyleSheet.Load("deathmatch_dep/ui/DeathmatchHud.scss");

        RootPanel.AddChild<NameTags>();
        RootPanel.AddChild<DamageIndicator>();

        RootPanel.AddChild<InventoryBar>();
        RootPanel.AddChild<PickupFeed>();

        RootPanel.AddChild<ChatBox>();
        RootPanel.AddChild<KillFeed>();
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
}
