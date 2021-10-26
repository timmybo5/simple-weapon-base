using Sandbox;

namespace SWB_Base
{
    public partial class UISettings : BaseNetworkable
    {
        [Net]
        public bool ShowHealth { get; set; } = true; // Show health counter
        [Net]
        public bool ShowAmmo { get; set; } = true; // Show ammo counter
        [Net]
        public bool ShowCrosshair { get; set; } = true; // Show crosshair
        [Net]
        public bool ShowCrosshairDot { get; set; } = true; // Show crosshair dot
        [Net]
        public bool ShowCrosshairLines { get; set; } = true; // Show crosshair lines
        [Net]
        public bool ShowHitmarker { get; set; } = true; // Show hitmarker

        [Net]
        public bool PlayHitmarkerSound { get; set; } = true; // Play the hitmarker sound
    }
}
