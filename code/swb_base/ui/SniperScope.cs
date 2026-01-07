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

	float verticalMov;
	float horizontalMov;
	float lensBob;
	float lensRotation;
	bool wasInAir;

	public SniperScope( Weapon weapon, string lensTexture, string scopeTexture )
	{
		this.weapon = weapon;
		StyleSheet.Load( "/swb_base/ui/SniperScope.cs.scss" );
		AddClass( "hide" );

		if ( scopeTexture is not null )
			Add.Panel( "leftBar" );

		lensWrapper = Add.Panel( "lensWrapper" );
		lensWrapper.Add.Image( lensTexture, "lens" );

		if ( scopeTexture is not null )
		{
			scope = lensWrapper.Add.Image( scopeTexture, "scope" );

			Add.Panel( "rightBar" );
			Add.Panel( "topBar" );
			Add.Panel( "bottomBar" );
			Add.Panel( "leftBarOffscreen" );
			Add.Panel( "rightBarOffscreen" );
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
		var targetBob = 0f;

		if ( velocityJump != 0 )
			targetBob += velocityJump * 2;
		else if ( velocityMove != 0 )
			targetBob += MathF.Sin( RealTime.Now * 17f ) * velocityMove * 2;

		if ( wasInAir && player.IsOnGround )
			lensBob = 40;

		lensBob = MathUtil.FILerp( lensBob, targetBob, 10 );
		var fovModifier = (Preferences.FieldOfView - weapon.ScopeInfo.FOV) / 10;
		horizontalMov = MathUtil.FILerp( horizontalMov, Input.AnalogLook.yaw * fovModifier, 10 );
		verticalMov = MathUtil.FILerp( verticalMov, -Input.AnalogLook.pitch * fovModifier, 10 );

		Style.MarginTop = Length.Percent( lensBob + verticalMov );
		Style.MarginLeft = Length.Percent( horizontalMov );
		wasInAir = !player.IsOnGround;

		if ( scope is null ) return;

		// Rotation impact
		var rightVector = player.EyeAngles.ToRotation().Right * player.Velocity;
		var targetRotation = (rightVector.y + rightVector.x) * 0.015f;
		var rotateTransform = new PanelTransform();
		lensRotation = MathUtil.FILerp( lensRotation, targetRotation, 20 );
		rotateTransform.AddRotation( 0, 0, lensRotation );
		scope.Style.Transform = rotateTransform;

		// Movement blur (deactivated due to lag)
		// scope.Style.FilterBlur = Math.Abs( lensRotation * 2 + velocityJump + lensBob + (horizontalMov + verticalMov) * 3 );
	}

	[PanelEvent( "shoot" )]
	public void ShootEvent( float fireDelay )
	{
		var rndHorizontal = Game.Random.Float( -1.25f, 1.25f );
		var rndVertical = Game.Random.Float( -2.5f, -5 );
		horizontalMov += rndHorizontal * weapon.Primary.Recoil * 2f;
		verticalMov += rndVertical * weapon.Primary.Recoil * 2f;
	}
}
