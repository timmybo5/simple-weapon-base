using System;
using System.Collections.Generic;

namespace SWB.Base;

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
