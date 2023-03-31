
using Sandbox;

/* Result from Pain Day 4, this will be here temporarily until it is clear how templates work */

namespace SWB_Player;

[Library]
public class PlayerDuck : BaseNetworkable
{
    public PlayerBaseController Controller;
    public bool IsActive;
    public float HeightMod;

    public PlayerDuck(PlayerBaseController controller)
    {
        Controller = controller;
    }

    public virtual void PreTick()
    {
        bool wants = Input.Down(InputButton.Duck);

        if (wants != IsActive)
        {
            if (wants) TryDuck();
            else TryUnDuck();
        }

        var targetHeightMod = IsActive && Controller.GroundEntity != null ? 0.5f : 1;
        HeightMod = MathX.Lerp(HeightMod, targetHeightMod, Time.Delta * 10f);
        Controller.EyeLocalPosition *= HeightMod;

        if (IsActive)
            Controller.SetTag("ducked");
    }

    protected virtual void TryDuck()
    {
        IsActive = true;
    }

    protected virtual void TryUnDuck()
    {
        var pm = Controller.TraceBBox(Controller.Position, Controller.Position, originalMins, originalMaxs);
        if (pm.StartedSolid) return;

        IsActive = false;
    }

    // Uck, saving off the bbox kind of sucks
    // and we should probably be changing the bbox size in PreTick
    Vector3 originalMins;
    Vector3 originalMaxs;

    public virtual void UpdateBBox(ref Vector3 mins, ref Vector3 maxs, float scale)
    {
        originalMins = mins;
        originalMaxs = maxs;

        if (IsActive)
            maxs = maxs.WithZ(36 * scale);
    }

    //
    // Coudl we do this in a generic callback too?
    //
    public virtual float GetWishSpeed()
    {
        if (!IsActive) return -1;
        return 64.0f;
    }
}
