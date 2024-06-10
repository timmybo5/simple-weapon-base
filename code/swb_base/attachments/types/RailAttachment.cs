namespace SWB.Base.Attachments;

public abstract class RailAttachment : Attachment
{
	public override string Name => "Rail";
	public override AttachmentCategory Category => AttachmentCategory.Rail;
	public override string Description => "Used by other attachments to be able to attach to the weapon.";
	public override bool Hide { get; set; } = true;

	public override void OnEquip()
	{
	}

	public override void OnUnequip()
	{
	}
}
