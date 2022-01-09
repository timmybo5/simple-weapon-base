using System;
using System.Collections.Generic;
using Sandbox;

/* 
 * Weapon base attachments support
*/

namespace SWB_Base
{
    public enum AttachmentCategoryName
    {
        Barrel,
        Sight,
        Grip,
        Magazine,
        Muzzle,
        Stock,
        Other,
        Special,
        None,
    }

    public class AttachmentCategory : IComparable<AttachmentCategory>
    {
        public AttachmentCategoryName Name { get; set; }

        public string BoneOrAttachment { get; set; } // Equip menu will point to this bone or attachment

        public List<AttachmentBase> Attachments { get; set; } // List of attachments

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
        public virtual string ViewParentBone { get; set; }
        public virtual Transform ViewTransform { get; set; }

        public virtual string WorldParentBone { get; set; }
        public virtual Transform WorldTransform { get; set; }

        private AttachmentModel attachmentModel = null;

        public override AttachmentModel CreateModel(WeaponBase weapon)
        {
            var model = new AttachmentModel(Host.IsClient);
            model.Owner = weapon.Owner;
            model.SetModel(ModelPath);

            if (Host.IsClient)
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

    // Attachments using bodygroups (TODO)
    public abstract class BodygroupAttachment : AttachmentBase
    {
        // viewbodygroup
        // worldbodygroup
    }

    // General attachment
    public abstract class AttachmentBase : IComparable<AttachmentBase>
    {
        public virtual string Name => ""; // Needs to be unique
        public virtual string Description => "";
        public virtual string[] Positives => new string[0];
        public virtual string[] Negatives => new string[0];
        public virtual string IconPath => "";
        public virtual string ModelPath => "";
        public virtual string EffectAttachment => ""; // New effect point if used
        public virtual StatModifier StatModifier => null;

        public bool Enabled { get; set; } // Always on if enabled (cannot be disabled through menu)

        public virtual AttachmentModel Equip(WeaponBase weapon, bool createModel = true)
        {
            OnEquip(weapon, null);
            return null;
        }

        public virtual void Unequip(WeaponBase weapon)
        {
            OnUnequip(weapon);
        }

        public abstract AttachmentModel CreateModel(WeaponBase weapon);
        public abstract void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel);
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
        public AttachmentModel ViewAttachmentModel { get; set; }
        [Net]
        public AttachmentModel WorldAttachmentModel { get; set; }

        [Net]
        public string Name { get; set; }
        [Net]
        public AttachmentCategoryName Category { get; set; }
        [Net]
        public bool Forced { get; set; }

        public override string ToString()
        {
            return String.Format("Name: {0}, Category:{1}, ViewAttachmentModel:{2}, WorldAttachmentModel:{3}", Name, Category, ViewAttachmentModel, WorldAttachmentModel);
        }
    }
}
