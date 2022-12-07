/* 
 * Weapon base attachment model
*/

using Sandbox;

namespace SWB_Base;

public class AttachmentModel : AnimatedEntity
{
    public AttachmentModel() { }

    public AttachmentModel(bool isViewModel)
    {
        EnableViewmodelRendering = isViewModel;
        EnableHideInFirstPerson = !isViewModel;
    }
}
