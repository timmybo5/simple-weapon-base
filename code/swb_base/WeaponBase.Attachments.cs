using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base
{

    public partial class WeaponBase
    {
        [Net]
        public IList<ActiveAttachment> ActiveAttachments { get; set; }
        private bool initializedAttachments = false;

        protected void InitAttachments()
        {
            Host.AssertServer();

            foreach (var attachmentCategory in AttachmentCategories)
            {
                foreach (var attachment in attachmentCategory.Attachments)
                {
                    // Add forced attachments
                    if (attachment.Enabled)
                    {
                        var activeAttachment = new ActiveAttachment
                        {
                            Name = attachment.Name,
                            Category = attachmentCategory.Name,
                            Forced = true,
                        };

                        // Adding to ActiveAttachments in initialize on client will not work since server overrides it
                        ActiveAttachments.Add(activeAttachment);
                    }

                    // One attachment per category
                    break;
                }
            }
        }

        public AttachmentBase GetAttachment(string name)
        {
            foreach (var attachmentCategory in AttachmentCategories)
            {
                foreach (var attachment in attachmentCategory.Attachments)
                {
                    if (attachment.Name == name)
                    {
                        return attachment;
                    }

                }
            }

            return null;
        }

        public AttachmentCategoryName GetAttachmentCategory(string name)
        {
            foreach (var attachmentCategory in AttachmentCategories)
            {
                foreach (var attachment in attachmentCategory.Attachments)
                {
                    if (attachment.Name == name)
                    {
                        return attachmentCategory.Name;
                    }
                }
            }
            return AttachmentCategoryName.None;
        }

        public void EquipAttachment(ActiveAttachment activeAttachment)
        {
            var attachment = GetAttachment(activeAttachment.Name);
            if (attachment == null)
            {
                LogUtil.Error("Attachment with name " + activeAttachment.Name + " does not exist!");
                return;
            }

            var shouldCreateModel = IsServer ? activeAttachment.WorldAttachmentModel == null : true;
            var attachmentModel = attachment.Equip(this, shouldCreateModel);

            if (attachmentModel != null)
            {
                if (IsClient)
                {
                    activeAttachment.ViewAttachmentModel = attachmentModel;
                }
                else
                {
                    activeAttachment.WorldAttachmentModel = attachmentModel;
                }
            }

            if (!ActiveAttachments.Contains(activeAttachment))
            {
                ActiveAttachments.Add(activeAttachment);
            }
        }

        public void EquipAttachment(string name)
        {
            var activeAttachment = new ActiveAttachment
            {
                Name = name,
                Forced = false,
                Category = GetAttachmentCategory(name)
            };

            EquipAttachment(activeAttachment);
        }

        [ClientRpc]
        protected void HandleAttachmentsRecheckCL()
        {
            var instanceID = InstanceID;
            _ = TryHandleAttachments(instanceID);
        }

        async Task TryHandleAttachments(int instanceID)
        {
            Host.AssertClient();

            if (ActiveAttachments.Count > 0)
            {
                HandleAttachments(true);
                return;
            }

            var activeWeapon = Owner.ActiveChild;
            await GameTask.DelaySeconds(0.05f);
            if (!IsAsyncValid(activeWeapon, instanceID)) return;
            _ = TryHandleAttachments(instanceID);
        }

        public virtual void HandleAttachments(bool shouldEquip)
        {
            if (AttachmentCategories != null)
            {
                // Initialize attachments on server
                if (Host.IsServer && shouldEquip && !initializedAttachments)
                {
                    initializedAttachments = true;
                    InitAttachments();

                    // Client will not have attachments instantly
                    if (ActiveAttachments.Count > 0)
                    {
                        HandleAttachmentsRecheckCL(To.Single(Owner));
                    }
                }

                foreach (var activeAttachment in ActiveAttachments)
                {
                    var attachment = GetAttachment(activeAttachment.Name);

                    if (attachment == null) continue;

                    if (shouldEquip)
                    {
                        EquipAttachment(activeAttachment);
                    }
                    else
                    {
                        attachment.Unequip(this);
                    }
                }
            }
        }

        public virtual ActiveAttachment GetActiveAttachmentFromCategory(AttachmentCategoryName attachmentCategoryName)
        {
            if (AttachmentCategories != null)
            {
                foreach (var activeAttachment in ActiveAttachments)
                {
                    if (activeAttachment.Category == attachmentCategoryName)
                    {
                        return activeAttachment;
                    }
                }
            }

            return null;
        }
    }
}
