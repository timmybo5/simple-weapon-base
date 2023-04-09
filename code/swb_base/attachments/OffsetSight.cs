
namespace SWB_Base.Attachments;

public class OffsetSight : OffsetAttachment
{
    public override string Name => "Sight";
    public override string Description => "An optical sight that allows the user to look through a partially reflecting glass element and see an illuminated projection of an aiming point or some other image superimposed on the field of view.";
    public override string[] Positives => new string[]
    {
        "Precision sight picture"
    };

    public override string[] Negatives => new string[]
    {
    };

    public override StatModifier StatModifier { get; set; } = new StatModifier
    {
        Spread = -0.05f,
    };

    /// <summary>The new zoom offset</summary>
    public AngPos ZoomAnimData;
    private AngPos oldZoomAnimData;

    /// <summary>The new weapon zoom FOV (-1 to use default weapon fov)</summary>
    public int ZoomWeaponFOV;
    private int oldZoomWeaponFOV;

    /// <summary>The new player zoom FOV (-1 to use default player fov)</summary>
    public int ZoomPlayerFOV;
    private int oldZoomPlayerFOV;

    /// <summary>The new player scoped FOV (-1 to use default player fov)</summary>
    public int ScopedPlayerFOV;
    private int oldScopedPlayerFOV;

    /// <summary>The new aim sensitivity</summary>
    public float AimSensitivity = -1;
    private float oldAimSensitivity;

    /// <summary>The new scoped aim sensitivity</summary>
    public float ScopedAimSensitivity = -1;
    private float oldScopedAimSensitivity;

    /// <summary>The new zoom out speed</summary>
    public int ZoomOutFOVSpeed;
    private int oldZoomOutFOVSpeed;

    /// <summary>The new zoom spread modifier</summary>
    public float ZoomSpreadMod = -1;
    private float oldZoomSpreadMod;

    public override void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel)
    {
        oldZoomAnimData = weapon.ZoomAnimData;
        oldZoomWeaponFOV = weapon.General.ZoomWeaponFOV;
        oldZoomPlayerFOV = weapon.General.ZoomPlayerFOV;
        oldScopedPlayerFOV = weapon.General.ScopedPlayerFOV;
        oldAimSensitivity = weapon.General.AimSensitivity;
        oldZoomOutFOVSpeed = weapon.General.ZoomOutFOVSpeed;
        oldZoomSpreadMod = weapon.General.ZoomSpreadMod;
        oldScopedAimSensitivity = weapon.General.ScopedAimSensitivity;

        weapon.ZoomAnimData = ZoomAnimData;

        if (ZoomWeaponFOV > 0)
            weapon.General.ZoomWeaponFOV = ZoomWeaponFOV;

        if (ZoomPlayerFOV > 0)
            weapon.General.ZoomPlayerFOV = ZoomPlayerFOV;

        if (ScopedPlayerFOV > 0)
            weapon.General.ScopedPlayerFOV = ScopedPlayerFOV;

        if (AimSensitivity > -1)
            weapon.General.AimSensitivity = AimSensitivity;

        if (ScopedAimSensitivity > -1)
            weapon.General.ScopedAimSensitivity = ScopedAimSensitivity;

        if (ZoomOutFOVSpeed > 0)
            weapon.General.ZoomOutFOVSpeed = ZoomOutFOVSpeed;

        if (ZoomSpreadMod > -1)
            weapon.General.ZoomSpreadMod = ZoomSpreadMod;
    }

    public override void OnUnequip(WeaponBase weapon)
    {
        weapon.ZoomAnimData = oldZoomAnimData;
        weapon.General.ZoomWeaponFOV = oldZoomWeaponFOV;
        weapon.General.ZoomPlayerFOV = oldZoomPlayerFOV;
        weapon.General.AimSensitivity = oldAimSensitivity;
        weapon.General.ScopedAimSensitivity = oldScopedAimSensitivity;
        weapon.General.ZoomOutFOVSpeed = oldZoomOutFOVSpeed;
        weapon.General.ZoomSpreadMod = oldZoomSpreadMod;
        weapon.General.ScopedPlayerFOV = oldScopedPlayerFOV;
    }
}

public class ReflexSight : OffsetSight
{
    public override string Name => "Walther MRS Reflex";
    public override string IconPath => "attachments/swb/sight/reflex/ui/icon.png";
    public override string ModelPath => "attachments/swb/sight/reflex/w_reflex_sight.vmdl";
}
