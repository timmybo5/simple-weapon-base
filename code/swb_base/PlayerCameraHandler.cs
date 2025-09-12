using SWB.Shared;

namespace SWB.Base;

public class PlayerCameraHandler : Component
{
	public Weapon Weapon { get; set; }

	float targetPlayerFOV = -1;
	float finalPlayerFOV;
	float playerFOVSpeed = 1;

	IPlayerBase player => Weapon.Owner;

	protected override void OnStart()
	{
		if ( IsProxy )
			Enabled = false;
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

		var animSpeed = 10 * Weapon.AnimSpeed;
		finalPlayerFOV = MathX.LerpTo( finalPlayerFOV, targetPlayerFOV, playerFOVSpeed * animSpeed * RealTime.Delta );

		player.FieldOfView = Screen.CreateVerticalFieldOfView( finalPlayerFOV );

		// Initialize the target vectors for this frame
		targetPlayerFOV = Preferences.FieldOfView;

		HandleIronFOV();
	}

	void HandleIronFOV()
	{
		var isAiming = !Weapon.ShouldTuckVar && Weapon.IsAiming;
		if ( isAiming && !Weapon.IsReloading )
		{
			var aimFOV = Weapon.AimInfo.PlayerFOV;

			if ( !player.IsFirstPerson && aimFOV <= 0 )
				aimFOV = 70f;

			if ( aimFOV > 0 )
				targetPlayerFOV = aimFOV;

			if ( Weapon.IsScoping && Weapon.ScopeInfo.FOV > 0 )
				targetPlayerFOV = Weapon.ScopeInfo.FOV;

			playerFOVSpeed = Weapon.AimInfo.AimInFOVSpeed;
		}
		else
		{
			if ( finalPlayerFOV != Weapon.AimInfo.PlayerFOV )
			{
				playerFOVSpeed = Weapon.AimInfo.AimOutFOVSpeed;
			}
		}
	}
}
