using Sandbox;
using System.Collections.Generic;

namespace SWB_Base
{
	public enum HoldType
	{
		Pistol = 1,
		Rifle = 2,
		Shotgun = 3
	}

	public partial class WeaponBase
	{
		// Virtual
		public virtual int Bucket => 1; // Inventory slot position
		public virtual int BucketWeight => 100; // Inventory slot position weight ( higher = more important )
		public virtual bool DrawCrosshair => true; // Draw the crosshair
		public virtual bool DropWeaponOnDeath => true; // Drop the weapon on death
		public virtual string FreezeViewModelOnZoom => null; // Some weapons have looping idle animations -> force spam another animation to "freeze" it
		public virtual int FOV => 65; // Default FOV
		public virtual int ZoomFOV => 65; // FOV while zooming
		public virtual float TuckRange => 30; // Range that tucking should be enabled (set to -1 to disable tucking)
		public virtual HoldType HoldType => HoldType.Pistol; // Thirdperson holdtype
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl"; // Path to the view model
		public virtual string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl"; // Path to the world model
		public virtual string Icon => ""; // Path to an image that represent the weapon on the HUD
		public virtual float WalkAnimationSpeedMod => 1; // Procedural animation speed ( lower is slower )
		public virtual float AimSensitivity => 0.85f; // Aim sensitivity while zooming ( lower is slower )
		public virtual bool DualWield => false; // If the weapon should be dual wielded

		// Properties
		public List<AnimatedAction> AnimatedActions { get; set; } // Extra actions that use certain key combinations to trigger animations

		public AngPos ZoomAnimData { get; set; } // Data used for setting the weapon to its zoom position

		public AngPos RunAnimData { get; set; } // Data used for setting the weapon to its run position

		[Net]
		public ClipInfo Primary { get; set; } = new ClipInfo(); // Primary attack data

		[Net]
		public ClipInfo Secondary { get; set; } = null; // Secondary attack data ( setting this will disable weapon zooming )

		[Net, Predicted]
		public TimeSince TimeSincePrimaryAttack { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceSecondaryAttack { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceReload { get; set; }

		[Net, Predicted]
		public bool IsReloading { get; set; }

		[Net, Predicted]
		public bool IsZooming { get; set; }

		[Net, Predicted]
		public bool IsRunning { get; set; }

		[Net, Predicted]
		public bool IsAnimating { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceDeployed { get; set; }

		public PickupTrigger PickupTrigger { get; protected set; }

		// Private
		private bool doRecoil = false;

		private BaseViewModel dualWieldViewModel;
		private bool isDualWieldConverted = false;
		private bool dualWieldLeftFire = false;
		private bool dualWieldShouldReload = false;
	}
}
