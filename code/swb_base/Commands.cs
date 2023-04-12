using Sandbox;
using SWB_Base;

internal class Commands
{
    [ConCmd.Server("swb_attachment_equip", Help = "Equips an attachment by name")]
    public static void EquipAttachmentCMD(string name, bool enabled)
    {
        var client = ConsoleSystem.Caller;

        if (client != null)
        {
            var player = client.Pawn as ISWBPlayer;

            if (player.ActiveChild is not WeaponBase activeWeapon)
                return;

            if (enabled && activeWeapon.GetAttachment(name) != null)
            {
                // Attach
                activeWeapon.EquipAttachmentSV(name);
            }
            else if (!enabled && activeWeapon.GetActiveAttachment(name) != null)
            {
                // Detach
                activeWeapon.UnequipAttachmentSV(name);
            }
        }
    }
}
