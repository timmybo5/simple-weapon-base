using Sandbox;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base
{
    public partial class WeaponBase
    {
        // Tucking
        public virtual float GetTuckDist()
        {
            if (TuckRange == -1)
                return -1;

            var player = Owner as Player;
            if (player == null) return -1;

            var pos = player.EyePosition;
            var forward = Owner.EyeRotation.Forward;
            var trace = Trace.Ray(pos, pos + forward * TuckRange)
                .Ignore(this)
                .Ignore(player)
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

        [Event.Tick.Server]
        public void BarrelHeatCheck()
        {
            if (TimeSinceFired > 3)
            {
                barrelHeat = 0;
            }
        }

        // Burst Fire
        public virtual void ResetBurstFireCount(ClipInfo clipInfo, InputButton inputButton)
        {
            if (clipInfo == null || clipInfo.FiringType != FiringType.burst) return;

            if (Input.Released(inputButton))
            {
                burstCount = 0;
            }
        }
    }
}
