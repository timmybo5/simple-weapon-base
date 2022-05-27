using Sandbox;

namespace SWB_Base
{
    /// <summary>
    /// A utilty class. Add as a child to your pickupable entities to expand
    /// the trigger boundaries. They'll be able to pick up the parent entity
    /// using these bounds.
    /// </summary>
    public class PickupBase : ModelEntity
    {
        public override void Spawn()
        {
            base.Spawn();

            SetTriggerSize();

            Transmit = TransmitType.Never;
        }

        /// <summary>
        /// Set the trigger radius. Default is 16.
        /// </summary>
        public void SetTriggerSize(float radius = 16f)
        {
            SetupPhysicsFromCapsule(PhysicsMotionType.Keyframed, new Capsule(Vector3.Zero, Vector3.One * 0.1f, radius));
            CollisionGroup = CollisionGroup.Trigger;
        }
    }
}
