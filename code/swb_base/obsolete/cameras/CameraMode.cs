using Sandbox;

namespace SWB_Base;

public class CameraMode : EntityComponent
{
    public virtual void UpdateCamera()
    {
        Host.AssertClient();
    }
}
