using Sandbox;

namespace SWB_Base;

public class ThirdPersonCamera : CameraMode
{
    public override void UpdateCamera()
    {
        base.UpdateCamera();

        if (Entity is not PlayerBase player) return;

        Camera.ZNear = 1f;
        Camera.ZFar = 25000.0f;
        Camera.Rotation = player.ViewAngles.ToRotation();

        Camera.FieldOfView = Game.Preferences.FieldOfView;
        Camera.FirstPersonViewer = null;

        Vector3 targetPos;
        var center = player.Position + Vector3.Up * 64;

        var pos = center;
        var rot = Rotation.FromAxis(Vector3.Up, -16) * Camera.Rotation;

        float distance = 100.0f * player.Scale;
        targetPos = pos + rot.Right * ((player.CollisionBounds.Mins.x + 64) * player.Scale);
        targetPos += rot.Forward * -distance;

        var tr = Trace.Ray(pos, targetPos)
            .WithAnyTags("solid")
            .Ignore(player)
            .Radius(8)
            .Run();

        Camera.Position = tr.EndPosition;
    }
}
