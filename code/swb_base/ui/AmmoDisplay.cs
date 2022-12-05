using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SWB_Base.UI;

public class AmmoDisplay : Panel
{
    Panel ammoWrapper;
    Panel iconWrapper;
    Panel fireModeWrapper;

    Image weaponIcon;
    Image semiFireIcon;
    Image autoFireIcon;

    Label clipLabel;
    Label reserveLabel;

    Color reserveColor = new Color32(200, 200, 200).ToColor();
    Color emptyColor = new Color32(175, 175, 175, 200).ToColor();

    public AmmoDisplay(UISettings uiSettings)
    {
        StyleSheet.Load("/swb_base/ui/AmmoDisplay.scss");

        if (uiSettings.ShowAmmoCount)
        {
            ammoWrapper = Add.Panel("ammoWrapper");
            clipLabel = ammoWrapper.Add.Label("", "clipLabel");
            reserveLabel = ammoWrapper.Add.Label("", "reserveLabel");
        }

        if (uiSettings.ShowWeaponIcon)
        {
            iconWrapper = Add.Panel("iconWrapper");
            weaponIcon = iconWrapper.Add.Image("", "weaponIcon");
        }

        if (uiSettings.ShowFireMode)
        {
            fireModeWrapper = iconWrapper.Add.Panel("fireModeWrapper");
            semiFireIcon = fireModeWrapper.Add.Image("/materials/swb/bullets/bullets_1.png", "fireModeIcon");
            autoFireIcon = fireModeWrapper.Add.Image("/materials/swb/bullets/bullets_3.png", "fireModeIcon");
        }
    }

    public override void Tick()
    {
        var player = Local.Pawn as PlayerBase;
        if (player == null) return;

        var weapon = player.ActiveChild as WeaponBase;
        bool isValidWeapon = weapon != null;

        SetClass("hideAmmoDisplay", !isValidWeapon);

        if (!isValidWeapon) return;

        if (ammoWrapper != null)
        {
            var hasClipSize = weapon.Primary.ClipSize > 0;
            var reserveAmmo = Math.Min(player.AmmoCount(weapon.Primary.AmmoType), 999);

            if (weapon.Primary.InfiniteAmmo != InfiniteAmmoType.clip)
            {
                var clipAmmo = hasClipSize ? weapon.Primary.Ammo : reserveAmmo;
                clipAmmo = Math.Min(clipAmmo, 999);

                clipLabel.SetText(clipAmmo.ToString());
                clipLabel.Style.FontColor = clipAmmo == 0 ? emptyColor : Color.White;
            }
            else
            {
                clipLabel.SetText("∞");
                clipLabel.Style.FontColor = Color.White;
            }

            if (hasClipSize)
            {
                if (weapon.Primary.InfiniteAmmo == 0)
                {
                    reserveLabel.SetText("|" + reserveAmmo);
                    reserveLabel.Style.FontColor = reserveAmmo == 0 ? emptyColor : reserveColor;
                }
                else
                {
                    reserveLabel.SetText("|∞");
                    reserveLabel.Style.FontColor = reserveColor;
                }
            }
        }

        if (iconWrapper != null)
        {
            weaponIcon.SetTexture(weapon.Icon);
        }

        if (fireModeWrapper != null)
        {
            var isSemiFire = weapon.Primary.FiringType == FiringType.semi;

            semiFireIcon.Style.Opacity = isSemiFire ? 1 : 0.25f;
            autoFireIcon.Style.Opacity = isSemiFire ? 0.25f : 1;
        }
    }
}
