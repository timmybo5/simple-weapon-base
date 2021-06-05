using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

public partial class HitIndicator : Panel
{
	public static HitIndicator Current;

	public HitIndicator()
	{
		Current = this;
		StyleSheet.Load( "swb_base/deathmatch_dep/ui/scss/HitIndicator.scss" );
	}

	public override void Tick()
	{
		base.Tick();
	}

	public void OnHit( Vector3 pos, float amount )
	{
		new HitPoint( amount, pos, this );
	}

	public class HitPoint : Panel
	{
		public HitPoint( float amount, Vector3 pos, Panel parent )
		{
			Parent = parent;
			_ = Lifetime();
		}

		async Task Lifetime()
		{
			await Task.Delay( 200 );
			Delete();
		}
	}
}


