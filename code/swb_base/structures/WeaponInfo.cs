using Sandbox;

namespace SWB_Base;

public partial class WeaponInfo : BaseNetworkable
{
    // FOV //

    /// <summary>Default weapon FOV</summary>
    [Net] public int FOV { get; set; } = 90;

    /// <summary>Weapon FOV while zooming (-1 to use default weapon fov)</summary>
    [Net] public int ZoomWeaponFOV { get; set; } = -1;

    /// <summary>Player FOV while zooming (-1 to use default player fov)</summary>
    [Net] public int ZoomPlayerFOV { get; set; } = -1;

    /// <summary>FOV zoom in speed</summary>
    [Net] public int ZoomInFOVSpeed { get; set; } = 1;

    /// <summary>FOV zoom out speed</summary>
    [Net] public int ZoomOutFOVSpeed { get; set; } = 1;

    // Other //

    /// <summary>Aim sensitivity while zooming (lower is slower)</summary>
    [Net] public float AimSensitivity { get; set; } = 0.85f;

    /// <summary>Procedural animation speed (lower is slower)</summary>
    [Net] public float WalkAnimationSpeedMod { get; set; } = 1;

    // Animations // 

    /// <summary>Duration of the draw animation</summary>
    [Net] public float DrawTime { get; set; } = 0.5f;

    /// <summary>Duration of the reload animation</summary>
    [Net] public float ReloadTime { get; set; } = 1f;

    /// <summary>Duration of the empty reload animation (-1 to disable)</summary>
    [Net] public float ReloadEmptyTime { get; set; } = -1f;

    /// <summary>Duration of the boltback animation (-1 to disable)</summary>
    [Net] public float BoltBackTime { get; set; } = -1f;

    /// <summary>Bullet eject delay during the boltback animation (-1 to disable)</summary>
    [Net] public float BoltBackEjectDelay { get; set; } = 0f;

    /// <summary>Reloading animation</summary>
    [Net] public string ReloadAnim { get; set; } = "reload";

    /// <summary>Reloading animation when clip is empty</summary>
    [Net] public string ReloadEmptyAnim { get; set; } = "reload_empty";

    /// <summary>Draw animation</summary>
    [Net] public string DrawAnim { get; set; } = "deploy";

    /// <summary>Draw animation when there is no ammo</summary>
    [Net] public string DrawEmptyAnim { get; set; } = "";

    /// <summary>Bolt pullback animation (usually a sniper)</summary>
    [Net] public string BoltBackAnim { get; set; } = "boltback";
}
