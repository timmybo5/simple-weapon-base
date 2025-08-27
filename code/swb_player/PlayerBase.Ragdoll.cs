using SWB.Shared;

namespace SWB.Player;

public partial class PlayerBase
{
	public GameObject RagdollGO { get; set; }
	public bool IsRagdolled => RagdollGO.IsValid;

	[Rpc.Broadcast]
	public virtual void Ragdoll( Vector3 force, Vector3 forceOrigin, Vector3 velocity )
	{
		if ( !IsValid ) return;
		CreateRagdoll( force, forceOrigin, velocity );
		Body.Enabled = false;
	}

	public virtual void CreateRagdoll( Vector3 force, Vector3 forceOrigin, Vector3 velocity )
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
		Dresser.BodyTarget = renderer;
		Dresser.Apply();
		Dresser.BodyTarget = BodyRenderer;

		// Physics
		var physics = RagdollGO.AddComponent<ModelPhysics>( true );
		physics.Model = renderer.Model;
		physics.Renderer = renderer;
		physics.CopyBonesFrom( BodyRenderer, true );

		var forceMultiplier = IsOnGround ? 200 : 100;
		velocity *= forceMultiplier;

		foreach ( var body in physics.Bodies )
		{
			body.Component.ApplyForceAt( renderer.SceneModel.Bounds.Center, velocity );
			body.Component.ApplyImpulseAt( forceOrigin, force );
		}
	}

	[Rpc.Broadcast]
	public virtual void Unragdoll()
	{
		if ( !IsValid ) return;
		RagdollGO?.Destroy();
		Body.Enabled = true;
		Body.LocalPosition = Vector3.Zero;
		Body.LocalRotation = Rotation.Identity;
		BodyRenderer.ClearParameters();
	}
}
