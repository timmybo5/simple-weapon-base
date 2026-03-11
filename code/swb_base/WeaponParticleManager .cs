using System.Collections.Generic;

namespace SWB.Base;

/// <summary>
/// Attach this component somewhere in the root of your scene.
/// Manages particle spawning
/// </summary>
[Group( "SWB" )]
[Title( "Weapon Particle Settings" )]
public class WeaponParticleManager : Component
{
	public static WeaponParticleManager Instance { get; private set; }

	/// <summary>Max total decals allowed at the same time</summary>
	[Sync( SyncFlags.FromHost ), Property] public int MaxDecals { get; set; } = 30;

	/// <summary>Max total eject particles allowed at the same time</summary>
	[Sync( SyncFlags.FromHost ), Property] public int MaxEject { get; set; } = 180;

	private Queue<GameObject> ejectParticles = new();
	private Queue<GameObject> decals = new();

	protected override void OnDestroy()
	{
		if ( Instance == this )
			Instance = null;
	}

	protected override void OnAwake()
	{
		Instance = this;
	}

	private void AddToQueue( GameObject particle, Queue<GameObject> queue, int max )
	{
		// Add to queue
		queue.Enqueue( particle );

		if ( queue.Count > max )
		{
			var oldest = queue.Dequeue();
			if ( oldest.IsValid() )
				oldest.Destroy();
		}
	}

	public void AddEject( GameObject particle, bool parent = true )
	{
		AddToQueue( particle, ejectParticles, MaxEject );

		if ( parent )
			particle.SetParent( this.GameObject );
	}

	public void AddDecal( GameObject decal, bool parent = true )
	{
		AddToQueue( decal, decals, MaxDecals );

		if ( parent )
			decal.SetParent( this.GameObject );
	}
}
