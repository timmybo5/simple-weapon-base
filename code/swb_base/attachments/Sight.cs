
namespace SWB_Base.Attachments
{
    public class Sight : OffsetAttachment
    {
        public override string Name => "Sight";
        public override string Description => "An optical sight that allows the user to look through a partially reflecting glass element and see an illuminated projection of an aiming point or some other image superimposed on the field of view.";
        public override string[] Positives => new string[]
        {
            "Precision sight picture"
        };

        public override string[] Negatives => new string[]
        {
        };

        public override StatModifier StatModifier => new StatModifier
        {
            Spread = -0.05f,
        };

        public AngPos ZoomAnimData; // New sight zoom offset

        public override void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel)
        {
        }

        public override void OnUnequip(WeaponBase weapon)
        {
        }
    }

    public class ReflexSight : Sight
    {
        public override string Name => "Walther MRS Reflex";
        public override string IconPath => "attachments/swb/sight/reflex/ui/icon.png";
        public override string ModelPath => "attachments/swb/sight/reflex/w_reflex_sight.vmdl";
    }
}
