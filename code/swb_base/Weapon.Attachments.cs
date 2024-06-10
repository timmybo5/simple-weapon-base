using SWB.Base.Attachments;

namespace SWB.Base;

public partial class Weapon
{
	public Attachment GetActiveAttachmentForCategory( AttachmentCategory category )
	{
		foreach ( var attachment in Attachments )
		{
			if ( attachment.Category == category && attachment.Equipped )
				return attachment;
		}

		return null;
	}
}
