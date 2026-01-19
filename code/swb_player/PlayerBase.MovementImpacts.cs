using SWB.Shared;
using System.Collections.Generic;

namespace SWB.Player;

public partial class PlayerBase
{
	List<MovementImpact> movementImpacts = new();
	float movementImpact;

	public virtual void ApplyMovementImpact( MovementImpact impact )
	{
		movementImpacts.Add( impact );
	}

	public virtual void HandleMovementImpacts()
	{
		movementImpact = 1;
		if ( movementImpacts.Count == 0 ) return;
		if ( !IsAlive )
		{
			movementImpacts.Clear();
			return;
		}

		for ( int i = movementImpacts.Count - 1; i >= 0; i-- )
		{
			var impact = movementImpacts[i];

			if ( impact.Duration <= 0f )
			{
				movementImpacts.RemoveAt( i );
				continue;
			}

			movementImpact *= impact.Amount;
		}
	}
}
