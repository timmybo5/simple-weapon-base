using Sandbox;

namespace SWB_Base;

public class SpectateCamera : CameraMode
{
    public override void UpdateCamera()
    {
        base.UpdateCamera();

        if (Entity is not PlayerBase player) return;

        Camera.Position = player.EyePosition;

        //
    }
}
