using Sandbox;
using Sandbox.UI;

public partial class KillFeed : Panel
{
    public static KillFeed Current;

    public KillFeed()
    {
        Current = this;
        StyleSheet.Load("deathmatch_dep/ui/KillFeed.scss");
    }

    public virtual Panel AddEntry(long lsteamid, string left, long rsteamid, string right, string icon)
    {
        var e = Current.AddChild<KillFeedEntry>();

        e.Left.Text = left;
        e.Left.SetClass("me", lsteamid == Local.PlayerId);
        e.Icon.Style.BackgroundImage = Texture.Load(icon);
        e.Right.Text = right;
        e.Right.SetClass("me", rsteamid == Local.PlayerId);

        return e;
    }
}
