using System.ComponentModel;
using Sandbox;

/* Result from Pain Day 4, this will be here temporarily until it is clear how templates work */

namespace SWB_Base;

[Title("Player"), Icon("emoji_people")]
public partial class PlayerBase : AnimatedEntity
{
    /// <summary>
    /// The PlayerController takes player input and moves the player. This needs
    /// to match between client and server. The client moves the local player and
    /// then checks that when the server moves the player, everything is the same.
    /// This is called prediction. If it doesn't match the player resets everything
    /// to what the server did, that's a prediction error.
    /// You should really never manually set this on the client - it's replicated so
    /// that setting the class on the server will automatically network and set it
    /// on the client.
    /// </summary>
    [Net, Predicted] public PlayerPawnController Controller { get; set; }

    /// <summary>
    /// This is used for noclip mode 
    /// </summary>
    [Net, Predicted] public PlayerPawnController DevController { get; set; }

    /// <summary> 
    /// The active weapon, or tool, or whatever else
    /// </summary>
    [Net, Predicted] public Entity ActiveChild { get; set; }
    [ClientInput] public Vector3 InputDirection { get; protected set; }
    [ClientInput] public Entity ActiveChildInput { get; set; }
    [ClientInput] public Angles ViewAngles { get; set; }
    public Angles OriginalViewAngles { get; private set; }

    public IInventoryBase Inventory { get; protected set; }

    /// <summary>
    /// Return the controller to use. Remember any logic you use here needs to match
    /// on both client and server. This is called as an accessor every tick.. so maybe
    /// avoid creating new classes here or you're gonna be making a ton of garbage!
    /// </summary>
    public virtual PlayerPawnController GetActiveController()
    {
        if (DevController != null) return DevController;

        return Controller;
    }


    /// <summary>
    /// The player animator is responsible for positioning/rotating the player and
    /// interacting with the animation graph.
    /// </summary>
    [Net, Predicted] public PlayerBaseAnimator Animator { get; set; }

    /// <summary>
    /// Return the controller to use. Remember any logic you use here needs to match
    /// on both client and server. This is called as an accessor every tick.. so maybe
    /// avoid creating new classes here or you're gonna be making a ton of garbage!
    /// </summary>
    public virtual PlayerBaseAnimator GetActiveAnimator() => Animator;

    /// <summary>
    /// Provides an easy way to switch our current cameramode component
    /// </summary> 
    public CameraMode CameraMode
    {
        get => Components.Get<CameraMode>();
        set
        {
            Components.RemoveAny<CameraMode>(); // Temp workaround
            Components.Add(value);
        }
    }

    TimeSince timeSinceDied;

    public virtual void SimulateBase(IClient cl)
    {
        // E.g player trying to switch weapon
        if (ActiveChildInput.IsValid() && ActiveChildInput.Owner == this)
        {
            ActiveChild = ActiveChildInput;
        }

        if (LifeState == LifeState.Dead)
        {
            if (timeSinceDied > 3 && Game.IsServer)
            {
                Respawn();
            }

            return;
        }

        var controller = GetActiveController();
        controller?.Simulate(cl, this, GetActiveAnimator());
    }


    public override void FrameSimulate(IClient cl)
    {
        base.FrameSimulate(cl);

        CameraMode.UpdateCamera();
        //UpdateCamera();

        var controller = GetActiveController();
        controller?.FrameSimulate(cl, this, GetActiveAnimator());

        if (this.GetWaterLevel() > 0.9f)
        {
            Audio.SetEffect("underwater", 1, velocity: 5.0f);
        }
        else
        {
            Audio.SetEffect("underwater", 0, velocity: 1.0f);
        }
    }

    /// <summary>
    /// Applies flashbang-like ear ringing effect to the player.
    /// </summary>
    /// <param name="strength">Can be approximately treated as duration in seconds.</param>
    [ClientRpc]
    public void Deafen(float strength)
    {
        Audio.SetEffect("flashbang", strength, velocity: 20.0f, fadeOut: 4.0f * strength);
    }

    public override void Spawn()
    {
        EnableLagCompensation = true;

        Tags.Add("player");

        base.Spawn();
    }

    /// <summary>
    /// Called once the player's health reaches 0
    /// </summary>
    public override void OnKilled()
    {
        GameManager.Current?.OnKilled(this);

        timeSinceDied = 0;
        LifeState = LifeState.Dead;
        StopUsing();

        Client?.AddInt("deaths", 1);
    }


    /// <summary>
    /// Sets LifeState to Alive, Health to Max, nulls velocity, and calls Gamemode.PlayerRespawn
    /// </summary>
    public virtual void Respawn()
    {
        Game.AssertServer();

        LifeState = LifeState.Alive;
        Health = 100;
        Velocity = Vector3.Zero;
        this.ClearWaterLevel();

        CreateHull();

        GameManager.Current?.MoveToSpawnpoint(this);
        ResetInterpolation();
    }

    /// <summary>
    /// Create a physics hull for this player. The hull stops physics objects and players passing through
    /// the player. It's basically a big solid box. It also what hits triggers and stuff.
    /// The player doesn't use this hull for its movement size.
    /// </summary>
    public virtual void CreateHull()
    {
        SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-16, -16, 0), new Vector3(16, 16, 72));

        //var capsule = new Capsule( new Vector3( 0, 0, 16 ), new Vector3( 0, 0, 72 - 16 ), 32 );
        //var phys = SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, capsule );


        //	phys.GetBody(0).RemoveShadowController();

        // TODO - investigate this? if we don't set movetype then the lerp is too much. Can we control lerp amount?
        // if so we should expose that instead, that would be awesome.
        // MoveType = MoveType.MOVETYPE_WALK;

        EnableHitboxes = true;
    }

    /// <summary>
    /// Called from the gamemode, clientside only.
    /// </summary>
    public override void BuildInput()
    {
        OriginalViewAngles = ViewAngles;
        InputDirection = Input.AnalogMove;

        if (Input.StopProcessing)
            return;

        var look = Input.AnalogLook;

        if (ViewAngles.pitch > 90f || ViewAngles.pitch < -90f)
        {
            look = look.WithYaw(look.yaw * -1f);
        }

        var viewAngles = ViewAngles;
        viewAngles += look;
        viewAngles.pitch = viewAngles.pitch.Clamp(-89f, 89f);
        viewAngles.roll = 0f;
        ViewAngles = viewAngles.Normal;

        ActiveChild?.BuildInput();

        GetActiveController()?.BuildInput();

        if (Input.StopProcessing)
            return;

        GetActiveAnimator()?.BuildInput();
    }

    /// <summary>
    /// A generic corpse entity
    /// </summary>
    public ModelEntity Corpse { get; set; }

    TimeSince timeSinceLastFootstep = 0;

    /// <summary>
    /// A foostep has arrived!
    /// </summary>
    public override void OnAnimEventFootstep(Vector3 pos, int foot, float volume)
    {
        if (LifeState != LifeState.Alive)
            return;

        if (!Game.IsClient)
            return;

        if (timeSinceLastFootstep < 0.2f)
            return;

        volume *= FootstepVolume();

        timeSinceLastFootstep = 0;

        //DebugOverlay.Box( 1, pos, -1, 1, Color.Red );
        //DebugOverlay.Text( pos, $"{volume}", Color.White, 5 );

        var tr = Trace.Ray(pos, pos + Vector3.Down * 20)
            .Radius(1)
            .Ignore(this)
            .Run();

        if (!tr.Hit) return;

        tr.Surface.DoFootstep(this, tr, foot, volume);
    }

    public virtual float FootstepVolume()
    {
        return Velocity.WithZ(0).Length.LerpInverse(0.0f, 200.0f) * 0.2f;
    }

    public override void StartTouch(Entity other)
    {
        if (Game.IsClient) return;

        if (other is PickupTrigger)
        {
            StartTouch(other.Parent);
            return;
        }

        Inventory?.Add(other, Inventory.Active == null);
    }

    /// <summary>
    /// This isn't networked, but it's predicted. If it wasn't then when the prediction system
    /// re-ran the commands LastActiveChild would be the value set in a future tick, so ActiveEnd
    /// and ActiveStart would get called multiple times and out of order, causing all kinds of pain.
    /// </summary>
    [Predicted]
    Entity LastActiveChild { get; set; }

    /// <summary>
    /// Simulated the active child. This is important because it calls ActiveEnd and ActiveStart.
    /// If you don't call these things, viewmodels and stuff won't work, because the entity won't
    /// know it's become the active entity.
    /// </summary>
    public virtual void SimulateActiveChild(IClient cl, Entity child)
    {
        if (LastActiveChild != child)
        {
            OnActiveChildChanged(LastActiveChild, child);
            LastActiveChild = child;
        }

        if (!LastActiveChild.IsValid())
            return;

        if (LastActiveChild.IsAuthority)
        {
            LastActiveChild.Simulate(cl);
        }
    }

    /// <summary>
    /// Called when the Active child is detected to have changed
    /// </summary>
    public virtual void OnActiveChildChanged(Entity previous, Entity next)
    {
        if (previous is CarriableBase previousBc)
        {
            previousBc?.ActiveEnd(this, previousBc.Owner != this);
        }

        if (next is CarriableBase nextBc)
        {
            nextBc?.ActiveStart(this);
        }
    }

    public virtual void TakeDamageBase(DamageInfo info)
    {
        if (LifeState == LifeState.Dead)
            return;

        base.TakeDamage(info);

        this.ProceduralHitReaction(info);

        //
        // Add a score to the killer
        //
        if (LifeState == LifeState.Dead && info.Attacker != null)
        {
            if (info.Attacker.Client != null && info.Attacker != this)
            {
                info.Attacker.Client.AddInt("kills");
            }
        }

        if (info.HasTag("blast"))
        {
            Deafen(To.Single(Client), info.Damage.LerpInverse(0, 60));
        }
    }

    public override void OnChildAdded(Entity child)
    {
        Inventory?.OnChildAdded(child);
    }

    public override void OnChildRemoved(Entity child)
    {
        Inventory?.OnChildRemoved(child);
    }

    /// <summary>
    /// Position a player should be looking from in world space.
    /// </summary>
    [Browsable(false)]
    public Vector3 EyePosition
    {
        get => Transform.PointToWorld(EyeLocalPosition);
        set => EyeLocalPosition = Transform.PointToLocal(value);
    }

    /// <summary>
    /// Position a player should be looking from in local to the entity coordinates.
    /// </summary>
    [Net, Predicted, Browsable(false)]
    public Vector3 EyeLocalPosition { get; set; }

    /// <summary>
    /// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
    /// </summary>
    [Browsable(false)]
    public Rotation EyeRotation
    {
        get => Transform.RotationToWorld(EyeLocalRotation);
        set => EyeLocalRotation = Transform.RotationToLocal(value);
    }

    /// <summary>
    /// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
    /// </summary>
    [Net, Predicted, Browsable(false)]
    public Rotation EyeLocalRotation { get; set; }

    /// <summary>
    /// Override the aim ray to use the player's eye position and rotation.
    /// </summary>
    public override Ray AimRay => new Ray(EyePosition, EyeRotation.Forward);
}
