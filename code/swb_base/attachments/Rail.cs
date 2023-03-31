
namespace SWB_Base.Attachments;

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

    public override StatModifier StatModifier { get; set; } = new StatModifier
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
    public override string IconPath => "attachments/swb/rail/rail_quad/ui/icon.png";
    public override string ModelPath => "attachments/swb/rail/rail_quad/w_rail_quad.vmdl";
}

public class SingleRail : Rail
{
    public override string Name => "UTG Single-Rail";
    public override string IconPath => "attachments/swb/rail/rail_single/ui/icon.png";
    public override string ModelPath => "attachments/swb/rail/rail_single/w_rail_single.vmdl";
}


public class SideTopRail : Rail
{
    public override string Name => "Leapers UTG";
    public override string IconPath => "attachments/swb/rail/side_rail_top/ui/icon.png";
    public override string ModelPath => "attachments/swb/rail/side_rail_top/w_side_rail_top.vmdl";
}
