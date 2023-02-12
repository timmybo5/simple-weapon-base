using Sandbox;

namespace SWB_Base;

public interface ISWBPlayer
{
    public Entity ActiveChild { get; set; }
    public Vector3 EyePosition { get; }
    public Rotation EyeRotation { get; }
    public Angles ViewAngles { get; set; }
    public BulletSimulator BulletSimulator { get; }
    public void ScreenShake(ScreenShakeStruct screenShake);
    public bool IsModelEditing();
    public bool IsAttachmentEditing();
    public int AmmoCount(AmmoType type);
    public int TakeAmmo(AmmoType type, int amount);
    public void OnScopeStart();
    public void OnScopeEnd();

}
