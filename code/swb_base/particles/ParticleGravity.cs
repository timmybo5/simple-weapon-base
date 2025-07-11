namespace SWB.Base.Particles;

public enum ParticleGravityDirection
{
	Up,
	Down,
	Left,
	Right
}

[Group( "SWB Particles" )]
public class ParticleGravity : Component
{
	[Property] public float Multiplier { get; set; } = 1f;
	[Property] public ParticleGravityDirection Direction { get; set; }
	ParticleEffect effect { get; set; }
	Vector3 dir { get; set; }

	protected override void OnAwake()
	{
		effect = GetComponent<ParticleEffect>();

		dir = Direction switch
		{
			ParticleGravityDirection.Up => Vector3.Up,
			ParticleGravityDirection.Down => Vector3.Down,
			ParticleGravityDirection.Left => Vector3.Left,
			ParticleGravityDirection.Right => Vector3.Right,
			_ => Vector3.Zero,
		};
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		var gravity = dir * 9.81f * Time.Delta * Multiplier;
		var forceDirectionMod = gravity;

		// Convert gravity to local space
		if ( effect.ForceSpace == ParticleEffect.SimulationSpace.Local )
			forceDirectionMod = effect.WorldRotation.Inverse * gravity;

		effect.ForceDirection += forceDirectionMod;
	}
}
