using SWB.Shared;
using System;
using System.Collections.Generic;

namespace SWB.Base;

public partial class Weapon
{
	IPlayerBase aimAssistTarget;
	TimeSince timeSinceAimAssist;

	float assistSensitivityMod = 1;
	float assistSensitivityTarget = 1;
	float assistAreaWidth = 0;

	bool assistWaitingForLookInput;
	bool aimAssistDebug = false;

	public virtual void OnAimStart() { }

	public virtual void OnAimStop()
	{
		if ( !Settings.AimAssist || !Input.UsingController ) return;
		assistSensitivityTarget = 1;
	}

	public virtual void OnAimAssistUpdate()
	{
		if ( !Settings.AimAssist || !Input.UsingController ) return;

		// Slow down sens
		var speed = IsAiming ? 2f : 4f;
		assistSensitivityMod = MathUtil.FILerp( assistSensitivityMod, assistSensitivityTarget, speed );
		Owner.InputSensitivity /= assistSensitivityMod;

		// Debug
		if ( aimAssistDebug )
			GetAimAssistTarget();
	}

	public virtual void OnAimUpdate()
	{
		if ( !Settings.AimAssist || !Input.UsingController ) return;

		// Stop assist when target dies (prevent snapping to other players)
		if ( aimAssistTarget is not null && !aimAssistTarget.IsAlive )
		{
			aimAssistTarget = null;
			assistWaitingForLookInput = true;
			Input.Suppressed = true;
		}

		// Only work when actively looking
		if ( assistWaitingForLookInput )
		{
			var lookMagnitude = MathF.Abs( Input.AnalogLook.pitch ) + MathF.Abs( Input.AnalogLook.yaw );
			if ( lookMagnitude > 0.05f )
				assistWaitingForLookInput = false;
			else
			{
				assistSensitivityTarget = 1;
				return;
			}
		}

		if ( timeSinceAimAssist > 0.1f )
		{
			timeSinceAimAssist = 0;
			aimAssistTarget = GetAimAssistTarget();
		}

		if ( aimAssistTarget is null || !aimAssistTarget.IsValid() )
		{
			assistSensitivityTarget = 1;
			return;
		}

		// Sens lerp
		var lowestSens = IsScoping ? 1 : 0.1f;
		assistSensitivityTarget = Math.Clamp( 200f / assistAreaWidth, lowestSens, 6f );

		// Aim assist dead zone
		var sp = Owner.Camera.PointToScreenPixels( aimAssistTarget.EyePos );
		var center = new Vector2( Screen.Width / 2, Screen.Height / 2 );
		var delta2 = sp - center;
		var dist = delta2.Length;
		if ( dist < 10f ) return;

		// Update angles
		var from = Owner.Camera.WorldPosition;
		var to = aimAssistTarget.EyePos + Vector3.Down * 20;

		var dir = (to - from).Normal;
		var desiredAngles = dir.EulerAngles;
		var delta = (desiredAngles - Owner.Camera.WorldRotation.Angles()).Normal;

		// Scale by time and strength (frame-rate independent)
		var strength = 6f;
		var assistOffset = delta * (Time.Delta * strength);

		Owner.ApplyEyeAnglesOffset( assistOffset );
	}

	public virtual bool CanAimAssistOnTarget( IPlayerBase target )
	{
		return true;
	}

	// Single trace, no allocations. For now we only care if the eyepos is visible.
	public bool HasLOS( IPlayerBase target )
	{
		var from = Owner.Camera.WorldPosition;
		var tr = TraceBullet( Owner.GameObject, from, target.EyePos, 1f );

		// DebugOverlay.Line( from, target.EyePos, Color.Red, 1 );
		return tr.GameObject == target.GameObject || tr.GameObject?.Parent == target.GameObject;
	}

	public float ScaleRadiusByFov( float baseRadius, float baseFov, float currentFov )
	{
		var scaled = baseRadius * (baseFov / currentFov);
		// Log.Info( $"radius: {baseRadius}, fov: {baseFov}, curr fov: {currentFov}, scaled: {scaled}" );
		return scaled;
	}

	public IPlayerBase GetAimAssistTarget()
	{
		// Player registry instead of walking the whole scene
		var players = SWB.Player.PlayerBase.All;
		var targetScore = 999999f;
		var center = new Vector2( Screen.Width / 2, Screen.Height / 2 );
		IPlayerBase target = null;

		foreach ( var ply in players )
		{
			if ( ply == Owner || !ply.IsAlive || !CanAimAssistOnTarget( ply ) ) continue;

			var pos = ply.GameObject.WorldPosition + Vector3.Up * 32;
			var posToScreen = Owner.Camera.PointToScreenPixels( pos, out bool isBehind );
			if ( isBehind ) continue;

			var size = !IsScoping ? 110f : 110f;
			var fov = Preferences.FieldOfView;

			if ( IsScoping && ScopeInfo.FOV != -1 )
				fov = ScopeInfo.FOV;

			size = ScaleRadiusByFov( 110f, Preferences.FieldOfView, fov );
			var threshold = 30000;
			var dist = ply.GameObject.WorldPosition.DistanceSquared( Owner.GameObject.WorldPosition );
			var maxDist = Math.Clamp( dist, 1, threshold );
			var width = Math.Max( 20, size * (35000 / (maxDist + (dist * 0.05f))) );
			var height = width * 1.5f;
			var zone = new Vector4( center.x - width, center.x + width, center.y - height, center.y + height );
			var inZone = false;
			var score = 0f;

			if ( posToScreen.x > zone.x && posToScreen.x < zone.y && posToScreen.y > zone.z && posToScreen.y < zone.w )
			{
				var aimDist = (posToScreen - center).Length;
				score = aimDist + (dist / 1000);

				if ( score < targetScore && HasLOS( ply ) )
				{
					inZone = true;
					target = ply;
					targetScore = score;

					// Share for sens calculation
					assistAreaWidth = width;
				}
			}

			// Debug
			if ( aimAssistDebug )
			{
				var color = inZone ? Color.Green : Color.Red;
				using var painter = (Owner.IsFirstPerson ? Owner.ViewModelCamera : Owner.Camera).BeginHud();
				var debugRect = new Rect()
				{
					Position = new( posToScreen.x - width, posToScreen.y - height ),
					Size = new( width * 2, height * 2 )
				};
				var debugTextPos = debugRect.Position.WithX( debugRect.Position.x + 4 ).WithY( debugRect.Position.y + 4 );
				var debugText =
				$"Width: {Math.Round( width )}\n" +
				$"Height: {Math.Round( height )}\n" +
				$"Dist: {Math.Round( dist )}\n" +
				$"Score: {Math.Round( score )}";

				painter.Fill = color.WithAlpha( 0.35f );
				painter.Rect( debugRect, 0 );
				painter.TextStyle = TextStyle.Default.WithSize( 12 ).WithColor( Color.Blue );
				painter.Text( debugText, new Rect( debugTextPos, new Vector2( 300, 100 ) ) );
			}
		}

		return target;
	}
}
