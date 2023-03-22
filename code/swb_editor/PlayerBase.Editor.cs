namespace SWB_Base;

public partial class PlayerBase
{
	public bool IsEditingWeapon
	{
		get
		{
			return attachmentEditor != null || modelEditor != null;
		}
	}
}
