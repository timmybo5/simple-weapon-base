
using Sandbox;
using Sandbox.UI;

public partial class KillFeed : Sandbox.UI.KillFeed
{
	public KillFeed()
	{
		StyleSheet.Load( "swb_base/deathmatch_dep/ui/scss/KillFeed.scss" );
	}

	public override Panel AddEntry( ulong lsteamid, string left, ulong rsteamid, string right, string method )
	{
		Log.Info( $"{left} killed {right} using {method}" );

		var e = Current.AddChild<KillFeedEntry>();

		e.AddClass( method );

		e.Left.Text = left;
		e.Left.SetClass( "me", lsteamid == (Local.SteamId) );

		e.Right.Text = right;
		e.Right.SetClass( "me", rsteamid == (Local.SteamId) );

		return e;
	}
}
