
namespace SWB_Base.Attachments;

public class Sight : OffsetAttachment
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

    public override StatModifier StatModifier => new StatModifier
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

    public override void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel)
    {
        oldZoomAnimData = weapon.ZoomAnimData;
        oldZoomWeaponFOV = weapon.General.ZoomWeaponFOV;
        oldZoomPlayerFOV = weapon.General.ZoomPlayerFOV;

        weapon.ZoomAnimData = ZoomAnimData;

        if (ZoomWeaponFOV > 0)
            weapon.General.ZoomWeaponFOV = ZoomWeaponFOV;

        if (ZoomPlayerFOV > 0)
            weapon.General.ZoomPlayerFOV = ZoomPlayerFOV;
    }

    public override void OnUnequip(WeaponBase weapon)
    {
        weapon.ZoomAnimData = oldZoomAnimData;
        weapon.General.ZoomWeaponFOV = oldZoomWeaponFOV;
        weapon.General.ZoomPlayerFOV = oldZoomPlayerFOV;
    }
}

public class ReflexSight : Sight
{
    public override string Name => "Walther MRS Reflex";
    public override string IconPath => "attachments/swb/sight/reflex/ui/icon.png";
    public override string ModelPath => "attachments/swb/sight/reflex/w_reflex_sight.vmdl";
}
