using System;
using Sandbox;

namespace SWB_Base;

partial class ScreenUtil
{
    [ClientRpc]
    public static void ShakeRPC(float length = 0, float delay = 0, float size = 0, float rotation = 0)
    {
        Shake(length, delay, size, rotation);
    }

    public static void Shake(float length = 0, float delay = 0, float size = 0, float rotation = 0)
    {
        if (Game.LocalPawn is ISWBPlayer player)
        {
            var screenShake = new ScreenShakeStruct
            {
                Length = length,
                Delay = delay,
                Size = size,
                Rotation = rotation
            };

            player.ScreenShake(screenShake);
        }
    }

    public static void Shake(To to, float length = 0, float delay = 0, float size = 0, float rotation = 0)
    {
        ShakeRPC(to, length, delay, size, rotation);
    }

    public static void Shake(To to, ScreenShake screenShake)
    {
        if (screenShake != null)
            Shake(to, screenShake.Length, screenShake.Delay, screenShake.Size, screenShake.Rotation);
    }

    public static void Shake(ScreenShake screenShake)
    {
        if (screenShake != null)
            Shake(screenShake.Length, screenShake.Delay, screenShake.Size, screenShake.Rotation);
    }

    public static void ShakeAt(Vector3 origin, float radius = 0, float delay = 0, float speed = 0, float size = 0, float rotation = 0)
    {
        var objects = Entity.FindInSphere(origin, radius);

        foreach (var obj in objects)
        {
            // Player check
            if (!obj.IsValid() || obj is not ISWBPlayer ply)
                continue;

            // Distance check
            var targetPos = ply.Position;
            var dist = Vector3.DistanceBetween(origin, targetPos);
            if (dist > radius)
                continue;

            // Intensity calculation
            var distanceMul = 1.0f - Math.Clamp(dist / radius, 0.0f, 0.75f);
            rotation *= distanceMul;
            size *= distanceMul;

            ShakeRPC(To.Single(ply.Client), delay, speed, size, rotation);
        }
    }

    public static void ShakeAt(Vector3 position, float radius, ScreenShake screenShake)
    {
        if (screenShake != null)
            ShakeAt(position, radius, screenShake.Length, screenShake.Delay, screenShake.Size, screenShake.Rotation);
    }
}
