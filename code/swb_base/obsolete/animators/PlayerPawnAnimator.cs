using Sandbox;

/* Result from Pain Day 4, this will be here temporarily until it is clear how templates work */

namespace SWB_Base;

public class PlayerPawnAnimator : PlayerPawnController
{
    public AnimatedEntity AnimPawn => Pawn as AnimatedEntity;

    /// <summary>
    /// We'll convert Position to a local position to the players eyes and set
    /// the param on the animgraph.
    /// </summary>
    public virtual void SetLookAt(string name, Vector3 Position)
    {
        var player = Pawn as PlayerBase;
        var localPos = (Position - player.EyePosition) * Rotation.Inverse;
        SetAnimParameter(name, localPos);
    }

    /// <summary>
    /// Sets the param on the animgraph
    /// </summary>
    public virtual void SetAnimParameter(string name, Vector3 val)
    {
        AnimPawn?.SetAnimParameter(name, val);
    }

    /// <summary>
    /// Sets the param on the animgraph
    /// </summary>
    public virtual void SetAnimParameter(string name, float val)
    {
        AnimPawn?.SetAnimParameter(name, val);
    }

    /// <summary>
    /// Sets the param on the animgraph
    /// </summary>
    public virtual void SetAnimParameter(string name, bool val)
    {
        AnimPawn?.SetAnimParameter(name, val);
    }

    /// <summary>
    /// Sets the param on the animgraph
    /// </summary>
    public virtual void SetAnimParameter(string name, int val)
    {
        AnimPawn?.SetAnimParameter(name, val);
    }

    /// <summary>
    /// Calls SetParam( name, true ). It's expected that your animgraph
    /// has a "name" param with the auto reset property set.
    /// </summary>
    public virtual void Trigger(string name)
    {
        SetAnimParameter(name, true);
    }

    /// <summary>
    /// Resets all params to default values on the animgraph
    /// </summary>
    public virtual void ResetParameters()
    {
        AnimPawn?.ResetAnimParameters();
    }
}
