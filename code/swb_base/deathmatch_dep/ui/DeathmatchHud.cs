
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

[Library]
public partial class DeathmatchHud : HudEntity<RootPanel>
{
    public DeathmatchHud()
    {
        if ( !IsClient )
            return;

        RootPanel.StyleSheet.Load( "swb_base/deathmatch_dep/ui/scss/DeathmatchHud.scss" );

        RootPanel.AddChild<Vitals>();
        RootPanel.AddChild<Ammo>();

        RootPanel.AddChild<NameTags>();
        RootPanel.AddChild<DamageIndicator>();
        RootPanel.AddChild<HitIndicator>();

        RootPanel.AddChild<InventoryBar>();
        RootPanel.AddChild<PickupFeed>();

        RootPanel.AddChild<ChatBox>();
        RootPanel.AddChild<KillFeed>();
        RootPanel.AddChild<Scoreboard>();
        RootPanel.AddChild<VoiceList>();
    }

    [ClientRpc]
    public void OnPlayerDied( string victim, string attacker = null )
    {
        Host.AssertClient();
    }

    [ClientRpc]
    public void ShowDeathScreen( string attackerName )
    {
        Host.AssertClient();
    }
}
