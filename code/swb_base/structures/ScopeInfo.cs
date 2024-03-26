namespace SWB.Base;

public class ScopeInfo
{
	/// <summary>2D lens texture</summary>
	[Property, ImageAssetPathAttribute] public string LensTexture { get; set; } = "/materials/swb/scopes/swb_lens_hunter.png";

	/// <summary>2D scope texture</summary>
	[Property, ImageAssetPathAttribute] public string ScopeTexture { get; set; } = "/materials/swb/scopes/swb_scope_hunter.png";

	/// <summary>Delay between ADS and scoping in ms</summary>
	[Property] public float ScopeInDelay { get; set; } = 0.2f;

	/// <summary>Sound that plays when scoping starts</summary>
	[Property] public SoundEvent ScopeInSound { get; set; }

	/// <summary>Sound that plays when scoping ends</summary>
	[Property] public SoundEvent ScopeOutSound { get; set; }

	/// <summary>Player FOV while scoping</summary>
	[Property] public float FOV { get; set; } = 8f;

	/// <summary>Mouse sensitivity while scoping (lower is slower, 0 to use AimSensitivity while scoped)</summary>
	[Property] public float AimSensitivity { get; set; } = 0.25f;
}
