using Sandbox;

namespace SWB_Base
{
    public partial class WeaponInfo : BaseNetworkable
    {
        [Net]
        public float DrawTime { get; set; } = 0.5f; // Duration of the draw animation
        [Net]
        public float ReloadTime { get; set; } = 1f; // Duration of the reload animation
        [Net]
        public float ReloadEmptyTime { get; set; } = -1f; // Duration of the empty reload animation ( -1 to disable )
        [Net]
        public float BoltBackTime { get; set; } = -1f; // Duration of the bolback animation ( -1 to disable )
        [Net]
        public float BoltBackEjectDelay { get; set; } = 0f; // Bullet eject delay during the boltback animation

        // Strings
        [Net]
        public string ReloadAnim { get; set; } = "reload"; // Reloading animation
        [Net]
        public string ReloadEmptyAnim { get; set; } = "reload_empty"; // Reloading animation when clip is empty
        [Net]
        public string DrawAnim { get; set; } = "deploy"; // Draw animation
        [Net]
        public string DrawEmptyAnim { get; set; } = ""; // Draw animation when there is no ammo
        [Net]
        public string BoltBackAnim { get; set; } = "boltback"; // Bolt pullback animation ( usually a sniper )
    }
}
