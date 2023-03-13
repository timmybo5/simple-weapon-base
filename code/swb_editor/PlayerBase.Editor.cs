namespace SWB_Base;

public partial class PlayerBase
{
    public bool IsLookingAtGun
    {
        get
        {
            return attachmentEditor != null || modelEditor != null;
        }
    }
}
