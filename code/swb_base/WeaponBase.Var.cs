using System.Collections.Generic;
using Sandbox;

/* 
 * Weapon base properties/fields
*/

namespace SWB_Base
{
    public enum HoldType
    {
        Pistol = 1,
        Rifle = 2,
        Shotgun = 3,
        Carry = 4,
        Fists = 5
    }

    public partial class WeaponBase
    {
        // Virtual // 

        /// <summary>Inventory slot position</summary>
        public virtual int Bucket => 1;

        /// <summary>Inventory slot position weight (higher = more important)</summary>
        public virtual int BucketWeight => 100;

        /// <summary>Can manually drop weapon</summary>
        public virtual bool CanDrop => true;

        /// <summary>Drop the weapon on death</summary>
        public virtual bool DropWeaponOnDeath => true;

        /// <summary>Can bullets be cocked in the barrel? (clip ammo + 1)</summary>
        public virtual bool BulletCocking => true;

        /// <summary>Should the barrel smoke after heavy weapon usage?</summary>
        public virtual bool BarrelSmoking => true;

        /// <summary>Default FOV</summary>
        public virtual int FOV => 65;

        /// <summary>FOV while zooming</summary>
        public virtual int ZoomFOV => 65;

        /// <summary>FOV zoom in speed</summary>
        public virtual int ZoomInFOVSpeed => 1;

        /// <summary>FOV zoom out speed</summary>
        public virtual int ZoomOutFOVSpeed => 10;

        /// <summary>Range that tucking should be enabled (-1 to disable tucking)</summary>
        public virtual float TuckRange => 30;

        /// <summary>How the player holds the weapon in thirdperson</summary>
        public virtual HoldType HoldType => HoldType.Pistol;

        /// <summary>Path to the hands model (will be bonemerged with viewmodel, leave empty to disable)</summary>
        public virtual string HandsModelPath => "";

        /// <summary>Path to the view model</summary>
        public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

        /// <summary>Offset the viewmodel with an angle (not available in ModelDoc)</summary>
        public virtual AngPos ViewModelOffset => new();

        /// <summary>Path to the world model</summary>
        public virtual string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";

        /// <summary>Path to an image that represent the weapon on the HUD</summary>
        public virtual string Icon => "";

        /// <summary>Procedural animation speed (lower is slower)</summary>
        public virtual float WalkAnimationSpeedMod => 1;

        /// <summary>Aim sensitivity while zooming (lower is slower)</summary>
        public virtual float AimSensitivity => 0.85f;

        /// <summary>Delay before firing when the primary attack button is pressed</summary>
        public virtual float PrimaryDelay => -1;

        /// <summary>Delay before firing when the secondary attack button is pressed</summary>
        public virtual float SecondaryDelay => -1;

        // Properties // 

        /// <summary>Display name</summary>
        public string PrintName { get { return DisplayInfo.For(this).Name; } }

        /// <summary>Extra actions that use certain key combinations to trigger animations</summary>
        public List<AnimatedAction> AnimatedActions { get; set; }

        /// <summary>List of all weapon attachments</summary>
        public List<AttachmentCategory> AttachmentCategories { get; set; }

        /// <summary>Data used for setting the weapon to its zoom position</summary>
        public AngPos ZoomAnimData { get; set; }

        /// <summary>Data used for setting the weapon to its run position</summary>
        public AngPos RunAnimData { get; set; }

        /// <summary>Data used for setting the weapon to its customization position</summary>
        public AngPos CustomizeAnimData { get; set; }

        /// <summary>Weapon UI settings</summary>
        public UISettings UISettings { get; set; } = new UISettings();

        /// <summary>General data</summary>
        [Net]
        public WeaponInfo General { get; set; } = new WeaponInfo();

        /// <summary>Primary attack data</summary>
        [Net]
        public ClipInfo Primary { get; set; } = new ClipInfo();

        /// <summary>Secondary attack data (setting this will disable weapon zooming)</summary>
        [Net]
        public ClipInfo Secondary { get; set; } = null;

        /// <summary>Time since the last primary attack</summary>
        public TimeSince TimeSincePrimaryAttack { get; set; }

        /// <summary>Time since the last secondary attack</summary>
        public TimeSince TimeSinceSecondaryAttack { get; set; }

        /// <summary>Time since the last reload</summary>
        public TimeSince TimeSinceReload { get; set; }

        /// <summary>Time since deployment</summary>
        public TimeSince TimeSinceDeployed { get; set; }

        /// <summary>Time since added to inventory</summary>
        public TimeSince TimeSinceActiveStart { get; set; }

        /// <summary>If the weapon is being reloaded</summary>
        public bool IsReloading { get; set; }

        /// <summary>If the weapon is being zoomed</summary>
        public bool IsZooming { get; set; }

        /// <summary>If the weapon is being scope</summary>
        public bool IsScoped { get; set; }

        /// <summary>If the weapon is being ran with</summary>
        public bool IsRunning { get; set; }

        /// <summary>If the weapon is being customized (client only)</summary>
        public bool IsCustomizing { get; set; }

        /// <summary>If the weapon is being animated (AnimatedActions)</summary>
        public bool IsAnimating { get; set; }

        /// <summary>If the weapon is being bolt backed</summary>
        [Net]
        public bool InBoltBack { get; set; }

        /// <summary>Instance ID</summary>
        public int InstanceID { get; protected set; }

        /// <summary>Model of the hands</summary>
        public BaseViewModel HandsModel;

        /// <summary>Initial stats (not actual)</summary>
        public StatModifier InitialStats { get; protected set; }

        /// <summary>Bullet velocity modifier (phys bullets only)</summary>
        public float BulletVelocityMod = 1;

        // Private
        private bool doRecoil = false;
        private int burstCount = 0;

        private int barrelHeat = 0;
        private TimeSince timeSinceFired;
    }
}
