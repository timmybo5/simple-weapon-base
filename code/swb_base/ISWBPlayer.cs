using Sandbox;

namespace SWB_Base;

public interface ISWBPlayer
{
    public Entity ActiveChild { get; set; }
    public Vector3 EyePosition { get; }
    public Rotation EyeRotation { get; }
    public Angles OriginalViewAngles { get; }
    public Angles ViewAngles { get; set; }
    public Vector3 Velocity { get; }
    public PhysicsBody PhysicsBody { get; }
    public BulletSimulator BulletSimulator { get; }
    public bool IsFirstPersonMode { get; }
    public void ScreenShake(ScreenShakeStruct screenShake);
    public bool IsModelEditing();
    public bool IsAttachmentEditing();
    public int AmmoCount(AmmoType type);
    public int TakeAmmo(AmmoType type, int amount);
    public void OnScopeStart();
    public void OnScopeEnd();

}
