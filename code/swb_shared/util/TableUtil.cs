using System;
using System.Collections.Generic;

/* 
 * Utility class for tables
*/

namespace SWB.Shared;

class TableUtil
{
	public static T GetRandom<T>( List<T> list )
	{
		if ( list.Count == 0 ) return default;

		var random = new Random();
		var randI = random.Next( list.Count );
		return list[randI];
	}
}
