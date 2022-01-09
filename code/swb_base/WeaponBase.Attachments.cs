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

                        // Adding to ActiveAttachments on client will not work since server overrides it
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

        public ActiveAttachment GetActiveAttachment(string name)
        {
            foreach (var activeAttachment in ActiveAttachments)
            {
                if (activeAttachment.Name == name)
                {
                    return activeAttachment;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds an AttachmentCategory by name
        /// </summary>
        public AttachmentCategory GetAttachmentCategory(string name)
        {
            foreach (var attachmentCategory in AttachmentCategories)
            {
                if (attachmentCategory.Name.ToString() == name)
                {
                    return attachmentCategory;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets an AttachmentCategoryName from an attachment name
        /// </summary>
        public AttachmentCategoryName GetAttachmentCategoryName(string name)
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

        /// <summary>
        /// Equips an attachment and assigns a valid attachment model to the activeAttachment.
        /// Gets called by EquipAttachmentSV for both client and server.
        /// </summary>
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

        /// <summary>
        /// Creates an ActiveAttachment and calls EquipAttachment.
        /// </summary>
        public void EquipAttachment(string name)
        {
            var activeAttachment = new ActiveAttachment
            {
                Name = name,
                Category = GetAttachmentCategoryName(name)
            };

            ActiveAttachments.Add(activeAttachment);

            EquipAttachment(activeAttachment);
        }

        /// <summary>
        /// Unequips an attachment.
        /// Gets called by EquipAttachmentSV for both client and server.
        /// </summary>
        public void UnequipAttachment(string name)
        {
            var activeAttachment = GetActiveAttachment(name);

            if (IsServer && activeAttachment != null)
            {
                ActiveAttachments.Remove(activeAttachment);
            }

            var attachment = GetAttachment(name);
            attachment.Unequip(this);
        }

        [ClientRpc]
        public void UnequipAttachmentCL(string name)
        {
            UnequipAttachment(name);
        }

        /// <summary>
        /// Tries to equip attachment on client after server created a valid networked attachment
        /// </summary>
        [ClientRpc]
        protected void EquipAttachmentCL(string name)
        {
            var instanceID = InstanceID;
            _ = TryEquipAttachmentCL(name, instanceID);
        }

        async Task TryEquipAttachmentCL(string name, int instanceID)
        {
            Host.AssertClient();

            var activeAttachment = GetActiveAttachment(name);

            if (activeAttachment != null)
            {
                EquipAttachment(activeAttachment);
                return;
            }

            var activeWeapon = Owner.ActiveChild;
            await GameTask.DelaySeconds(0.05f);
            if (!IsAsyncValid(activeWeapon, instanceID)) return;
            _ = TryEquipAttachmentCL(name, instanceID);
        }

        /// <summary>
        /// Request server to equip an attachment.
        /// </summary>
        public void EquipAttachmentSV(string name)
        {
            // Server
            EquipAttachment(name);

            // Client
            EquipAttachmentCL(To.Single(Owner), name);
        }

        /// <summary>
        /// Request server to unequip an attachment.
        /// </summary>
        public void UnequipAttachmentSV(string name)
        {
            // Server
            UnequipAttachment(name);

            // Client
            UnequipAttachmentCL(To.Single(Owner), name);
        }

        [ClientRpc]
        protected void HandleAttachmentsRecheckCL()
        {
            var instanceID = InstanceID;
            _ = TryHandleAttachmentsCL(instanceID);
        }

        async Task TryHandleAttachmentsCL(int instanceID)
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
            _ = TryHandleAttachmentsCL(instanceID);
        }

        /// <summary>
        /// Handles forced and active attachments.
        /// </summary>
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
            return GetActiveAttachmentFromCategory(attachmentCategoryName.ToString());
        }

        public virtual ActiveAttachment GetActiveAttachmentFromCategory(string attachmentCategoryName)
        {
            if (AttachmentCategories != null)
            {
                foreach (var activeAttachment in ActiveAttachments)
                {
                    if (activeAttachment.Category.ToString() == attachmentCategoryName)
                    {
                        return activeAttachment;
                    }
                }
            }

            return null;
        }
    }
}
