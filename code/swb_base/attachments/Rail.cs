
namespace SWB_Base.Attachments
{
    public class Rail : OffsetAttachment
    {
        public override string Name => "Rail";
        public override string Description => "Used by other attachments to be able to attach to the weapon.";
        public override string[] Positives => new string[]
        {
        };

        public override string[] Negatives => new string[]
        {
        };

        public override StatModifier StatModifier => new StatModifier
        {
        };

        public override void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel)
        {
        }

        public override void OnUnequip(WeaponBase weapon)
        {
        }
    }

    public class QuadRail : Rail
    {
        public override string Name => "UTG Quad-Rail";
        public override string IconPath => "attachments/swb/rail/rail_barrel/ui/icon.png";
        public override string ModelPath => "attachments/swb/rail/rail_barrel/w_rail_barrel.vmdl";
    }
}
