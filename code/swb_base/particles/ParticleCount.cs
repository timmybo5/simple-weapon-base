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

		var rnd = new Random();
		var max = rnd.Next( Min, Max + 1 );
		effect.MaxParticles = max;
	}
}
