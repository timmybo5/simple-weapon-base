using Sandbox;

/**
 * Unfinished, some asset features missing that would make the base clutered.
 * Waiting until they improve the asset system.
 */

namespace SWB_Base;

[GameResource("SWB Weapon", "swb", "A weapon on the Simple Weapon Base.", Icon = "military_tech")]
public class WeaponBaseAsset : GameResource
{
    [Property] public float DrawTime { get; set; } = 0.5f;
    [Property] public float ReloadTime { get; set; } = 1f;
    [Property] public float ReloadEmptyTime { get; set; } = -1f;
    [Property] public float BoltBackTime { get; set; } = -1f;
    [Property] public float BoltBackEjectDelay { get; set; } = 0f;
    [Property] public string ReloadAnim { get; set; } = "reload";
    [Property] public string ReloadEmptyAnim { get; set; } = "reload_empty";
    [Property] public string DrawAnim { get; set; } = "deploy";
    [Property] public string DrawEmptyAnim { get; set; } = "";
    [Property] public string BoltBackAnim { get; set; } = "boltback";
}
