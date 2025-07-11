using System;

namespace SWB.Base;

public static class ParticleExtensions
{
	[Obsolete]
	public static async void PlayUntilFinished( this SceneParticles particles, TaskSource source, Action<SceneParticles> OnFrame = null )
	{
	}
}
