/* 
 * Weapon base attachments support
*/

using Sandbox;

namespace SWB_Base
{
    public class AttachmentModel : ModelEntity
    {
        public AttachmentModel() { }

        public AttachmentModel(bool isViewModel)
        {
            EnableViewmodelRendering = isViewModel;
            EnableHideInFirstPerson = !isViewModel;
        }

        public override void PostCameraSetup(ref CameraSetup camSetup) { }
    }
}
