/* 
 * Utility class for dealing with Map objects
*/

using System.Collections.Generic;
using System.Linq;

namespace SWB.Shared;

public static class MapUtil
{
	public static void TagLights()
	{
		var mapInstance = Game.ActiveScene.GetComponentInChildren<MapInstance>();
		var envProbes = mapInstance.GetComponentsInChildren<EnvmapProbe>();

		if ( envProbes.Count() > 0 )
			TagLights( envProbes );
	}

	static async void TagLights( IEnumerable<Component> components )
	{
		// Log.Info( "[SWB] Tagging lights..." );

		// Cannot add tags directly, needs a delay to work
		await GameTask.DelaySeconds( 1 );
		foreach ( var comp in components )
		{
			if ( comp is null ) continue;
			comp.Tags.Add( TagsHelper.Light );
		}

		// Check if the tags were added
		await GameTask.DelaySeconds( 1 );
		var count = 0;
		foreach ( var comp in components )
		{
			if ( comp is null || comp.Tags.Has( TagsHelper.Light ) )
				count++;
		}

		if ( count != components.Count() )
			TagLights( components );
	}
}
