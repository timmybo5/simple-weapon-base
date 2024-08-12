namespace SWB.Player;

public partial class PlayerBase
{
	[Property] public ModelPhysics RagdollPhysics { get; set; }

	public bool IsRagdolled => RagdollPhysics.Enabled;

	[Broadcast]
	public virtual void Ragdoll( Vector3 force, Vector3 forceOrigin )
	{
		if ( !IsValid ) return;

		ToggleColliders( false );
		RagdollPhysics.Enabled = true;
		//Body.Tags.Add( TagsHelper.Trigger );
		//RagdollPhysics.Tags.Add( TagsHelper.Trigger );

		foreach ( var body in RagdollPhysics.PhysicsGroup.Bodies )
		{
			//body.GetGameObject().Tags.Add( TagsHelper.Trigger );
			body.ApplyImpulseAt( forceOrigin, force );
		}
	}

	public virtual void ToggleColliders( bool enable )
	{
		var colliders = Body.Components.GetAll<Collider>( FindMode.EverythingInSelfAndParent );

		foreach ( var collider in colliders )
		{
			collider.Enabled = enable;
		}
	}

	[Broadcast]
	public virtual void Unragdoll()
	{
		if ( !IsValid || RagdollPhysics is null ) return;

		RagdollPhysics.Renderer.Transform.LocalPosition = Vector3.Zero;
		RagdollPhysics.Renderer.Transform.LocalRotation = Rotation.Identity;
		RagdollPhysics.Enabled = false;
		ToggleColliders( true );
	}
}
