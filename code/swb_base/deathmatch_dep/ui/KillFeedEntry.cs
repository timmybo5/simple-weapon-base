
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

public partial class KillFeedEntry : Panel
{
	public Label Left { get; internal set; }
	public Label Right { get; internal set; }
	public Panel Icon { get; internal set; }

	public KillFeedEntry()
	{
		Left = Add.Label( "", "left" );
		Icon = Add.Panel( "icon" );
		Right = Add.Label( "", "right" );

		_ = RunAsync();
	}

	async Task RunAsync()
	{
		await Task.Delay( 4000 );
		Delete();
	}

}
