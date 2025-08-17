using SWB.Shared;

namespace SWB.Player;

public partial class PlayerBase
{
	public GameObject RagdollGO { get; set; }
	public bool IsRagdolled => RagdollGO.IsValid;

	[Rpc.Broadcast]
	public virtual void Ragdoll( Vector3 force, Vector3 forceOrigin )
	{
		if ( !IsValid ) return;
		CreateRagdoll( force, forceOrigin );
		Body.Enabled = false;
	}

	public virtual void CreateRagdoll( Vector3 force, Vector3 forceOrigin )
	{
		RagdollGO = new GameObject( true, "Ragdoll" );
		RagdollGO.Tags.Add( TagsHelper.DeadPlayer );
		RagdollGO.NetworkMode = NetworkMode.Never;
		RagdollGO.WorldPosition = WorldPosition;
		RagdollGO.WorldRotation = Body.WorldRotation;

		// Renderer
		var renderer = RagdollGO.AddComponent<SkinnedModelRenderer>();
		renderer.Model = BodyRenderer.Model;
		renderer.UseAnimGraph = false;
		renderer.Sequence.Name = "Eyes_Closed";

		// Clothes
		var clothingContainer = ClothingContainer.CreateFromJson( clothingJSON );
		clothingContainer.Apply( renderer );

		// Physics
		var physics = RagdollGO.AddComponent<ModelPhysics>( true );
		physics.Model = renderer.Model;
		physics.Renderer = renderer;
		physics.CopyBonesFrom( BodyRenderer, true );
		foreach ( var body in physics.Bodies )
		{
			var forceMultiplier = IsOnGround ? 200 : 100;
			body.Component.ApplyForceAt( renderer.SceneModel.Bounds.Center, Velocity * forceMultiplier );
			body.Component.ApplyImpulseAt( forceOrigin, force );
		}
	}

	[Rpc.Broadcast]
	public virtual void Unragdoll( Vector3 pos )
	{
		if ( !IsValid ) return;
		RagdollGO?.Destroy();
		Body.Enabled = true;
		Body.LocalPosition = Vector3.Zero;
		Body.LocalRotation = Rotation.Identity;
		BodyRenderer.ClearParameters();
	}
}
