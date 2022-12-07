using Sandbox;

namespace SWB_Base;

public class SpectateCamera : CameraMode
{
    public SpectateCamera()
    {
    }

    public SpectateCamera(PlayerBase player) : base(player)
    {
    }

    public override void UpdateCamera()
    {
        if (Local.Pawn is not Player player)
            return;

        Camera.Position = player.EyePosition;

        //
    }
}
