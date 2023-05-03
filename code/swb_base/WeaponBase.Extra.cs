using Sandbox;

/* 
 * Extra features such as tucking, barrel heat, and burst fire mechanics
*/

namespace SWB_Base;

public partial class WeaponBase
{
    // Tucking
    public virtual float GetTuckDist()
    {
        if (TuckRange == -1)
            return -1;

        if (Owner is not ISWBPlayer player) return -1;

        var pos = player.EyePosition;
        var forward = player.EyeRotation.Forward;
        var trace = Trace.Ray(pos, pos + forward * TuckRange)
            .WithTag("solid")
            .Ignore(this)
            .Ignore(Owner)
            .Run();

        if (trace.Entity == null)
            return -1;

        return trace.Distance;
    }

    public bool ShouldTuck(float dist)
    {
        return dist != -1;
    }

    public bool ShouldTuck()
    {
        return GetTuckDist() != -1;
    }

    public bool ShouldTuck(out float dist)
    {
        dist = GetTuckDist();
        return dist != -1;
    }

    // Barrel heat
    public void AddBarrelHeat()
    {
        barrelHeat += 1;
    }

    [GameEvent.Tick.Server]
    public void BarrelHeatCheck()
    {
        if (timeSinceFired > 3)
        {
            barrelHeat = 0;
        }
    }

    // Burst Fire
    public virtual void ResetBurstFireCount(ClipInfo clipInfo, string inputButton)
    {
        if (clipInfo == null || clipInfo.FiringType != FiringType.burst) return;

        if (Input.Released(inputButton))
        {
            burstCount = 0;
        }
    }
}
