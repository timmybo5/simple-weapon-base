using System;
using System.Collections.Generic;
using Sandbox;

/* 
 * Weapon base attachments support
*/

namespace SWB_Base;

public enum AttachmentCategoryName
{
    Barrel,
    Sight,
    Grip,
    Rail,
    Magazine,
    Muzzle,
    Stock,
    Other,
    Special,
    Tactical,
    None,
}

public class AttachmentCategory : IComparable<AttachmentCategory>
{
    /// <summary>Display name</summary>
    public AttachmentCategoryName Name { get; set; }

    /// <summary>If attachments in this category be equipped from the customization menu</summary>
    public bool Selectable { get; set; } = true;

    /// <summary>WIP</summary>
    public string BoneOrAttachment { get; set; }

    /// <summary>List of attachments</summary>
    public List<AttachmentBase> Attachments { get; set; }

    public int CompareTo(AttachmentCategory obj)
    {
        if (obj == null)
            return 1;

        else
            return Name.ToString().CompareTo(obj.Name.ToString());
    }
}

// Attachments using offsets
public abstract class OffsetAttachment : AttachmentBase
{
    /// <summary>The viewmodel bone to parent the attachment to</summary>
    public virtual string ViewParentBone { get; set; }

    /// <summary>The offset to the viewmodel bone</summary>
    public virtual Transform ViewTransform { get; set; }

    /// <summary>The worldmodel bone to parent the attachment to</summary>
    public virtual string WorldParentBone { get; set; }

    /// <summary>The offset to the worldmodel bone</summary>
    public virtual Transform WorldTransform { get; set; }

    private AttachmentModel attachmentModel = null;

    public override AttachmentModel CreateModel(WeaponBase weapon)
    {
        var model = new AttachmentModel(Game.IsClient);
        model.Owner = weapon.Owner;
        model.SetModel(ModelPath);

        if (Game.IsClient)
        {
            model.SetParent(weapon.ViewModelEntity, ViewParentBone, ViewTransform);
        }
        else
        {
            model.SetParent(weapon, WorldParentBone, WorldTransform);
        }

        return model;
    }

    public override AttachmentModel Equip(WeaponBase weapon, bool createModel = true)
    {
        // Model
        if (createModel)
        {
            attachmentModel = CreateModel(weapon);
        }

        // Stats
        if (StatModifier != null)
        {
            StatModifier.Apply(weapon);
        }

        OnEquip(weapon, attachmentModel);
        return attachmentModel;
    }

    public override void Unequip(WeaponBase weapon)
    {
        // Model
        if (attachmentModel != null)
            attachmentModel.Delete();

        // Stats
        if (StatModifier != null)
        {
            StatModifier.Remove(weapon);
        }

        OnUnequip(weapon);
    }
}

// Attachments using bodygroups
public abstract class BodyGroupAttachment : AttachmentBase
{
    /// <summary>The name of the body group</summary>
    public virtual string BodyGroup { get; set; }

    /// <summary>The name of the body group choice</summary>
    public virtual int BodyGroupChoice { get; set; }

    /// <summary>The default target body group value</summary>
    public virtual int BodyGroupDefault { get; set; } = 0;

    private void SetBodyGroup(WeaponBase weapon, int choice)
    {
        if (Game.IsClient)
        {
            weapon.ViewModelEntity?.SetBodyGroup(BodyGroup, choice);
        }
        else
        {
            weapon.SetBodyGroup(BodyGroup, choice);
        }
    }

    public override AttachmentModel CreateModel(WeaponBase weapon)
    {
        // Not possible to get mesh from bodygroup
        return null;
    }

    public override AttachmentModel Equip(WeaponBase weapon, bool createModel = true)
    {
        // Model
        if (createModel)
        {
            SetBodyGroup(weapon, BodyGroupChoice);
        }

        // Stats
        if (StatModifier != null)
        {
            StatModifier.Apply(weapon);
        }

        OnEquip(weapon, null);
        return null;
    }

    public override void Unequip(WeaponBase weapon)
    {
        // Model
        SetBodyGroup(weapon, BodyGroupDefault);

        // Stats
        if (StatModifier != null)
        {
            StatModifier.Remove(weapon);
        }

        OnUnequip(weapon);
    }
}

// General attachment
public abstract class AttachmentBase : IComparable<AttachmentBase>
{
    /// <summary>Display name (needs to be unique)</summary>
    public virtual string Name => "";

    /// <summary>Display description</summary>
    public virtual string Description => "";

    /// <summary>List of positive attributes</summary>
    public virtual string[] Positives => Array.Empty<string>();

    /// <summary>List of negative attributes</summary>
    public virtual string[] Negatives => Array.Empty<string>();

    /// <summary>Path to an image that represent the attachment on the HUD</summary>
    public virtual string IconPath => "";

    /// <summary>Path to the attachment model</summary> 
    public virtual string ModelPath => "";

    /// <summary>Name of the model attachment used for new effect origins</summary>
    public virtual string EffectAttachment => "";

    /// <summary>Weapon stats changer</summary>
    public virtual StatModifier StatModifier => null;

    /// <summary>Depends on another attachment (e.g. rail/mount)</summary>
    public string RequiresAttachmentWithName { get; set; }

    /// <summary>Will be auto-equipped on first deploy</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Equips the attachment
    /// </summary>
    /// <param name="weapon">Weapon to equip attachment on</param>
    /// <param name="createModel">If new attachment models should be created</param>
    /// <returns></returns>
    public abstract AttachmentModel Equip(WeaponBase weapon, bool createModel = true);

    /// <summary>
    /// Unequips the attachment
    /// </summary>
    /// <param name="weapon">Weapon to unequip attachment from</param>
    public abstract void Unequip(WeaponBase weapon);

    /// <summary>
    /// Creates the view and world attachment models
    /// </summary>
    /// <param name="weapon">Weapon to parent the attachments to</param>
    /// <returns></returns>
    public abstract AttachmentModel CreateModel(WeaponBase weapon);

    /// <summary>
    /// Gets called after the attachment is equipped
    /// </summary>
    /// <param name="weapon">Weapon the attachment was attached to</param>
    /// <param name="attachmentModel">The created attachment model</param>
    public abstract void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel);

    /// <summary>
    /// Gets called after the attachment is unequipped
    /// </summary>
    /// <param name="weapon">Weapon the attachment was attached to</param>
    public abstract void OnUnequip(WeaponBase weapon);

    public int CompareTo(AttachmentBase obj)
    {
        if (obj == null)
            return 1;

        else
            return Name.CompareTo(obj.Name);
    }
}

public partial class ActiveAttachment : BaseNetworkable
{
    /// <summary>The viewmodel attachment model (client only)</summary>
    public AttachmentModel ViewAttachmentModel { get; set; }

    /// <summary>The worldmodel attachment model</summary>
    [Net]
    public AttachmentModel WorldAttachmentModel { get; set; }

    [Net]
    public string Name { get; set; }
    [Net]
    public AttachmentCategoryName Category { get; set; }

    /// <summary>If the attachment was equipped automatically</summary>
    [Net]
    public bool Forced { get; set; }

    public override string ToString()
    {
        return String.Format("Name: {0}, Category:{1}, ViewAttachmentModel:{2}, WorldAttachmentModel:{3}", Name, Category, ViewAttachmentModel, WorldAttachmentModel);
    }
}
