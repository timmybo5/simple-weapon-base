namespace SWB.Shared;

public struct MovementImpact
{
	/// <summary>Modifies the velocity (WishVelocity *= Amount) [1 = none]</summary>
	public float Amount = 1f;
	public TimeUntil Duration = 0;

	public MovementImpact() { }
}
