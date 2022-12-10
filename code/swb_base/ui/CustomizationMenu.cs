using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB_Base.Translations;

namespace SWB_Base.UI;

[UseTemplate]
public class CustomizationMenu : Panel
{
    public Panel Menu { get; set; }
    public Panel Categories { get; set; }
    public Label WeaponName { get; set; }

    private WeaponBase activeWeapon;
    private Panel activeCategoryP;
    private string activeCategoryPName;
    private Panel activeSelectionMenu;
    private Panel activeInfoP;
    private string activeInfoPName;

    private Dictionary<string, Panel> activeAttachmentPanels = new Dictionary<string, Panel>();

    private AttachmentModel previewModel;
    private Material previewMaterial;

    private int catActiveAttachPIndex;
    private int catActiveAttachPLabelIndex;
    private int catActiveAttachPIconWrapperIndex;
    private Translator translator;

    public CustomizationMenu() : base()
    {
        var player = Game.LocalPawn as PlayerBase;
        if (player == null) return;

        activeWeapon = player.ActiveChild as WeaponBase;

        if (activeWeapon == null || activeWeapon.AttachmentCategories == null)
        {
            Delete(); // True will not delete the base panel
            return;
        }

        translator = Translator.GetInstance();
        previewMaterial = Material.Load("materials/swb/attachments/preview.vmat");
        WeaponName.Text = activeWeapon.PrintName;
        activeWeapon.AttachmentCategories.Sort();

        // Add categories
        foreach (var cat in activeWeapon.AttachmentCategories)
        {
            // Do not list empty categories
            if (cat.Attachments == null || cat.Attachments.Count == 0 || !cat.Selectable)
            {
                continue;
            }

            var categoryP = new Panel(Categories, "category");
            var catName = cat.Name.ToString();
            categoryP.Add.Label(catName, "name");
            var catActiveAttachP = categoryP.Add.Panel("activeAttachment");
            var attachLabel = catActiveAttachP.Add.Label("", "attachName");
            var iconWrapperP = catActiveAttachP.Add.Panel("iconWrapper");
            var attachIcon = iconWrapperP.Add.Image("", "icon");

            catActiveAttachPIndex = categoryP.GetChildIndex(catActiveAttachP);
            catActiveAttachPLabelIndex = catActiveAttachP.GetChildIndex(attachLabel);
            catActiveAttachPIconWrapperIndex = catActiveAttachP.GetChildIndex(iconWrapperP);

            // Add active attachments
            UpdateCategory(categoryP, catName);

            // Onclick
            categoryP.AddEventListener("onmousedown", () =>
            {
                PlaySound("swb_click");

                if (activeCategoryP != categoryP)
                {
                    OnCategorySelect(categoryP, cat, catName);
                }
                else
                {
                    CloseActiveCategory();
                }
            });

            categoryP.AddEventListener("onmouseover", () =>
            {
                PlaySound("ui.button.over");
            });
        }

        // Add stats
        Menu.AddChild(new StatsDisplay(activeWeapon));
    }

    private void CloseActiveCategory()
    {
        if (activeCategoryP != null)
        {
            activeCategoryP.SetClass("active", false);
            activeCategoryP = null;
            activeCategoryPName = null;
        }

        if (activeSelectionMenu != null)
        {
            activeSelectionMenu.Delete(true);
            activeSelectionMenu = null;
        }

        RemoveInfoPanel();
        RemovePreviewModel();
    }

    private void SetCategoryAttachment(Panel catP, bool hasAttach, AttachmentBase attach = null)
    {
        if (!hasAttach)
        {
            catP.SetClass("hasAttachment", false);
            return;
        }

        var attachP = catP.GetChild(catActiveAttachPIndex);
        Label attachLabel = (Label)attachP.GetChild(catActiveAttachPLabelIndex);
        var attachIconWrapper = attachP.GetChild(catActiveAttachPIconWrapperIndex);
        var attachIcon = (Image)attachIconWrapper.GetChild(0);

        attachLabel.Text = attach.Name;
        attachIcon.SetTexture(attach.IconPath);

        catP.SetClass("hasAttachment", true);
    }

    private void UpdateCategory(Panel catP, string catName)
    {
        // Update category attachments
        var activeAttach = activeWeapon.GetActiveAttachmentFromCategory(catName);
        if (activeAttach == null)
        {
            SetCategoryAttachment(catP, false);
            return;
        }

        var activeAttachBase = activeWeapon.GetAttachment(activeAttach.Name);
        if (activeAttachBase == null)
        {
            SetCategoryAttachment(catP, false);
            return;
        }

        SetCategoryAttachment(catP, true, activeAttachBase);
    }

    private void OnCategorySelect(Panel categoryP, AttachmentCategory cat, string catName)
    {
        if (categoryP == activeCategoryP) return;

        CloseActiveCategory();

        // Set button active
        categoryP.SetClass("active", true);
        activeCategoryP = categoryP;
        activeCategoryPName = catName;

        // Open attachment selection
        activeSelectionMenu = new Panel(this, "selectionMenu");

        cat.Attachments.Sort();

        // List attachments
        foreach (var attach in cat.Attachments)
        {
            var attachmentP = new Panel(activeSelectionMenu, "attachment");
            attachmentP.Add.Label(attach.Name, "name");
            var iconWrapperP = attachmentP.Add.Panel("iconWrapper");
            iconWrapperP.Add.Image(attach.IconPath, "icon");

            // Onclick
            attachmentP.AddEventListener("onmousedown", () =>
            {
                OnAttachmentSelect(attachmentP, attach, catName);
            });

            attachmentP.AddEventListener("onmouseover", () =>
            {
                PlaySound("ui.button.over");
                OnAttachmentHover(attachmentP, attach, catName);
            });

            attachmentP.AddEventListener("onmouseout", () =>
            {
                OnAttachmentHoverStop(attachmentP, attach, catName);
            });

            // Check if already equipped
            if (activeWeapon.GetActiveAttachment(attach.Name) != null)
            {
                OnAttachmentSelect(attachmentP, attach, catName, true);
            }
        }
    }

    private void OnAttachmentSelect(Panel attachmentP, AttachmentBase attach, string catName, bool isEquipped = false)
    {
        Panel activeAttachP;
        if (activeAttachmentPanels.TryGetValue(catName, out activeAttachP))
        {
            bool isDisable = activeAttachP == attachmentP;

            activeAttachP.SetClass("active", false);
            activeAttachmentPanels.Remove(catName);

            RemoveInfoPanel();

            if (isDisable)
            {
                // Unequip attachment
                var activeAttach = activeWeapon.GetActiveAttachmentFromCategory(catName);
                if (activeAttach != null)
                {
                    ConsoleSystem.Run("swb_attachment_equip", activeAttach.Name, false);
                    SetCategoryAttachment(activeCategoryP, false);
                }

                activeWeapon.PlaySound("swb_unequip");
                OnAttachmentHover(attachmentP, attach, catName);
                return;
            }
        }

        attachmentP.SetClass("active", true);
        activeAttachmentPanels.Add(catName, attachmentP);

        // Display extra information
        CreateInfoPanel(attach);

        // Equip new attachment
        if (!isEquipped)
        {
            RemovePreviewModel();
            activeWeapon.PlaySound("swb_equip");
            ConsoleSystem.Run("swb_attachment_equip", attach.Name, true);
            SetCategoryAttachment(activeCategoryP, true, attach);
        }
    }

    private void OnAttachmentHover(Panel attachmentP, AttachmentBase attach, string catName)
    {
        if (activeInfoPName == attach.Name) return;

        CreateInfoPanel(attach);
        CreatePreviewModel(attach);

        // Hide equipped model (to show preview)
        if (activeAttachmentPanels.ContainsKey(catName))
        {
            var activeAttach = activeWeapon.GetActiveAttachmentFromCategory(catName);
            if (activeAttach == null) return;

            // Hide equipped model
            activeAttach.ViewAttachmentModel.RenderColor = Color.Transparent;
        }
    }

    private void OnAttachmentHoverStop(Panel attachmentP, AttachmentBase attach, string catName)
    {
        if (activeInfoPName != attach.Name) return;

        RemoveInfoPanel();
        RemovePreviewModel();

        // Reopen the active attachment info
        if (activeAttachmentPanels.ContainsKey(catName))
        {
            var activeAttach = activeWeapon.GetActiveAttachmentFromCategory(catName);
            if (activeAttach == null) return;

            var activeAttachBase = activeWeapon.GetAttachment(activeAttach.Name);
            if (activeAttachBase == null) return;

            CreateInfoPanel(activeAttachBase);

            // Unhide equipped model
            activeAttach.ViewAttachmentModel.RenderColor = Color.White;
        }
    }

    private void AddStatModifierTranslation(Panel parent, string key, float value)
    {
        var realKey = value > 0 ? key + "Pos" : key + "Neg";
        var translation = translator.Translate(realKey, (value * 100).ToString() + "%");
        parent.Add.Label("> " + translation, "label");
    }

    private void AddStatModifierTranslations(Panel parent, AttachmentBase attach, bool isPositives)
    {
        if (attach.StatModifier == null) return;

        if ((isPositives && attach.StatModifier.Spread < 0) || (!isPositives && attach.StatModifier.Spread > 0))
            AddStatModifierTranslation(parent, "StatModifierAccuracy", attach.StatModifier.Spread * -1);

        if ((isPositives && attach.StatModifier.Damage > 0) || (!isPositives && attach.StatModifier.Damage < 0))
            AddStatModifierTranslation(parent, "StatModifierDamage", attach.StatModifier.Damage);

        if ((isPositives && attach.StatModifier.RPM > 0) || (!isPositives && attach.StatModifier.RPM < 0))
            AddStatModifierTranslation(parent, "StatModifierRPM", attach.StatModifier.RPM);

        if ((isPositives && attach.StatModifier.BulletVelocity > 0) || (!isPositives && attach.StatModifier.BulletVelocity < 0))
            AddStatModifierTranslation(parent, "StatModifierBulletVelocity", attach.StatModifier.BulletVelocity);

        if ((isPositives && attach.StatModifier.Recoil < 0) || (!isPositives && attach.StatModifier.Recoil > 0))
            AddStatModifierTranslation(parent, "StatModifierRecoil", attach.StatModifier.Recoil * -1);
    }

    private void CreateInfoPanel(AttachmentBase attach)
    {
        RemoveInfoPanel();

        activeInfoPName = attach.Name;
        activeInfoP = new Panel(this, "info");
        activeInfoP.Add.Label(attach.Description, "description");

        var positives = new Panel(activeInfoP, "positives");
        foreach (var pos in attach.Positives)
        {
            positives.Add.Label("> " + pos, "label");
        }
        AddStatModifierTranslations(positives, attach, true);

        var negatives = new Panel(activeInfoP, "negatives");
        foreach (var neg in attach.Negatives)
        {
            negatives.Add.Label("> " + neg, "label");
        }
        AddStatModifierTranslations(negatives, attach, false);
    }

    private void RemoveInfoPanel()
    {
        if (activeInfoP != null)
        {
            activeInfoP.Delete(true);
            activeInfoP = null;
            activeInfoPName = "";
        }
    }

    private void CreatePreviewModel(AttachmentBase attach)
    {
        // Remove old model
        RemovePreviewModel();

        // Create preview model
        previewModel = attach.CreateModel(activeWeapon);
        previewModel.SetMaterialOverride(previewMaterial);
    }

    private void RemovePreviewModel()
    {
        if (previewModel != null)
        {
            previewModel.Delete();
            previewModel = null;
        }
    }

    public override void OnDeleted()
    {
        // Make sure active attachment is visible
        var activeAttach = activeWeapon.GetActiveAttachmentFromCategory(activeCategoryPName);
        if (activeAttach != null)
            activeAttach.ViewAttachmentModel.RenderColor = Color.White;


        // RemovePreviewModel
        RemovePreviewModel();
    }

    public override void Tick()
    {
        var player = Game.LocalPawn as PlayerBase;
        if (player == null) return;

        if (activeWeapon == null || activeWeapon != player.ActiveChild as WeaponBase)
        {
            Delete(true);
            return;
        }

        /*
        // Bone/Attach indication
        if (activeCategoryPName == null) return;

        var activeCategory = activeWeapon.GetAttachmentCategory(activeCategoryPName);
        if (activeCategory == null) return;

        var activeViewModel = activeWeapon.ViewModelEntity;
        if (activeViewModel == null) return;

        var startTransform = activeViewModel.GetAttachment(activeCategory.BoneOrAttachment);
        if (startTransform == null)
        {
            // Use bone if model attachment not found
            var boneIndex = activeViewModel.GetBoneIndex(activeCategory.BoneOrAttachment);
            startTransform = activeWeapon.GetBoneTransform(boneIndex);
        }
        if (startTransform == null) return;

        startTransform = startTransform.GetValueOrDefault();
        var line1StartPos = startTransform.Value.Position;
        var line1EndPos = MathUtil.RelativeAdd(line1StartPos, new Vector3(-2, 2, 0), activeViewModel.Rotation);
        var line2EndPos = MathUtil.RelativeAdd(line1EndPos, new Vector3(-3, 0, 0), activeViewModel.Rotation);

        DebugOverlay.Line(line1StartPos, line1EndPos, Color.White, 0, false);
        DebugOverlay.Line(line1EndPos, line2EndPos, Color.White, 0, false);
        */
    }
}
