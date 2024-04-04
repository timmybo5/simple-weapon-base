namespace SWB.Player;

public partial class PlayerBase
{
	bool isLoweringFlinch;
	float currFlinch;
	float targetFlinch;
	float flinchSpeed;

	public virtual void DoHitFlinch( float amount )
	{
		isLoweringFlinch = false;
		flinchSpeed = amount / 4f;
		targetFlinch = amount;
	}

	public virtual void HandleFlinch()
	{
		if ( !IsAlive )
		{
			targetFlinch = 0;
			isLoweringFlinch = false;
			return;
		}

		if ( currFlinch == targetFlinch )
		{
			targetFlinch = 0;
			isLoweringFlinch = true;
		}
		else
		{
			currFlinch = currFlinch.Approach( targetFlinch, flinchSpeed );
		}

		if ( currFlinch > 0 )
		{
			var flinchAngles = new Angles( isLoweringFlinch ? currFlinch : -currFlinch, 0, 0 );
			EyeAnglesOffset += flinchAngles;
		}
	}
}
