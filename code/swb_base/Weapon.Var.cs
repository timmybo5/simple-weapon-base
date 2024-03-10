using Sandbox.Citizen;

namespace SWB.Base;

public partial class Weapon
{
	/// <summary>Firstperson Model</summary>
	[Property, Group( "Models" )] public Model ViewModel { get; set; }

	/// <summary>Firstperson Hands Model</summary>
	[Property, Group( "Models" )] public Model ViewModelHands { get; set; }

	/// <summary>Thirdperson Model</summary>
	[Property, Group( "Models" )] public Model WorldModel { get; set; }


	/// <summary>Unique name that identifies the weapon</summary>
	[Property, Group( "General" )] public string ClassName { get; set; }

	/// <summary>Name that represent the weapon on the HUD</summary>
	[Property, Group( "General" )] public string DisplayName { get; set; }

	/// <summary>Image that represent the weapon on the HUD</summary>
	[Property, Group( "General" )] public Texture Icon { get; set; }

	/// <summary>How the player holds the weapon in thirdperson</summary>
	[Property, Group( "General" )] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.Pistol;

	/// <summary>Aim sensitivity while zooming (lower is slower, 0 to disable)</summary>
	[Property, Group( "General" )] public float AimSensitivity { get; set; } = 0.85f;

	/// <summary>Can bullets be cocked in the barrel? (clip ammo + 1)</summary>
	[Property, Group( "General" )] public bool BulletCocking { get; set; } = true;


	/// <summary>Default weapon field of view</summary>
	[Property, Group( "FOV" )] public float FOV { get; set; } = 70;

	/// <summary>Weapon FOV while aiming (-1 to use default weapon fov)</summary>
	[Property, Group( "FOV" )] public float AimFOV { get; set; } = -1;

	/// <summary>Player FOV while aiming (-1 to use default player fov)</summary>
	[Property, Group( "FOV" )] public int AimPlayerFOV { get; set; } = -1;

	/// <summary>FOV aim in speed</summary>
	[Property, Group( "FOV" ), Title( "Aim in FOV speed" )] public float AimInFOVSpeed { get; set; } = 1;

	/// <summary>FOV aim out speed</summary>
	[Property, Group( "FOV" ), Title( "Aim out FOV speed" )] public float AimOutFOVSpeed { get; set; } = 1;


	/// <summary>Procedural animation speed (lower is slower)</summary>
	[Property, Group( "Animations" )] public float AnimSpeed { get; set; } = 1;

	/// <summary>Data used for setting the weapon to its aim position</summary>
	[Property, Group( "Animations" )] public AngPos AimAnimData { get; set; }

	/// <summary>Data used for setting the weapon to its run position</summary>
	[Property, Group( "Animations" )] public AngPos RunAnimData { get; set; }


	/// <summary>Duration of the reload animation</summary>
	[Property, Group( "Animations" )] public float ReloadTime { get; set; } = 1f;

	/// <summary>Reloading animation</summary>
	[Property, Group( "Animations" )] public string ReloadAnim { get; set; } = "reload";

	/// <summary>Duration of the empty reload animation (-1 to disable)</summary>
	[Property, Group( "Animations" )] public float ReloadEmptyTime { get; set; } = -1f;

	/// <summary>Reloading animation when clip is empty</summary>
	[Property, Group( "Animations" )] public string ReloadEmptyAnim { get; set; } = "reload_empty";

	/// <summary>Duration of the draw animation</summary>
	[Property, Group( "Animations" )] public float DrawTime { get; set; } = 0.5f;

	/// <summary>Draw animation</summary>
	[Property, Group( "Animations" )] public string DrawAnim { get; set; } = "deploy";

	/// <summary>Duration of the empty draw animation (-1 to disable)</summary>
	[Property, Group( "Animations" )] public float DrawEmptyTime { get; set; } = -1f;

	/// <summary>Draw animation when there is no ammo</summary>
	[Property, Group( "Animations" )] public string DrawEmptyAnim { get; set; } = "";

	/// <summary>Primary attack data</summary>
	[Property, Group( "Firing" ), Title( "Primary ShootInfo (component)" )] public ShootInfo Primary { get; set; } = new();

	/// <summary>Secondary attack data (setting this will disable weapon zooming)</summary>
	[Property, Group( "Firing" ), Title( "Secondary ShootInfo (component)" )] public ShootInfo Secondary { get; set; }


	/// <summary>Time since the last primary attack</summary>
	public TimeSince TimeSincePrimaryShoot { get; set; }

	/// <summary>Time since the last secondary attack</summary>
	public TimeSince TimeSinceSecondaryShoot { get; set; }

	/// <summary>Time since the last reload</summary>
	public TimeSince TimeSinceReload { get; set; }

	public bool IsCustomizing { get; set; }

	/// <summary>If the player is running</summary>
	public bool IsRunning => Owner.IsRunning;

	/// <summary>If the player is crouching</summary>
	public bool IsCrouching => Owner.IsCrouching;

	/// <summary>Is the view model visible</summary>
	public bool CanSeeViewModel => !IsProxy && Owner.IsFirstPerson;

	/// <summary>If the weapon is being reloaded</summary>
	[Sync] public bool IsReloading { get; set; }

	/// <summary>If the weapon is being aimed</summary>
	[Sync] public bool IsAiming { get; set; }
}
