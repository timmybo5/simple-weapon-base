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

    public class AttachmentCategory
    {
        public AttachmentCategoryName Name { get; set; }

        public string BoneOrAttachment { get; set; } // Equip menu will point to this bone or attachment

        public List<AttachmentBase> Attachments { get; set; } // List of attachments
    }

    // Attachments using offsets
    public abstract class OffsetAttachment : AttachmentBase
    {
        public virtual string ViewParentBone { get; set; }
        public virtual Transform ViewTransform { get; set; }

        public virtual string WorldParentBone { get; set; }
        public virtual Transform WorldTransform { get; set; }

        public override AttachmentModel Equip(WeaponBase weapon, bool createModel = true)
        {
            AttachmentModel attachmentModel = null;

            //if ((Host.IsClient) || (Host.IsServer && WorldAttachmentModel == null))
            if (createModel)
            {
                attachmentModel = new AttachmentModel(Host.IsClient);
                attachmentModel.Owner = weapon.Owner;
                attachmentModel.SetModel(ModelPath);

                if (Host.IsClient)
                {
                    attachmentModel.SetParent(weapon.ViewModelEntity, ViewParentBone, ViewTransform);
                }
                else
                {
                    attachmentModel.SetParent(weapon, WorldParentBone, WorldTransform);
                }
            }

            OnEquip(weapon, attachmentModel);
            return attachmentModel;
        }
    }

    // Attachments using bodygroups (TODO)
    public abstract class BodygroupAttachment : AttachmentBase
    {
        // viewbodygroup
        // worldbodygroup
    }

    // General attachment
    public abstract class AttachmentBase
    {
        public virtual string Name => ""; // Needs to be unique
        public virtual string Description => "";
        public virtual string IconPath => "";
        public virtual string ModelPath => "";
        public virtual string EffectAttachment => ""; // New effect point if used

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

        public abstract void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel);
        public abstract void OnUnequip(WeaponBase weapon);
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
