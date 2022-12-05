using Sandbox;

namespace SWB_Base;

public partial class WeaponInfo : BaseNetworkable
{
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

    // Strings // 

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
