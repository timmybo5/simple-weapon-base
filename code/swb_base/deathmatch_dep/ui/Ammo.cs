
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB_Base;

public class Ammo : Panel
{
    public Label Weapon;
    public Label Inventory;

    public Ammo()
    {
        Weapon = Add.Label( "100", "weapon" );
        Inventory = Add.Label( "100", "inventory" );
    }

    public override void Tick()
    {
        var player = Local.Pawn;
        if ( player == null ) return;

        var weapon = player.ActiveChild as WeaponBase;
        SetClass( "active", weapon != null );

        if ( weapon == null || weapon is WeaponBaseMelee )
        {
            Weapon.Text = "";
            Inventory.Text = "";
            return;
        };

        var inv = weapon.AvailableAmmo();

        if ( weapon.Primary.ClipSize != -1 )
        {
            Weapon.Text = $"{weapon.Primary.Ammo}";
            Inventory.Text = $" / {inv}";
        }
        else
        {
            Weapon.Text = "";
            Inventory.Text = $"{inv}";
        }

        Inventory.SetClass( "active", inv >= 0 );
    }
}
