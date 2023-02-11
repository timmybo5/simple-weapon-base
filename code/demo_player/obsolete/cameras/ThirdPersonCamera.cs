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
        Camera.FieldOfView = Game.Preferences.FieldOfView;
        Camera.FirstPersonViewer = null;
        Camera.Rotation = player.ViewAngles.ToRotation();
        Camera.Position = GetPosition(player);

        // Look at crosshair pos (Smooth on CL)
        player.EyePosition = Camera.Position;
        //player.EyeRotation = GetEyeRotation(player);
    }

    public static Vector3 GetPosition(PlayerBase player)
    {
        Vector3 targetPos;
        var center = player.Position + Vector3.Up * 64;

        var pos = center;
        var rot = player.ViewAngles.ToRotation();
        rot *= Rotation.FromAxis(Vector3.Up, -16);

        float distance = 100.0f * player.Scale;
        targetPos = pos + rot.Right * ((player.CollisionBounds.Mins.x + 64) * player.Scale);
        targetPos += rot.Forward * -distance;

        // Prevent clipping inside walls
        var tr = Trace.Ray(pos, targetPos)
            .WithAnyTags("solid")
            .Ignore(player)
            .Radius(8)
            .Run();

        return tr.EndPosition;
    }

    public static Rotation GetEyeRotation(PlayerBase player)
    {
        var camPos = Game.IsClient ? Camera.Position : GetPosition(player);

        var rot = player.ViewAngles.ToRotation();
        var camTr = Trace.Ray(camPos + rot.Forward * 20, camPos + rot.Forward * 9999)
            .WithAnyTags("solid")
            .Ignore(player)
            .Radius(8)
            .Run();

        var posDiff = camTr.EndPosition - player.EyePosition;
        var newAng = Vector3.VectorAngle(posDiff);
        var newRot = newAng.ToRotation();

        // Cam
        //DebugOverlay.Line(camPos, camTr.EndPosition, color: Color.Blue);

        // Eye
        //DebugOverlay.Line(player.EyePosition, player.EyePosition + newRot.Forward * 9999, color: Color.Red);

        return newRot;
    }
}
