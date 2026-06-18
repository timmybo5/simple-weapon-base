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

	const float IdleSwayHorizontalPercent = 0.5f;
	const float IdleSwayVerticalPercent = 0.5f;
	const float IdleSwayDelay = 1f;

	TimeSince timeSinceActive;
	Vector2 idleSway;
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
		var show = weapon.IsValid() && weapon.IsScoping;

		// Show when zooming
		SetClass( "hide", !show );
		if ( !show )
		{
			horizontalMov = MathUtil.FILerp( horizontalMov, 0, 3 );
			verticalMov = MathUtil.FILerp( verticalMov, 0, 3 );
			timeSinceActive = 0;
			return;
		}

		// Scope size
		var scopeSize = Screen.Height * ScaleFromScreen;
		lensWrapper.Style.Width = Length.Pixels( scopeSize );

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
		var deltaYaw = (Input.AnalogLook.yaw / Time.Delta) / 100;
		var deltaPitch = (-Input.AnalogLook.pitch / Time.Delta) / 50;
		horizontalMov = MathUtil.FILerp( horizontalMov, deltaYaw * fovModifier, 10 );
		verticalMov = MathUtil.FILerp( verticalMov, deltaPitch * fovModifier, 10 );

		var swayTime = Math.Max( 0f, (float)timeSinceActive - IdleSwayDelay );
		if ( swayTime > 0 )
			idleSway = MathUtil.FILerp( idleSway, GetIdleSway( swayTime ), 10 );
		else
			idleSway = Vector2.Zero;

		var marginTopPercent = lensBob + verticalMov + idleSway.y;
		var marginLeftPercent = horizontalMov + idleSway.x;

		var screenOffset = new Vector2(
			Screen.Width * (marginLeftPercent / 100f),
			Screen.Height * (marginTopPercent / 100f)
		);

		Style.MarginLeft = Length.Pixels( screenOffset.x * ScaleFromScreen );
		Style.MarginTop = Length.Pixels( screenOffset.y * ScaleFromScreen );
		weapon.SetScopeLensCenter( new( 0.5f + screenOffset.x / Screen.Width, 0.5f + screenOffset.y / Screen.Height ) );
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

	Vector2 GetIdleSway( float swayTime )
	{
		var x = GetIdleSwayX( swayTime ) - GetIdleSwayX( 0f );
		var y = GetIdleSwayY( swayTime ) - GetIdleSwayY( 0f );

		return new Vector2( x * IdleSwayHorizontalPercent, y * IdleSwayVerticalPercent );
	}

	float GetIdleSwayX( float time )
	{
		return MathF.Sin( time * 1.15f ) * 0.7f + MathF.Sin( time * 0.63f + 1.8f ) * 0.3f;
	}

	float GetIdleSwayY( float time )
	{
		return MathF.Sin( time * 0.95f + 0.85f ) * 0.65f + MathF.Sin( time * 0.47f + 2.4f ) * 0.35f;
	}
}
