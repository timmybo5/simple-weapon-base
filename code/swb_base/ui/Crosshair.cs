using Sandbox.UI;
using SWB.Shared;
using System.Threading.Tasks;

namespace SWB.Base.UI;

public class Crosshair : Panel
{
	IPlayerBase player => weapon.Owner;
	Weapon weapon;
	CrosshairSettings crosshairSettings => weapon.CrosshairSettings;

	Panel centerDot;
	Panel leftBar;
	Panel rightBar;
	Panel topBar;
	Panel bottomBar;

	int spreadOffset = 400;
	int sprintOffset = 100;
	int fireOffset = 50;

	bool wasAiming = false;

	public Crosshair( Weapon weapon )
	{
		this.weapon = weapon;
		StyleSheet.Load( "/swb_base/ui/Crosshair.cs.scss" );

		centerDot = Add.Panel( "centerDot" );
		leftBar = Add.Panel( "leftBar" );
		rightBar = Add.Panel( "rightBar" );
		topBar = Add.Panel( "topBar" );
		bottomBar = Add.Panel( "bottomBar" );

		leftBar.AddClass( "sharedBarStyling" );
		rightBar.AddClass( "sharedBarStyling" );
		topBar.AddClass( "sharedBarStyling" );
		bottomBar.AddClass( "sharedBarStyling" );
	}

	private void UpdateCrosshair()
	{
		centerDot.Style.Dirty();
		leftBar.Style.Dirty();
		rightBar.Style.Dirty();
		topBar.Style.Dirty();
		bottomBar.Style.Dirty();
	}

	private void RestoreBarPositions()
	{
		leftBar.Style.Left = -16;
		rightBar.Style.Left = 5;
		topBar.Style.Top = -16;
		bottomBar.Style.Top = 5;
	}

	private void RestoreCrosshairOpacity()
	{
		centerDot.Style.Opacity = 1;
		leftBar.Style.Opacity = 1;
		rightBar.Style.Opacity = 1;
		topBar.Style.Opacity = 1;
		bottomBar.Style.Opacity = 1;
	}

	private void HideBarLines()
	{
		leftBar.Style.Opacity = 0;
		rightBar.Style.Opacity = 0;
		topBar.Style.Opacity = 0;
		bottomBar.Style.Opacity = 0;
	}

	public override void Tick()
	{
		bool isValidWeapon = weapon is not null;
		var shouldHide = !isValidWeapon || weapon.IsScoping || weapon.IsCustomizing;

		SetClass( "hideCrosshair", shouldHide );

		centerDot.SetClass( "hideCrosshair", !crosshairSettings.ShowDot );
		leftBar.SetClass( "hideCrosshair", !crosshairSettings.ShowLeft );
		rightBar.SetClass( "hideCrosshair", !crosshairSettings.ShowRight );
		topBar.SetClass( "hideCrosshair", !crosshairSettings.ShowTop );
		bottomBar.SetClass( "hideCrosshair", !crosshairSettings.ShowBottom );

		if ( shouldHide ) return;

		// Crosshair spread offset
		var screenOffset = spreadOffset * weapon.GetRealSpread();
		leftBar.Style.MarginLeft = -screenOffset;
		rightBar.Style.MarginLeft = screenOffset;
		topBar.Style.MarginTop = -screenOffset;
		bottomBar.Style.MarginTop = screenOffset;

		// Sprint spread offsets
		if ( weapon.IsRunning || weapon.ShouldTuckVar || weapon.IsReloading || weapon.IsDeploying || (weapon.InBoltBack && !weapon.IsAiming) )
		{
			leftBar.Style.Left = -sprintOffset;
			rightBar.Style.Left = sprintOffset - 5;
			topBar.Style.Top = -sprintOffset;
			bottomBar.Style.Top = sprintOffset - 5;

			HideBarLines();
		}
		else if ( weapon.IsAiming )
		{
			wasAiming = true;

			if ( player.IsFirstPerson )
			{
				centerDot.Style.Opacity = 0;
				HideBarLines();
			}
		}
		else if ( leftBar.Style.Left == -sprintOffset || wasAiming )
		{
			wasAiming = false;
			RestoreBarPositions();
			RestoreCrosshairOpacity();
		}

		UpdateCrosshair();
	}

	[PanelEvent( "shoot" )]
	public void ShootEvent( float fireDelay )
	{
		// Fire spread offsets
		leftBar.Style.Left = -fireOffset;
		rightBar.Style.Left = fireOffset - 5;
		topBar.Style.Top = -fireOffset;
		bottomBar.Style.Top = fireOffset - 5;

		_ = FireDelay( fireDelay / 2 );
	}

	private async Task FireDelay( float delay )
	{
		await GameTask.DelaySeconds( delay );
		RestoreBarPositions();
		RestoreCrosshairOpacity();
	}
}
