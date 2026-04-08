using System;

namespace SWB.Base.Particles;

[Group( "SWB Particles" )]
public class ParticleCount : Component
{
	[Property] public int Min { get; set; }
	[Property] public int Max { get; set; }

	protected override void OnAwake()
	{
		var effect = GetComponent<ParticleEffect>();
		var max = Random.Shared.Next( Min, Max + 1 );
		effect.MaxParticles = max;
	}
}
