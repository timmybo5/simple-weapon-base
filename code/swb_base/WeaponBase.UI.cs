using Sandbox;
using Sandbox.UI;
using SWB_Base.UI;

/* 
 * Weapon base UI
*/

namespace SWB_Base
{
    public partial class WeaponBase
    {
        private Panel healthDisplay;
        private Panel ammoDisplay;
        private Panel customizationMenu;

        private Panel hitmarker;
        private Panel crosshair;

        public override void CreateHudElements()
        {
            if (UISettings.HideAll) return;

            var showHUDCL = GetSetting<bool>("swb_cl_showhud", true);
            var showHUDSV = GetSetting<bool>("swb_sv_showhud", true);

            if (Local.Hud == null || !showHUDCL || !showHUDSV) return;

            if (UISettings.ShowCrosshair)
            {
                crosshair = CreateCrosshair();
                crosshair.Parent = Local.Hud;
            }

            if (UISettings.ShowHitmarker)
            {
                hitmarker = new Hitmarker
                {
                    Parent = Local.Hud
                };
            }

            if (UISettings.ShowHealthCount || UISettings.ShowHealthIcon)
            {
                healthDisplay = new HealthDisplay(UISettings)
                {
                    Parent = Local.Hud
                };
            }

            if (UISettings.ShowAmmoCount || UISettings.ShowWeaponIcon || UISettings.ShowFireMode)
            {
                ammoDisplay = new AmmoDisplay(UISettings)
                {
                    Parent = Local.Hud
                };
            }
        }

        public virtual Panel CreateCrosshair()
        {
            return new Crosshair();
        }

        public override void DestroyHudElements()
        {
            base.DestroyHudElements();

            if (healthDisplay != null) healthDisplay.Delete(true);
            if (ammoDisplay != null) ammoDisplay.Delete(true);
            if (hitmarker != null) hitmarker.Delete(true);
            if (crosshair != null) crosshair.Delete(true);
            if (customizationMenu != null) customizationMenu.Delete();
        }

        public void UISimulate(Client player)
        {
            // Cutomization menu
            if (EnableCustomizationSV > 0 && Input.Pressed(InputButton.Menu) && AttachmentCategories != null)
            {
                if (customizationMenu == null)
                {
                    customizationMenu = new CustomizationMenu();
                    customizationMenu.Parent = Local.Hud;
                }
                else
                {
                    customizationMenu.Delete();
                    customizationMenu = null;
                }
            }

            IsCustomizing = customizationMenu != null;
        }
    }
}
