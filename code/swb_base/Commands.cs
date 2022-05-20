using Sandbox;

namespace SWB_Base
{
    internal class Commands
    {
        [ConCmd.Server("swb_editor_model", Help = "Opens the model editor")]
        public static void OpenModelEditor()
        {
            Client client = ConsoleSystem.Caller;

            if (client != null)
            {
                var player = client.Pawn as PlayerBase;
                player.ToggleModelEditor();
            }
        }

        [ConCmd.Server("swb_editor_attachment", Help = "Opens the attachment editor")]
        public static void OpenAttachmentEditor()
        {
            Client client = ConsoleSystem.Caller;

            if (client != null)
            {
                var player = client.Pawn as PlayerBase;
                player.ToggleAttachmentEditor();
            }
        }

        [ConCmd.Server("swb_attachment_equip", Help = "Equips an attachment by name")]
        public static void EquipAttachmentCMD(string name, bool enabled)
        {
            Client client = ConsoleSystem.Caller;

            if (client != null)
            {
                var player = client.Pawn as PlayerBase;

                if (player.ActiveChild is not WeaponBase activeWeapon) return;

                if (enabled && activeWeapon.GetAttachment(name) != null)
                {
                    // Attach
                    _ = activeWeapon.EquipAttachmentSV(name);
                }
                else if (!enabled && activeWeapon.GetActiveAttachment(name) != null)
                {
                    // Detach
                    activeWeapon.UnequipAttachmentSV(name);
                }
            }
        }
    }
}
