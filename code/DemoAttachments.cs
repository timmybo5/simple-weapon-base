using SWB.Base;
using SWB.Base.Attachments;

namespace SWB.Demo;

/*
 * Almost all attachment properties can be edited from the scene inspector.
 * For ease of reusability I opted to initialize most properties by code but you don't have to
 */

[Title( "Reflex Sight" )]
public class ReflexSightBG : SightAttachment
{
	public override string Name => "Walther MRS Reflex";
	public override string IconPath => "attachments/swb/sight/reflex/ui/icon.png";
	public override string BodyGroup { get; set; } = "sight";
	public override int BodyGroupChoice { get; set; } = 1;
	public override int BodyGroupDefault { get; set; } = 0;

	// Sight
	public override float AimPlayerFOV { get; set; } = 50f;
	public override float AimSensitivity { get; set; } = 0.5f;
}

[Title( "Hunter Scope" )]
public class HunterScope2DBG : Scope2DAttachment
{
	public override string Name => "Hunter Scope x12";
	public override string IconPath => "attachments/swb/sight/scope_hunter/ui/icon.png";
	public override string BodyGroup { get; set; } = "sight";
	public override int BodyGroupChoice { get; set; } = 2;
	public override int BodyGroupDefault { get; set; } = 0;

	// Scope
	public override ScopeInfo ScopeInfo { get; set; } = new()
	{
		LensTexture = "materials/swb/scopes/swb_lens_hunter.png",
		ScopeTexture = "materials/swb/scopes/swb_scope_hunter.png",
		ScopeInDelay = 0.2f,
		ScopeInSound = ResourceLibrary.Get<SoundEvent>( "sounds/swb/scope/swb_sniper.zoom_in.sound" ),
		FOV = 8f,
		AimSensitivity = 0.25f
	};
}

[Title( "Silencer (sniper)" )]
public class SniperSilencerBG : SilencerAttachment
{
	public override string Name => "ATS5 Silencer";
	public override string IconPath => "attachments/swb/muzzle/silencer_sniper/ui/icon.png";
	public override string BodyGroup { get; set; } = "muzzle";
	public override int BodyGroupChoice { get; set; } = 1;
	public override int BodyGroupDefault { get; set; } = 0;

	// Silencer
	public override ParticleSystem MuzzleFlashParticle { get; set; } = ParticleSystem.Load( "particles/swb/muzzle/flash_silenced.vpcf" );
	[Property, Group( "Silencer" )] public override SoundEvent ShootSound { get; set; } = ResourceLibrary.Get<SoundEvent>( "sounds/swb/attachments/silencer/swb_sniper.silenced.fire.sound" );
}

[Title( "Silencer (rifle)" )]
public class RifleSilencerBG : SniperSilencerBG
{
	public override string Name => "ATS4 Silencer";
	public override string IconPath => "attachments/swb/muzzle/silencer_rifle/ui/icon.png";
}

[Title( "Silencer (shotgun)" )]
public class ShotgunSilencerBG : SniperSilencerBG
{
	public override string Name => "Salvo 12G Silencer";
	public override string IconPath => "attachments/swb/muzzle/silencer_shotgun/ui/icon.png";
}

[Title( "Silencer (pistol)" )]
public class PistolSilencerBG : SniperSilencerBG
{
	public override string Name => "SR8 Silencer";
	public override string IconPath => "attachments/swb/muzzle/silencer_pistol/ui/icon.png";
}

[Title( "Medium Laser" )]
public class RifleLaserBG : LaserAttachment
{
	public override string Name => "PEQ-15 Laser";
	public override string IconPath => "attachments/swb/tactical/laser_rifle/ui/icon.png";
	public override string BodyGroup { get; set; } = "laser";
	public override int BodyGroupChoice { get; set; } = 1;
	public override int BodyGroupDefault { get; set; } = 0;
}

[Title( "Rail (sniper)" )]
public class SniperRailBG : RailAttachment
{
	public override string Name => "UTG Quad-Rail";
	public override string IconPath => "attachments/swb/rail/rail_quad/ui/icon.png";
	public override string BodyGroup { get; set; } = "rail";
	public override int BodyGroupChoice { get; set; } = 1;
	public override int BodyGroupDefault { get; set; } = 0;
}

[Title( "Rail (pistol)" )]
public class PistolRailBG : RailAttachment
{
	public override string Name => "UTG Single-Rail";
	public override string IconPath => "attachments/swb/rail/rail_single/ui/icon.png";
	public override string BodyGroup { get; set; } = "rail";
	public override int BodyGroupChoice { get; set; } = 1;
	public override int BodyGroupDefault { get; set; } = 0;
}

