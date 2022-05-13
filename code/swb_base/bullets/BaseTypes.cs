
namespace SWB_Base.Bullets
{
    // 9×19mm Parabellum
    public class PistolBullet : PhysicalBulletBase
    {
        public override float Mass => 7.45f;
        public override float Drag => 0.1f;
        public override float Velocity => 360f;
    }

    // .50 Action Express
    public class DeagleBullet : PhysicalBulletBase
    {
        public override float Mass => 19f;
        public override float Drag => 0.15f;
        public override float Velocity => 470f;
    }

    // 12 gauge
    public class ShotgunBullet : PhysicalBulletBase
    {
        public override float Mass => 30f;
        public override float Drag => 0.5f;
        public override float Velocity => 115f;
    }

    // .45 ACP
    public class SMGBullet : PhysicalBulletBase
    {
        public override float Mass => 15f;
        public override float Drag => 0.15f;
        public override float Velocity => 255f;
    }

    // 7.62×51mm NATO
    public class RifleBullet : PhysicalBulletBase
    {
        public override float Mass => 11f;
        public override float Drag => 0.2f;
        public override float Velocity => 600f;
    }

    // .338 Winchester Magnum
    public class SniperBullet : PhysicalBulletBase
    {
        public override float Mass => 16f;
        public override float Drag => 0.15f;
        public override float Velocity => 750f;
    }
}
