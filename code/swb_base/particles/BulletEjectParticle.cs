using SWB.Shared;

namespace SWB.Base.Particles;

[Group( "SWB Particles" )]
public class BulletEjectParticle : Component
{
	public IPlayerBase Owner { get; set; }

	protected override void OnStart()
	{
		if ( Owner is null ) return;
		var vel = Owner.Velocity;
		var right = Owner.Camera.WorldRotation.Right;
		var lateralSpeed = vel.Dot( right );

		// Only push shells further right when the player is actually moving right.
		if ( lateralSpeed > 0f )
		{
			var effect = GetComponent<ParticleEffect>();
			var lateralForce = right * lateralSpeed * 0.2f;

			if ( effect.ForceSpace == ParticleEffect.SimulationSpace.Local )
				lateralForce = effect.WorldRotation.Inverse * lateralForce;

			effect.ForceDirection += lateralForce;
		}
	}
}
