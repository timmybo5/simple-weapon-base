using Sandbox;

namespace SWB_Base
{
    public class UISettings
    {
        /// <summary>Show health icon</summary>
        public bool ShowHealthIcon { get; set; } = true;

        /// <summary>Show health counter</summary>
        public bool ShowHealthCount { get; set; } = true;

        /// <summary>Show ammo counter</summary>
        public bool ShowAmmoCount { get; set; } = true;

        /// <summary>Show active fire mode icon (semi/auto)</summary>
        public bool ShowFireMode { get; set; } = true;

        /// <summary>Show weapon icon</summary>
        public bool ShowWeaponIcon { get; set; } = true;

        /// <summary>Show crosshair</summary>
        public bool ShowCrosshair { get; set; } = true;

        /// <summary>Show crosshair dot</summary>
        public bool ShowCrosshairDot { get; set; } = true;

        /// <summary>Show crosshair lines</summary>
        public bool ShowCrosshairLines { get; set; } = true;

        /// <summary>Show hitmarker</summary>
        public bool ShowHitmarker { get; set; } = true;

        /// <summary>Play the hitmarker sound</summary>
        public bool PlayHitmarkerSound { get; set; } = true;
    }
}
