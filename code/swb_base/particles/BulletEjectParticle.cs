using SWB.Shared;

namespace SWB.Base.Particles;

[Group( "SWB Particles" )]
public class BulletEjectParticle : Component
{
	public IPlayerBase Owner { get; set; }
	public float MoveDirection { get; set; } = 1f;

	protected override void OnStart()
	{
		if ( Owner is null ) return;
		var vel = Owner.Velocity;
		var rot = Owner.EyeAngles.ToRotation();
		var moveDir = rot.Right * MoveDirection;
		var lateralSpeed = vel.Dot( moveDir );
		var effect = GetComponent<ParticleEffect>();
		var rndOffset = Game.Random.Float(-10, 10);
		effect.ForceDirection += effect.WorldRotation.Left * rndOffset;

		// Only push shells further in their ejection direction when the player moves that way.
		if ( lateralSpeed > 0f )
		{
			var lateralForce = moveDir * lateralSpeed * 0.2f;

			if ( effect.ForceSpace == ParticleEffect.SimulationSpace.Local )
				lateralForce = effect.WorldRotation.Inverse * lateralForce;

			effect.ForceDirection += lateralForce;
		}
	}
}
