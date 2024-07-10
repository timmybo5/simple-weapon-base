using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Shared;
using System;

namespace SWB.Base.UI;

public class SniperScope : Panel
{
	IPlayerBase player => weapon.Owner;
	Weapon weapon;

	Panel lensWrapper;
	Panel scope;

	float lensRotation;

	public SniperScope( Weapon weapon, string lensTexture, string scopeTexture )
	{
		this.weapon = weapon;
		StyleSheet.Load( "/swb_base/ui/SniperScope.cs.scss" );
		AddClass( "hide" );

		if ( scopeTexture != null )
			Add.Panel( "leftBar" );

		lensWrapper = Add.Panel( "lensWrapper" );
		lensWrapper.Add.Image( lensTexture, "lens" );

		if ( scopeTexture != null )
		{
			scope = lensWrapper.Add.Image( scopeTexture, "scope" );

			Add.Panel( "rightBar" );
			Add.Panel( "topBar" );
			Add.Panel( "bottomBar" );
		}
	}

	public override void Tick()
	{
		if ( weapon is null ) return;

		// Scope size
		var scopeSize = Screen.Height * ScaleFromScreen;
		lensWrapper.Style.Width = Length.Pixels( scopeSize );

		// Show when zooming
		SetClass( "hide", !weapon.IsScoping );

		// Check if ADS & firing
		if ( weapon.IsAiming && weapon.TimeSincePrimaryShoot < 0.1f )
			return;

		// Movement impact
		var velocityJump = player.Velocity.z * 0.02f;
		var velocityMove = (Math.Abs( player.Velocity.y ) + Math.Abs( player.Velocity.x )) * 0.005f;
		var lensBob = 0f;

		if ( velocityJump != 0 )
		{
			lensBob += velocityJump;
		}
		else if ( velocityMove != 0 )
		{
			lensBob += MathF.Sin( RealTime.Now * 17f ) * velocityMove;
		}

		Style.MarginTop = Length.Percent( velocityJump + lensBob );

		if ( scope == null ) return;

		// Rotation impact
		var rightVector = player.EyeAngles.ToRotation().Right * player.Velocity;
		var targetRotation = (rightVector.y + rightVector.x) * 0.015f;
		var rotateTransform = new PanelTransform();
		lensRotation = MathUtil.FILerp( lensRotation, targetRotation, 20 );
		rotateTransform.AddRotation( 0, 0, lensRotation );
		scope.Style.Transform = rotateTransform;

		// Movement blur
		scope.Style.FilterBlur = Math.Abs( lensRotation * 2 + velocityJump + lensBob );
	}
}
