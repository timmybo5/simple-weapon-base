using Sandbox;

/*
 * Example Ragdoll implementation
*/

partial class ExamplePlayer
{
    // Can create lag, be cautious setting this too high
    static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 10 };

    [ClientRpc]
    void BecomeRagdollOnClient(Vector3 force, int forceBone)
    {
        var ent = new ModelEntity();
        ent.Position = Position;
        ent.Rotation = Rotation;
        ent.PhysicsEnabled = true;
        ent.UsePhysicsCollision = true;
        Tags.Add("debris");

        ent.SetModel(GetModelName());
        ent.CopyBonesFrom(this);
        ent.CopyBodyGroups(this);
        ent.CopyMaterialGroup(this);
        ent.TakeDecalsFrom(this);
        ent.SetRagdollVelocityFrom(this);
        ent.DeleteAsync(20.0f);

        // Copy the clothes over
        foreach (var child in Children)
        {
            if (!child.Tags.Has("clothes")) continue;
            if (child is not ModelEntity e) continue;

            var model = e.GetModelName();

            var clothing = new ModelEntity();
            clothing.SetModel(model);
            clothing.SetParent(ent, true);
            clothing.RenderColor = e.RenderColor;
            clothing.CopyBodyGroups(e);
            clothing.CopyMaterialGroup(e);
        }

        ent.PhysicsGroup.AddVelocity(force);

        if (forceBone >= 0)
        {
            var body = ent.GetBonePhysicsBody(forceBone);
            if (body != null)
            {
                body.ApplyForce(force * 1000);
            }
            else
            {
                ent.PhysicsGroup.AddVelocity(force);
            }
        }

        Corpse = ent;

        RagdollLimit.Watch(ent);
    }
}
