using Sandbox;

namespace SWB_Base;

public interface ISWBPlayer : IEntity
{
    /// <summary>
    /// The entity that the pawn is current holding/using
    /// </summary>
    public Entity ActiveChild { get; set; }

    /// <summary>
    /// The eye position of the pawn, networked
    /// </summary>
    public Vector3 EyePosition { get; }

    /// <summary>
    /// The eye (view) rotation of the pawn, networked
    /// </summary>
    public Rotation EyeRotation { get; }

    /// <summary>
    /// The local player's view angles, comes from client input, local/client only
    /// </summary>
    public Angles ViewAngles { get; set; }

    /// <summary>
    /// The physical bullet simulator for the player.
    /// </summary>
    public BulletSimulator BulletSimulator { get; }

    /// <summary>
    /// Called when the weapon base wants to shake the client screen
    /// </summary>
    /// <param name="screenShake">The screen shake parameters</param>
    public void ScreenShake(ScreenShakeStruct screenShake);

    /// <summary>
    /// Whether the player is looking at their gun (as if model or attachment editting.)
    /// </summary>
    public bool IsEditingWeapon { get; }

    /// <summary>
    /// Called when the weapon wants to know how much ammo is available
    /// </summary>
    /// <param name="type">The type of ammo</param>
    /// <returns>How much ammo is available</returns>
    public int AmmoCount(AmmoType type);

    /// <summary>
    /// Called when the weapon is trying to take ammo.
    /// </summary>
    /// <param name="type">The type of ammo</param>
    /// <param name="amount">The amount of ammo requested</param>
    /// <returns>How much ammo was actually taken</returns>
    public int TakeAmmo(AmmoType type, int amount);

    /// <summary>
    /// Called when the (sniper) is zoomed in
    /// </summary>
    public void OnScopeStart();

    /// <summary>
    /// Called when the (sniper) is no longer zoomed in.
    /// </summary>
    public void OnScopeEnd();

    /// <summary>
    /// Called when the weapon recoils.
    /// Call `WeaponBase.GetRecoilAngles()` to get the suggested recoil angle.
    /// </summary>
    public void OnRecoil();
}
