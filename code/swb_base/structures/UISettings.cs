using Sandbox;

namespace SWB_Base
{
    public class UISettings
    {
        public bool ShowHealth { get; set; } = true; // Show health counter

        public bool ShowAmmoCount { get; set; } = true; // Show ammo counter

        public bool ShowFireMode { get; set; } = true; // Show active fire mode icon (semi/auto)

        public bool ShowWeaponIcon { get; set; } = true; // Show weapon icon

        public bool ShowCrosshair { get; set; } = true; // Show crosshair

        public bool ShowCrosshairDot { get; set; } = true; // Show crosshair dot

        public bool ShowCrosshairLines { get; set; } = true; // Show crosshair lines

        public bool ShowHitmarker { get; set; } = true; // Show hitmarker

        public bool PlayHitmarkerSound { get; set; } = true; // Play the hitmarker sound
    }
}
