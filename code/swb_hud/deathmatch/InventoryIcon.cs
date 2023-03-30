
using Sandbox;
using Sandbox.UI;
using SWB_Base;

namespace SWB_HUD;

class InventoryIcon : Panel
{
    public WeaponBase Weapon;
    public Image Icon;

    public InventoryIcon(WeaponBase weapon)
    {
        Weapon = weapon;
        AddChild(out Icon, "icon");
        Icon.SetTexture(Weapon.Icon);
    }

    internal void TickSelection(WeaponBase selectedWeapon)
    {
        SetClass("active", selectedWeapon == Weapon);
        SetClass("empty", !Weapon?.IsUsable() ?? true);
    }

    public override void Tick()
    {
        base.Tick();

        if (!Weapon.IsValid() || Weapon.Owner != Game.LocalPawn)
            Delete();
    }
}
