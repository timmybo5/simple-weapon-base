using Sandbox;

namespace SWB_Player;

public class CameraMode : EntityComponent
{
    public virtual void UpdateCamera()
    {
        Game.AssertClient();
    }
}
