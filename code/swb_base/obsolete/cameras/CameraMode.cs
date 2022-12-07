using Sandbox;

namespace SWB_Base;

public partial class CameraMode : BaseNetworkable
{
    [Net] public PlayerBase player { get; private set; }

    public CameraMode()
    {
    }

    public CameraMode(PlayerBase player)
    {
        this.player = player;
    }

    public virtual void UpdateCamera()
    {
        Host.AssertClient();
    }
}
