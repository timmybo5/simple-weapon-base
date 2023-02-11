using Sandbox;

namespace SWB_Base;

public class DeathCamera : CameraMode
{
    Vector3 FocusPoint;

    public override void UpdateCamera()
    {
        base.UpdateCamera();

        if (Entity is not PlayerBase player) return;

        FocusPoint = Vector3.Lerp(FocusPoint, GetSpectatePoint(player), Time.Delta * 10.0f);

        Camera.ZNear = 1f;
        Camera.ZFar = 25000.0f;
        Camera.Position = FocusPoint + GetViewOffset(player);
        Camera.Rotation = player.Rotation * Rotation.FromYaw(-180) * Rotation.FromPitch(20);
        Camera.FieldOfView = Game.Preferences.FieldOfView;
        Camera.FirstPersonViewer = null;
    }

    private Vector3 GetSpectatePoint(PlayerBase player)
    {
        if (player.Corpse.IsValid())
        {
            return player.Corpse.PhysicsGroup.MassCenter;
        }

        return player.Position;
    }

    private Vector3 GetViewOffset(PlayerBase player)
    {
        return player.Rotation.Forward * (120 * 1) + Vector3.Up * (80 * 1);
    }
}
