using SWB.Shared;

namespace SWB.Base;

public class PlayerCameraHandler : Component
{
	[Property] public float ThirdPersonAimZoom { get; set; } = 1.4f;

	public Weapon Weapon { get; set; }

	float targetPlayerFOV = -1;
	float finalPlayerFOV;
	float playerFOVSpeed = 1;
	float cachedFOV = -1f;
	float cachedVerticalFOV;

	IPlayerBase player => Weapon.Owner;

	protected override void OnStart()
	{
		if ( IsProxy )
			Enabled = false;
	}

	protected override void OnEnabled()
	{
		targetPlayerFOV = -1;
		playerFOVSpeed = 1;
	}

	protected override void OnDestroy()
	{
		if ( IsProxy || player is null ) return;
		player.FieldOfView = Screen.CreateVerticalFieldOfView( Preferences.FieldOfView );
	}

	protected override void OnUpdate()
	{
		if ( targetPlayerFOV == -1 )
		{
			targetPlayerFOV = Preferences.FieldOfView;
			finalPlayerFOV = Preferences.FieldOfView;
		}

		var decayRate = playerFOVSpeed * 10f;
		if ( decayRate > 0f )
		{
			var halfLife = System.MathF.Log( 2f ) / decayRate;
			finalPlayerFOV = MathX.ExponentialDecay( finalPlayerFOV, targetPlayerFOV, halfLife, RealTime.Delta );
		}

		if ( finalPlayerFOV != cachedFOV )
		{
			cachedFOV = finalPlayerFOV;
			cachedVerticalFOV = Screen.CreateVerticalFieldOfView( finalPlayerFOV );
		}
		player.FieldOfView = cachedVerticalFOV;

		// Initialize the target vectors for this frame
		targetPlayerFOV = Preferences.FieldOfView;

		HandleIronFOV();
	}

	void HandleIronFOV()
	{
		var isAiming = !Weapon.ShouldTuckVar && Weapon.IsAiming;
		if ( isAiming && !Weapon.IsReloading )
		{
			var aimZoom = Weapon.AimInfo.PlayerFOVZoom;

			if ( !player.IsFirstPerson && aimZoom <= 0 )
				aimZoom = ThirdPersonAimZoom;

			if ( aimZoom > 0 )
				targetPlayerFOV = CalculateZoomedFOV( Preferences.FieldOfView, aimZoom );

			if ( Weapon.IsScoping && Weapon.ScopeInfo.FOV > 0 )
				targetPlayerFOV = Weapon.ScopeInfo.FOV;

			playerFOVSpeed = Weapon.AimInfo.AimInFOVSpeed;
		}
		else
		{
			playerFOVSpeed = Weapon.AimInfo.AimOutFOVSpeed;
		}
	}

	/// <summary>
	/// Take the camera’s current FOV, convert it into a projected view size, divide that size by the zoom factor, then convert it back into a new FOV.
	/// </summary>
	static float CalculateZoomedFOV( float baseFOV, float zoom )
	{
		var halfFOV = MathX.DegreeToRadian( baseFOV ) * 0.5f;
		return MathX.RadianToDegree( 2f * System.MathF.Atan( System.MathF.Tan( halfFOV ) / zoom ) );
	}
}
