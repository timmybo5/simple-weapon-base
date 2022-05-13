using System;
using Sandbox;

/* 
 * Weapon base console variables (convars)
*/

namespace SWB_Base
{
    public partial class WeaponBase
    {
        // Server
        [ConVar.Replicated("swb_sv_showhud", Help = "Enable HUD for all clients", Saved = true)]
        public static int ShowHudSV { get; set; } = 1;

        [ConVar.Replicated("swb_sv_autoreload", Help = "Reload weapons automatically when shooting if empty", Saved = true)]
        public static int AutoReloadSV { get; set; } = 0;

        [ConVar.Replicated("swb_sv_customization", Help = "Enable the customization menu (Q)", Saved = true)]
        public static int EnableCustomizationSV { get; set; } = 1;

        [ConVar.Replicated("swb_sv_debug_bullets", Help = "Shows physical bullet path and distance", Saved = true)]
        public static int DebugBulletsSV { get; set; } = 0;

        // Client
        [ConVar.ClientData("swb_cl_showhud", Help = "Enable HUD", Saved = true)]
        public static int ShowHudCL { get; set; } = 1;

        /// <summary>
        /// Gets a console variable and returns it with a specific type
        /// </summary>
        public T GetSetting<T>(string setting, T defaultValue)
        {
            string value;

            if (IsClient)
            {
                value = ConsoleSystem.GetValue(setting, defaultValue.ToString());
            }
            else
            {
                value = Client.GetClientData(setting, defaultValue.ToString());
            }

            // Bool support for ints
            if (typeof(T) == typeof(bool))
            {
                int number;
                bool success = int.TryParse(value, out number);
                value = (success && number > 0).ToString();
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
