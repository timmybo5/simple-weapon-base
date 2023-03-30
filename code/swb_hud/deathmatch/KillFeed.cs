using Sandbox;
using Sandbox.UI;

namespace SWB_HUD;

public partial class KillFeed : Panel
{
    public static KillFeed Current;

    public KillFeed()
    {
        Current = this;
        StyleSheet.Load("swb_hud/deathmatch/KillFeed.scss");
    }

    public virtual Panel AddEntry(long lsteamid, string left, long rsteamid, string right, string icon)
    {
        var e = Current.AddChild<KillFeedEntry>();

        e.Left.Text = left;
        e.Left.SetClass("me", lsteamid == Game.LocalClient.SteamId);
        e.Icon.Style.BackgroundImage = Texture.Load(icon);
        e.Right.Text = right;
        e.Right.SetClass("me", rsteamid == Game.LocalClient.SteamId);

        return e;
    }
}
