
using Sandbox;
using Sandbox.UI;

namespace SWB_Base
{

	public class Crosshair : Panel
	{
		int fireCounter;

		public Crosshair()
		{
			StyleSheet.Load( "/swb_base/ui/Crosshair.scss" );

			for ( int i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
			}
		}

		public override void Tick()
		{
			base.Tick();
			this.PositionAtCrosshair();

			SetClass( "fire", fireCounter > 0 );

			if ( fireCounter > 0 )
				fireCounter--;

			// Hide when zooming in
			var player = Local.Pawn;
			if ( player == null ) return;

			var weapon = player.ActiveChild as WeaponBase;
			SetClass( "hideCrosshair", weapon != null ? weapon.IsZooming : true );
		}

		public override void OnEvent( string eventName )
		{
			if ( eventName == "fire" )
			{
				// this is a hack until we have animation or TriggerClass support
				fireCounter += 2;
			}

			base.OnEvent( eventName );
		}
	}
}
