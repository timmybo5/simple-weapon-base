using SWB.Shared;

namespace SWB.Player;

[Group( "SWB" )]
[Title( "PlayerTriggerListener" )]
public class PlayerTriggerListener : Component, Component.ITriggerListener
{
	[Property] public PlayerBase Player { get; set; }
	[Property] public Collider Collider { get; set; }

	public void OnTriggerEnter( Collider other )
	{
		if ( other is null ) return;
		if ( other.Tags.Has( TagsHelper.Ladder ) )
			Player.OnLadderEnter( other );
	}

	public void OnTriggerExit( Collider other )
	{
		if ( other is null ) return;
		if ( other.Tags.Has( TagsHelper.Ladder ) )
			Player.OnLadderExit( other );
	}
}
