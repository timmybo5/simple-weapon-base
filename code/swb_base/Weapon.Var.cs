using SWB.Shared;

namespace SWB.Base;

public partial class Weapon
{
	/// <summary>Unique name that identifies the weapon</summary>
	[Property, Group( "General" ), Order( 0 ), Feature( "Core", Icon = "hub" )] public string ClassName { get; set; }

	[Property, Group( "General" ), Feature( "Core" )] public string DisplayName { get; set; }

	[Property, Group( "General" ), Feature( "Core" ), ImageAssetPath] public string Icon { get; set; }

	[Property, Group( "General" ), Feature( "Core" )] public CrosshairSettings CrosshairSettings { get; set; } = new();

	/// <summary>How the player holds the weapon in thirdperson</summary>
	[Property, Group( "General" ), Feature( "Core" )] public HoldTypes HoldType { get; set; } = HoldTypes.Pistol;

	/// <summary>Can bullets be cocked in the barrel? (clip ammo + 1)</summary>
	[Property, Group( "General" ), Feature( "Core" )] public bool BulletCocking { get; set; } = true;

	/// <summary>Range that tucking should be enabled (-1 to disable tucking)</summary>
	[Property, Group( "General" ), Feature( "Core" )] public float TuckRange { get; set; } = 30f;

	/// <summary>How much movement speed is affected by holding this weapon (Speed *= Mobility)</summary>
	[Property, Group( "General" ), Feature( "Core" )] public float Mobility { get; set; } = 1f;

	/// <summary>A speed multiplier for all reloading animations</summary>
	[Property, Group( "General" ), Feature( "Core" ), Sync] public float ReloadSpeed { get; set; } = 1f;

	[Property, Group( "General" ), Feature( "Core" )] public int Slot { get; set; } = 0;

	/// <summary>View Model field of view</summary>
	[Property, Group( "General" ), Feature( "Core" )] public float ViewModelFOV { get; set; } = 70f;

	/// <summary>Aim Information</summary>
	[Property, Group( "General" ), Feature( "Core" )]
	public AimInfo AimInfo { get; set; } = new AimInfo()
	{
		Sensitivity = 0.85f,
	};

	/// <summary>Firstperson Model</summary>
	[Property, Group( "Models", Icon = "3d_rotation" ), Order( 1 ), Feature( "Core" )] public Model ViewModel { get; set; }

	/// <summary>Firstperson Hands Model</summary>
	[Property, Group( "Models" ), Feature( "Core" )] public Model ViewModelHands { get; set; }

	/// <summary>Thirdperson Model</summary>
	[Property, Group( "Models" ), Feature( "Core" )] public Model WorldModel { get; set; }

	/// <summary>Enable scoping, renders a 2D scope on ADS</summary>
	[Property, Group( "Scoping" ), Order( 5 ), Feature( "Core" )] public bool Scoping { get; set; } = false;

	/// <summary>Scope Information</summary>
	[Property, Group( "Scoping" ), Feature( "Core" )] public ScopeInfo ScopeInfo { get; set; } = new();

	/// <summary>Firing sound when clip is empty</summary>
	[Property, Group( "Sounds" ), Order( 9 ), Feature( "Core" )] public SoundEvent DeploySound { get; set; }

	/// <summary>Primary attack data</summary>
	[Property, Group( "Firing" ), Order( 10 ), Feature( "Core" ), Title( "Primary ShootInfo (component)" ), RequireComponent] public ShootInfo Primary { get; set; }

	/// <summary>Secondary attack data (setting this will disable weapon aiming)</summary>
	[Property, Group( "Firing" ), Feature( "Core" ), Title( "Secondary ShootInfo (component)" )] public ShootInfo Secondary { get; set; }


	/// <summary>Procedural animation speed (lower is slower)</summary>
	[Property, Group( "General" ), Order( 0 ), Feature( "Animations", Icon = "animation" )] public float AnimSpeed { get; set; } = 1;

	/// <summary>Offset used for setting the weapon to its aim position</summary>
	[Property, Group( "General" ), Feature( "Animations" ), Title( "Aim Offset (swb_editor_offsets)" )] public AngPos AimAnimData { get; set; }

	/// <summary>Offset used for setting the weapon to its run position</summary>
	[Property, Group( "General" ), Feature( "Animations" ), Title( "Run Offset (swb_editor_offsets)" )] public AngPos RunAnimData { get; set; }

	/// <summary>Offset used for setting the weapon to its run position</summary>
	[Property, Group( "General" ), Feature( "Animations" ), Title( "Customizing Offset (swb_editor_offsets)" )] public AngPos CustomizeAnimData { get; set; }

	/// <summary>Duration of the reload animation</summary>
	[Property, Group( "General" ), Feature( "Animations" )] public float ReloadTime { get; set; } = 1f;

	/// <summary>Reloading animation</summary>
	[Property, Group( "General" ), Feature( "Animations" )] public string ReloadAnim { get; set; } = "reload";

	/// <summary>Duration of the empty reload animation (-1 to disable)</summary>
	[Property, Group( "General" ), Feature( "Animations" )] public float ReloadEmptyTime { get; set; } = -1f;

	/// <summary>Reloading animation when clip is empty</summary>
	[Property, Group( "General" ), Feature( "Animations" )] public string ReloadEmptyAnim { get; set; } = "reload_empty";

	/// <summary>Duration of the draw animation</summary>
	[Property, Group( "General" ), Feature( "Animations" )] public float DrawTime { get; set; } = 0.5f;

	/// <summary>Draw animation</summary>
	[Property, Group( "General" ), Feature( "Animations" )] public string DrawAnim { get; set; } = "deploy";

	/// <summary>Duration of the empty draw animation (-1 to disable)</summary>
	[Property, Group( "General" ), Feature( "Animations" )] public float DrawEmptyTime { get; set; } = -1f;

	/// <summary>Draw animation when there is no ammo</summary>
	[Property, Group( "General" ), Feature( "Animations" )] public string DrawEmptyAnim { get; set; } = "";


	/// <summary>Is the weapon reloading shells instead of a magazine?</summary>
	[Property, Group( "Shell Reloading" ), Order( 1 ), Feature( "Animations" )] public bool ShellReloading { get; set; } = false;

	/// <summary>Can the weapon shoot while reloading to cancel the reload?</summary>
	[Property, Group( "Shell Reloading" ), Feature( "Animations" )] public bool ShellReloadingShootCancel { get; set; } = true;

	/// <summary>Delay in fire animation to eject the shell</summary>
	[Property, Group( "Shell Reloading" ), Feature( "Animations" )] public float ShellEjectDelay { get; set; } = 0;

	/// <summary>Duration of the shell reload start animation (animation is set with ReloadAnim)</summary>
	[Property, Group( "Shell Reloading" ), Feature( "Animations" )] public float ShellReloadStartTime { get; set; } = 0;

	/// <summary>Duration of the shell reload insert animation (animation is set in animgraph)</summary>
	[Property, Group( "Shell Reloading" ), Feature( "Animations" )] public float ShellReloadInsertTime { get; set; } = 0;


	/// <summary>Is this a bolt action weapon?</summary>
	[Property, Group( "Bolt Action Reloading" ), Order( 2 ), Feature( "Animations" ), Title( "Bolt Action" )] public bool BoltBack { get; set; } = false;

	/// <summary>Duration of the boltback animation</summary>
	[Property, Group( "Bolt Action Reloading" ), Feature( "Animations" )] public float BoltBackTime { get; set; } = 0f;

	/// <summary>Boltback animation</summary>
	[Property, Group( "Bolt Action Reloading" ), Feature( "Animations" )] public string BoltBackAnim { get; set; } = "boltback";

	/// <summary>Bullet eject delay during the boltback animation (-1 to disable)</summary>
	[Property, Group( "Bolt Action Reloading" ), Feature( "Animations" )] public float BoltBackEjectDelay { get; set; } = 0f;


	/// <summary>Time since the last primary attack</summary>
	public TimeSince TimeSincePrimaryShoot { get; set; }

	/// <summary>Time since the last secondary attack</summary>
	public TimeSince TimeSinceSecondaryShoot { get; set; }

	/// <summary>Time since deployment</summary>
	public TimeSince TimeSinceDeployed { get; set; }

	/// <summary>Time since the last reload</summary>
	public TimeSince TimeSinceReload { get; set; }

	/// <summary>Time since the weapon was in run animation</summary>
	public TimeSince TimeSinceRunning { get; set; }

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
	public bool ShouldTuckVar = false;
	public float TuckDist = -1;

	// Private
	int burstCount = 0;
	int barrelHeat = 0;
}
