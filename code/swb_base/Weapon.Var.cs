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

	[Property, Group( "General" )] public string DisplayName { get; set; }

	[Property, Group( "General" ), ImageAssetPathAttribute] public string Icon { get; set; }

	/// <summary>How the player holds the weapon in thirdperson</summary>
	[Property, Group( "General" )] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.Pistol;

	/// <summary>Mouse sensitivity while aiming (lower is slower, 0 to disable)</summary>
	[Property, Group( "General" )] public float AimSensitivity { get; set; } = 0.85f;

	/// <summary>Can bullets be cocked in the barrel? (clip ammo + 1)</summary>
	[Property, Group( "General" )] public bool BulletCocking { get; set; } = true;

	/// <summary>Range that tucking should be enabled (-1 to disable tucking)</summary>
	[Property, Group( "General" )] public float TuckRange { get; set; } = 30f;

	[Property, Group( "General" )] public int Slot { get; set; } = 0;

	/// <summary>Firing sound when clip is empty</summary>
	[Property, Group( "Sounds" )] public SoundEvent DeploySound { get; set; }


	/// <summary>Default weapon field of view</summary>
	[Property, Group( "FOV" )] public float FOV { get; set; } = 70f;

	/// <summary>Weapon FOV while aiming (-1 to use default weapon fov)</summary>
	[Property, Group( "FOV" )] public float AimFOV { get; set; } = -1f;

	/// <summary>Player FOV while aiming (-1 to use default player fov)</summary>
	[Property, Group( "FOV" )] public float AimPlayerFOV { get; set; } = -1f;

	/// <summary>FOV aim in speed</summary>
	[Property, Group( "FOV" ), Title( "Aim in FOV speed" )] public float AimInFOVSpeed { get; set; } = 1f;

	/// <summary>FOV aim out speed</summary>
	[Property, Group( "FOV" ), Title( "Aim out FOV speed" )] public float AimOutFOVSpeed { get; set; } = 1f;


	/// <summary>Procedural animation speed (lower is slower)</summary>
	[Property, Group( "Animations" )] public float AnimSpeed { get; set; } = 1;

	/// <summary>Offset used for setting the weapon to its aim position</summary>
	[Property, Group( "Animations" ), Title( "Aim Offset (swb_editor_offsets)" )] public AngPos AimAnimData { get; set; }

	/// <summary>Offset used for setting the weapon to its run position</summary>
	[Property, Group( "Animations" ), Title( "Run Offset (swb_editor_offsets)" )] public AngPos RunAnimData { get; set; }

	/// <summary>Offset used for setting the weapon to its run position</summary>
	[Property, Group( "Animations" ), Title( "Customizing Offset (swb_editor_offsets)" )] public AngPos CustomizeAnimData { get; set; }

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


	/// <summary>Is the weapon reloading shells instead of a magazine?</summary>
	[Property, Group( "Shell Reloading" )] public bool ShellReloading { get; set; } = false;

	/// <summary>Can the weapon shoot while reloading to cancel the reload?</summary>
	[Property, Group( "Shell Reloading" )] public bool ShellReloadingShootCancel { get; set; } = true;

	/// <summary>Delay in fire animation to eject the shell</summary>
	[Property, Group( "Shell Reloading" )] public float ShellEjectDelay { get; set; } = 0;

	/// <summary>Duration of the shell reload start animation (animation is set with ReloadAnim)</summary>
	[Property, Group( "Shell Reloading" )] public float ShellReloadStartTime { get; set; } = 0;

	/// <summary>Duration of the shell reload insert animation (animation is set in animgraph)</summary>
	[Property, Group( "Shell Reloading" )] public float ShellReloadInsertTime { get; set; } = 0;


	/// <summary>Is this a bolt action weapon?</summary>
	[Property, Group( "Bolt Action Reloading" ), Title( "Bolt Action" )] public bool BoltBack { get; set; } = false;

	/// <summary>Duration of the boltback animation</summary>
	[Property, Group( "Bolt Action Reloading" )] public float BoltBackTime { get; set; } = 0f;

	/// <summary>Boltback animation</summary>
	[Property, Group( "Bolt Action Reloading" )] public string BoltBackAnim { get; set; } = "boltback";

	/// <summary>Bullet eject delay during the boltback animation (-1 to disable)</summary>
	[Property, Group( "Bolt Action Reloading" )] public float BoltBackEjectDelay { get; set; } = 0f;

	/// <summary>Enable scoping, renders a 2D scope on ADS</summary>
	[Property, Group( "Scoping" )] public bool Scoping { get; set; } = false;

	/// <summary>Scope Information</summary>
	[Property, Group( "Scoping" )] public ScopeInfo ScopeInfo { get; set; } = new();

	/// <summary>Primary attack data</summary>
	[Property, Group( "Firing" ), Title( "Primary ShootInfo (component)" )] public ShootInfo Primary { get; set; } = new();

	/// <summary>Secondary attack data (setting this will disable weapon aiming)</summary>
	[Property, Group( "Firing" ), Title( "Secondary ShootInfo (component)" )] public ShootInfo Secondary { get; set; }


	/// <summary>Time since the last primary attack</summary>
	public TimeSince TimeSincePrimaryShoot { get; set; }

	/// <summary>Time since the last secondary attack</summary>
	public TimeSince TimeSinceSecondaryShoot { get; set; }

	/// <summary>Time since deployment</summary>
	public TimeSince TimeSinceDeployed { get; set; }

	/// <summary>Time since the last reload</summary>
	public TimeSince TimeSinceReload { get; set; }

	public bool IsCustomizing { get; set; }

	/// <summary>If the player is running</summary>
	public bool IsRunning => Owner.IsRunning && Owner.IsOnGround && Owner.Velocity.Length >= 200;

	/// <summary>If the player is crouching</summary>
	public bool IsCrouching => Owner.IsCrouching;

	/// <summary>Is the view model visible</summary>
	public bool CanSeeViewModel => !IsProxy && Owner.IsFirstPerson;

	/// <summary>If the weapon is being reloaded</summary>
	[Sync] public bool IsReloading { get; set; }

	/// <summary>If the weapon is being aimed</summary>
	[Sync] public bool IsAiming { get; set; }

	/// <summary>If the weapon is being scoped</summary>
	[Sync] public bool IsScoping { get; set; }

	/// <summary>If the weapon is being bolt back reloaded</summary>
	[Sync] public bool InBoltBack { get; set; }

	public StatsModifier InitialPrimaryStats { get; private set; }
	public StatsModifier InitialSecondaryStats { get; private set; }

	public bool IsDeploying => TimeSinceDeployed < 0;

	// Private
	int burstCount = 0;
	int barrelHeat = 0;
}
