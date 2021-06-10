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
		public virtual int Bucket => 1;
		public virtual int BucketWeight => 100;
		public virtual bool DrawCrosshair => true;
		public virtual bool DropWeaponOnDeath => true;
		public virtual string FreezeViewModelOnZoom => null; // Some weapons have looping idle animations -> force spam another animation to "freeze" it
		public virtual int FOV => 65;
		public virtual int ZoomFOV => 65;
		public virtual float TuckRange => 30; // Set to -1 to disable tucking
		public virtual HoldType HoldType => HoldType.Pistol;
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
		public virtual string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
		public virtual float WalkAnimationSpeedMod => 1;
		public virtual bool DualWield => false;

		// Properties
		public List<AnimatedAction> AnimatedActions { get; set; }

		public AngPos ZoomAnimData { get; set; }

		public AngPos RunAnimData { get; set; }

		[Net]
		public ClipInfo Primary { get; set; } = new ClipInfo();

		[Net]
		public ClipInfo Secondary { get; set; } = null;

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
