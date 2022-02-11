
using Sandbox;
using Sandbox.UI;
using SWB_Base;
using System;

public partial class KillFeed : Sandbox.UI.KillFeed
{
    public KillFeed()
    {
        StyleSheet.Load("deathmatch_dep/ui/KillFeed.scss");
    }

    public override Panel AddEntry(long lsteamid, string left, long rsteamid, string right, string method)
    {
        Log.Info($"{left} killed {right} using {method}");

        var e = Current.AddChild<KillFeedEntry>();

        e.AddClass(method);

        e.Left.Text = left;
        e.Left.SetClass("me", lsteamid == Local.PlayerId);

        try
        {
            // Temp solution ( get reference to kill weapon icon )
            if (method.StartsWith("swb_"))
            {
                var killWeapon = Library.Create<WeaponBase>(method); // throws error when not found

                if (!string.IsNullOrEmpty(killWeapon.Icon))
                {
                    e.Icon.Style.BackgroundImage = Texture.Load(killWeapon.Icon);
                    killWeapon.Delete();
                }
            }
        }
        catch (Exception exception) { }

        e.Right.Text = right;
        e.Right.SetClass("me", rsteamid == Local.PlayerId);

        return e;
    }
}
