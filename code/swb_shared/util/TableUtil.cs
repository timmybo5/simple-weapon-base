using System;
using System.Collections.Generic;

/* 
 * Utility class for tables
*/

namespace SWB.Shared;

class TableUtil
{
	public static T GetRandom<T>( IList<T> list )
	{
		if ( list.Count == 0 ) return default;

		var randI = Random.Shared.Next( list.Count );
		return list[randI];
	}
}
