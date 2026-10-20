using System.Collections.Generic;

namespace SWB.Base;

/// <summary>
/// Collects the impact/tracer effects of every pellet (and penetration/ricochet segment) of one shot
/// and sends them together. 
/// This avoids a shotgun blast sending one network message per pellet.
/// </summary>
public partial class Weapon
{
	struct PendingHitEffect
	{
		public HitScanBulletInfo Info;
		public bool HasImpact;
		public bool HasTracer;
		public Vector3? TracerStart;
		public Vector3 Position;
		public Vector3 Normal;
		public Surface Surface;
	}

	const byte HitFlagImpact = 1;
	const byte HitFlagTracer = 2;
	const byte HitFlagTracerStart = 4;

	readonly List<PendingHitEffect> pendingHitEffects = new();

	/// <summary>While true, hitscan bullets queue their effects with <see cref="QueueHitEffect"/> instead of sending them right away</summary>
	public bool IsBatchingHitEffects { get; private set; }

	protected void BeginHitEffectBatch()
	{
		pendingHitEffects.Clear();
		IsBatchingHitEffects = true;
	}

	public void QueueHitEffect( HitScanBulletInfo info, bool hasImpact, bool hasTracer, Vector3? tracerStart, Vector3 position, Vector3 normal, Surface surface )
	{
		pendingHitEffects.Add( new PendingHitEffect
		{
			Info = info,
			HasImpact = hasImpact,
			HasTracer = hasTracer,
			TracerStart = tracerStart,
			Position = position,
			Normal = normal,
			Surface = surface,
		} );
	}

	protected void FlushHitEffects( bool isPrimary )
	{
		IsBatchingHitEffects = false;

		var count = pendingHitEffects.Count;
		if ( count == 0 ) return;

		// Single hit (every normal bullet): exactly the message it always sent
		if ( count == 1 )
		{
			var hit = pendingHitEffects[0];
			hit.Info?.SpawnEffects( this, isPrimary, hit.HasImpact, hit.HasTracer, hit.TracerStart, hit.Position, hit.Normal, hit.Surface?.SoundCollection.Bullet, hit.Surface?.PrefabCollection.BulletImpact );
			pendingHitEffects.Clear();
			return;
		}

		var positions = new Vector3[count];
		var normals = new Vector3[count];
		var tracerStarts = new Vector3[count];
		var flags = new byte[count];
		var surfacePaths = new string[count];

		for ( var i = 0; i < count; i++ )
		{
			var hit = pendingHitEffects[i];
			positions[i] = hit.Position;
			normals[i] = hit.Normal;
			tracerStarts[i] = hit.TracerStart ?? Vector3.Zero;
			surfacePaths[i] = hit.Surface?.ResourcePath ?? "";

			byte flag = 0;
			if ( hit.HasImpact ) flag |= HitFlagImpact;
			if ( hit.HasTracer ) flag |= HitFlagTracer;
			if ( hit.TracerStart.HasValue ) flag |= HitFlagTracerStart;
			flags[i] = flag;
		}

		pendingHitEffects.Clear();
		SpawnHitEffectsBatch( isPrimary, positions, normals, tracerStarts, flags, surfacePaths );
	}

	[Rpc.Broadcast( NetFlags.Unreliable )]
	public void SpawnHitEffectsBatch( bool isPrimary, Vector3[] positions, Vector3[] normals, Vector3[] tracerStarts, byte[] flags, string[] surfacePaths )
	{
		if ( !IsValid || Application.IsDedicatedServer ) return;
		if ( positions is null || normals is null || tracerStarts is null || flags is null || surfacePaths is null ) return;

		var count = positions.Length;
		if ( normals.Length != count || tracerStarts.Length != count || flags.Length != count || surfacePaths.Length != count ) return;

		var tracerInfo = GetShootInfo( isPrimary )?.BulletType as HitScanBulletInfo;

		for ( var i = 0; i < count; i++ )
		{
			var flag = flags[i];

			if ( (flag & HitFlagImpact) != 0 )
			{
				var surface = string.IsNullOrEmpty( surfacePaths[i] ) ? null : ResourceLibrary.Get<Surface>( surfacePaths[i] );
				CreateBulletImpact( positions[i], normals[i], surface?.SoundCollection.Bullet, surface?.PrefabCollection.BulletImpact );
			}

			if ( (flag & HitFlagTracer) != 0 && tracerInfo is not null )
			{
				Vector3? tracerStart = (flag & HitFlagTracerStart) != 0 ? tracerStarts[i] : null;
				tracerInfo.TracerEffects( this, isPrimary, tracerStart, positions[i] );
			}
		}
	}
}
