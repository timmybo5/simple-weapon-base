using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.UI;
using SWB_Base;

namespace Deathmatch.Hud;

/// <summary>
/// The main inventory panel, top left of the screen.
/// </summary>
public class InventoryBar : Panel
{
    List<InventoryColumn> columns = new();
    List<WeaponBase> Weapons = new();

    public bool IsOpen;
    WeaponBase SelectedWeapon;

    public InventoryBar()
    {
        StyleSheet.Load("deathmatch_hud/ui/InventoryBar.scss");

        for (int i = 0; i < 6; i++)
        {
            var icon = new InventoryColumn(i, this);
            columns.Add(icon);
        }
    }

    public override void Tick()
    {
        base.Tick();

        SetClass("active", IsOpen);

        if (Game.LocalPawn is not PlayerBase player) return;

        Weapons.Clear();
        Weapons.AddRange(player.Children.Select(x => x as WeaponBase).Where(x => x.IsValid() && x.IsUsable()));

        foreach (var weapon in Weapons)
        {
            columns[weapon.Bucket].UpdateWeapon(weapon);
        }
    }

    /// <summary>
    /// IClientInput implementation, calls during the client input build.
    /// You can both read and write to input, to affect what happens down the line.
    /// </summary>
    [Event.Client.BuildInput]
    public void ProcessClientInput()
    {
        var player = Game.LocalPawn as PlayerBase;
        if (player == null) return;

        bool wantOpen = IsOpen;

        // If we're not open, maybe this input has something that will 
        // make us want to start being open?
        wantOpen = wantOpen || Input.MouseWheel != 0;
        wantOpen = wantOpen || Input.Pressed(InputButton.Slot1);
        wantOpen = wantOpen || Input.Pressed(InputButton.Slot2);
        wantOpen = wantOpen || Input.Pressed(InputButton.Slot3);
        wantOpen = wantOpen || Input.Pressed(InputButton.Slot4);
        wantOpen = wantOpen || Input.Pressed(InputButton.Slot5);
        wantOpen = wantOpen || Input.Pressed(InputButton.Slot6);

        if (Weapons.Count == 0)
        {
            IsOpen = false;
            return;
        }

        // We're not open, but we want to be
        if (IsOpen != wantOpen)
        {
            SelectedWeapon = player.ActiveChild as WeaponBase;
            IsOpen = true;
        }

        // Not open fuck it off
        if (!IsOpen) return;

        //
        // Fire pressed when we're open - select the weapon and close.
        //
        if (Input.Down(InputButton.PrimaryAttack))
        {
            Input.SuppressButton(InputButton.PrimaryAttack);
            player.ActiveChildInput = SelectedWeapon;
            IsOpen = false;
            Sound.FromScreen("dm.ui_select");
            return;
        }

        // get our current index
        var oldSelected = SelectedWeapon;
        int SelectedIndex = Weapons.IndexOf(SelectedWeapon);
        SelectedIndex = SlotPressInput(SelectedIndex);

        // forward if mouse wheel was pressed
        SelectedIndex += Input.MouseWheel;
        SelectedIndex = SelectedIndex.UnsignedMod(Weapons.Count);

        SelectedWeapon = Weapons[SelectedIndex];

        for (int i = 0; i < 6; i++)
        {
            columns[i].TickSelection(SelectedWeapon);
        }

        Input.MouseWheel = 0;

        if (oldSelected != SelectedWeapon)
        {
            Sound.FromScreen("dm.ui_tap");
        }
    }

    int SlotPressInput(int SelectedIndex)
    {
        var columninput = -1;

        if (Input.Pressed(InputButton.Slot1)) columninput = 0;
        if (Input.Pressed(InputButton.Slot2)) columninput = 1;
        if (Input.Pressed(InputButton.Slot3)) columninput = 2;
        if (Input.Pressed(InputButton.Slot4)) columninput = 3;
        if (Input.Pressed(InputButton.Slot5)) columninput = 4;
        if (Input.Pressed(InputButton.Slot6)) columninput = 5;

        if (columninput == -1) return SelectedIndex;

        if (SelectedWeapon.IsValid() && SelectedWeapon.Bucket == columninput)
        {
            return NextInBucket();
        }

        // Are we already selecting a weapon with this column?
        var firstOfColumn = Weapons.Where(x => x.Bucket == columninput).OrderBy(x => x.BucketWeight).FirstOrDefault();
        if (firstOfColumn == null)
        {
            // DOOP sound
            return SelectedIndex;
        }

        return Weapons.IndexOf(firstOfColumn);
    }

    int NextInBucket()
    {
        Assert.NotNull(SelectedWeapon);

        WeaponBase first = null;
        WeaponBase prev = null;
        foreach (var weapon in Weapons.Where(x => x.Bucket == SelectedWeapon.Bucket).OrderBy(x => x.BucketWeight))
        {
            if (first == null) first = weapon;
            if (prev == SelectedWeapon) return Weapons.IndexOf(weapon);
            prev = weapon;
        }

        return Weapons.IndexOf(first);
    }
}
