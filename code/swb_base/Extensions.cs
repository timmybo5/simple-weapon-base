using System.Threading.Tasks;

namespace SWB.Base;

public static class Extensions
{
	public static async void PlayUntilFinished( this SceneParticles particles, TaskSource source )
	{
		try
		{
			while ( !particles.Finished )
			{
				await source.Frame();
				particles.Simulate( Time.Delta );
			}
		}
		catch ( TaskCanceledException )
		{
			// Do nothing.
		}

		particles.Delete();
	}
}
