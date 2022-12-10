using Sandbox;

/* Result from Pain Day 4, this will be here temporarily until it is clear how templates work */

namespace SWB_Base;

public partial class PlayerBase
{
    public Entity Using { get; protected set; }

    /// <summary>
    /// This should be called somewhere in your player's tick to allow them to use entities
    /// </summary> 
    protected virtual void TickPlayerUse()
    {
        // This is serverside only
        if (!Game.IsServer) return;

        // Turn prediction off
        using (Prediction.Off())
        {
            if (Input.Pressed(InputButton.Use))
            {
                Using = FindUsable();

                if (Using == null)
                {
                    UseFail();
                    return;
                }
            }

            if (!Input.Down(InputButton.Use))
            {
                StopUsing();
                return;
            }

            if (!Using.IsValid())
                return;

            // If we move too far away or something we should probably ClearUse()?

            //
            // If use returns true then we can keep using it
            //
            if (Using is IUse use && use.OnUse(this))
                return;

            StopUsing();
        }
    }

    /// <summary>
    /// Player tried to use something but there was nothing there.
    /// Tradition is to give a dissapointed boop.
    /// </summary>
    protected virtual void UseFail()
    {
        PlaySound("player_use_fail");
    }

    /// <summary>
    /// If we're using an entity, stop using it
    /// </summary>
    protected virtual void StopUsing()
    {
        Using = null;
    }

    /// <summary>
    /// Returns if the entity is a valid usaable entity
    /// </summary>
    protected bool IsValidUseEntity(Entity e)
    {
        if (e == null) return false;
        if (e is not IUse use) return false;
        if (!use.IsUsable(this)) return false;

        return true;
    }

    /// <summary>
    /// Find a usable entity for this player to use
    /// </summary>
    protected virtual Entity FindUsable()
    {
        // First try a direct 0 width line
        var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * 85)
            .Ignore(this)
            .Run();

        // See if any of the parent entities are usable if we ain't.
        var ent = tr.Entity;
        while (ent.IsValid() && !IsValidUseEntity(ent))
        {
            ent = ent.Parent;
        }

        // Nothing found, try a wider search
        if (!IsValidUseEntity(ent))
        {
            tr = Trace.Ray(EyePosition, EyePosition + this.EyeRotation.Forward * 85)
            .Radius(2)
            .Ignore(this)
            .Run();

            // See if any of the parent entities are usable if we ain't.
            ent = tr.Entity;
            while (ent.IsValid() && !IsValidUseEntity(ent))
            {
                ent = ent.Parent;
            }
        }

        // Still no good? Bail.
        if (!IsValidUseEntity(ent)) return null;

        return ent;
    }
}
