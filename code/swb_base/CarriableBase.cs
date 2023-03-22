using Sandbox;

namespace SWB_Base;

/// <summary>
/// An entity that can be carried in the player's inventory and hands.
/// </summary>
public class CarriableBase : AnimatedEntity
{
    public virtual string ViewModelPath => null;
    public BaseViewModel ViewModelEntity { get; protected set; }

    public override void Spawn()
    {
        base.Spawn();

        PhysicsEnabled = true;
        UsePhysicsCollision = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;
    }

    public virtual void ActiveStart(Entity ent)
    {
        //base.ActiveStart( ent );

        EnableDrawing = true;

        if (ent is AnimatedEntity animatedEntity)
        {
            SimulateAnimator(animatedEntity);
        }

        //
        // If we're the local player (clientside) create viewmodel
        // and any HUD elements that this weapon wants
        //
        if (IsLocalPawn)
        {
            DestroyViewModel();
            DestroyHudElements();

            CreateViewModel();
            CreateHudElements();
        }
    }

    public virtual void ActiveEnd(Entity ent, bool dropped)
    {
        //
        // If we're just holstering, then hide us
        //
        if (!dropped)
        {
            EnableDrawing = false;
        }

        if (Game.IsClient)
        {
            DestroyViewModel();
            DestroyHudElements();
        }
    }

    public virtual void OnCarryStart(Entity carrier)
    {
        if (Game.IsClient) return;

        SetParent(carrier, true);
        Owner = carrier;
        PhysicsEnabled = false;
        EnableAllCollisions = false;
        EnableDrawing = false;
    }

    public virtual void OnCarryDrop(Entity dropper)
    {
        if (Game.IsClient) return;

        SetParent(null);
        Owner = null;
        PhysicsEnabled = true;
        EnableDrawing = true;
        EnableAllCollisions = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (Game.IsClient && ViewModelEntity.IsValid())
        {
            DestroyViewModel();
            DestroyHudElements();
        }
    }

    public virtual void SimulateAnimator(AnimatedEntity anim)
    {
        anim.SetAnimParameter("holdtype", 1);
        anim.SetAnimParameter("aim_body_weight", 1.0f);
        anim.SetAnimParameter("holdtype_handedness", 0);
    }

    public virtual void CreateViewModel()
    {
        Game.AssertClient();

        if (string.IsNullOrEmpty(ViewModelPath))
            return;

        ViewModelEntity = new BaseViewModel();
        ViewModelEntity.Position = Position;
        ViewModelEntity.Owner = Owner;
        ViewModelEntity.EnableViewmodelRendering = true;
        ViewModelEntity.SetModel(ViewModelPath);
    }

    public virtual void CreateHudElements() { }

    public virtual void DestroyHudElements() { }

    public virtual void DestroyViewModel()
    {
        ViewModelEntity?.Delete();
        ViewModelEntity = null;
    }

    public virtual bool CanCarry(Entity carrier)
    {
        return true;
    }

    /// <summary>
    /// Utility - return the entity we should be spawning particles from etc
    /// </summary>
    public virtual ModelEntity EffectEntity => (ViewModelEntity.IsValid() && IsFirstPersonMode) ? ViewModelEntity : this;
}
