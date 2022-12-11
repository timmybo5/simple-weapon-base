using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sandbox;
using Sandbox.UI;

namespace SWB_Base.Editor;

public partial class AttachmentEditorMenu
{
    public float X { get; set; } = 0f;
    public float Y { get; set; } = 0f;
    public float Z { get; set; } = 0f;
    public float Pitch { get; set; } = 0f;
    public float Yaw { get; set; } = 0f;
    public float Roll { get; set; } = 0f;
    public float Scale { get; set; } = 1f;
    public string BoneName { get; set; }

    public TextEntry ModelInput { get; set; }
    public DropDown BoneDropDown { get; set; }
    public Button ViewModelButton { get; set; }
    public Button WorldModelButton { get; set; }
    public ModelDisplay ModelDisplay { get; set; }

    public Label DragModeLabel { get; set; }
    private DragMode dragMode = DragMode.pos;

    private AttachmentModel activeModel;
    private WeaponBase activeWeapon;
    private List<SceneObject> activeWorldAttachments = new List<SceneObject>();

    private Angles defaultModelAngles;

    private string modelPlaceholder = "path/to/attachment.vmdl";

    private bool isValidAttachment;
    private bool isEditingViewModel = true;

    private float startX;
    private float startY;
    private float xOrigin;
    private float zOrigin;
    private float pitchOrigin;
    private float yawOrigin;

    private string[] blacklistedBones = new string[] {
        "l_",
        "r_",
        "v_",
        "c_"
    };

    protected override void OnAfterTreeRender(bool firstTime)
    {
        if (!firstTime) return;

        DragModeLabel.Text = "x/z";

        // Weapon Model
        if (Game.LocalPawn is not PlayerBase player) return;

        activeWeapon = player.ActiveChild as WeaponBase;

        if (activeWeapon != null)
        {
            var viewModel = activeWeapon.ViewModelEntity;

            FillBoneList(viewModel.Model);
        }

        // ModelInput
        ModelInput.Text = "attachments/swb/muzzle/silencer_pistol/silencer_pistol.vmdl"; //modelPlaceholder;
        ModelInput.AddEventListener("onmousedown", () =>
        {
            if (ModelInput.Text == modelPlaceholder)
            {
                ModelInput.Text = "";
            }
        });

        ModelInput.AddEventListener("value.changed", () => OnModelChange(ModelInput.Text));

        // BoneDropDown
        BoneDropDown.AddEventListener("value.changed", () => CreateAttachmentModels());
    }

    public void FillBoneList(Model model)
    {
        BoneDropDown.Selected = new Option("", -1);
        BoneDropDown.Options.Clear();

        for (var i = 0; i < model.BoneCount; i++)
        {
            var name = model.GetBoneName(i);
            var canUseBone = true;

            foreach (string word in blacklistedBones)
            {
                if (name.StartsWith(word))
                {
                    canUseBone = false;
                    break;
                }
            }

            if (canUseBone)
                BoneDropDown.Options.Add(new Option(name, name));
        }
    }

    // No dragging if not directly on base panel
    public bool CanDragOnPanel(Panel p)
    {
        return p.ElementName == "attachmenteditormenu" || p.ElementName == "scenepanel";
    }

    protected override void OnMouseMove(MousePanelEvent e)
    {
        base.OnMouseMove(e);

        if (!HasActive || !CanDragOnPanel(e.Target)) return;

        if (dragMode == DragMode.pos)
        {

            X = xOrigin - (startX - Mouse.Position.x) * 0.001f;
            Z = zOrigin + (startY - Mouse.Position.y) * 0.001f;
        }
        else
        {
            Yaw = yawOrigin + (startX - Mouse.Position.x) * 0.01f;
            Pitch = pitchOrigin - (startY - Mouse.Position.y) * 0.01f;
        }

        SkipTransitions();
        e.StopPropagation();
    }

    protected override void OnRightClick(MousePanelEvent e)
    {
        if (dragMode == DragMode.pos)
        {
            DragModeLabel.Text = "yaw/pitch";
            dragMode = DragMode.angle;
        }
        else
        {
            DragModeLabel.Text = "x/z";
            dragMode = DragMode.pos;
        }
    }

    protected override void OnMouseDown(MousePanelEvent e)
    {
        base.OnMouseDown(e);

        if (!CanDragOnPanel(e.Target)) return;

        startX = Mouse.Position.x;
        startY = Mouse.Position.y;

        xOrigin = X;
        zOrigin = Z;
        pitchOrigin = Pitch;
        yawOrigin = Yaw;

        e.StopPropagation();
    }

    public void OnClose()
    {
        // Cleanup
        if (activeModel != null)
        {
            activeModel.Delete();
            activeModel = null;
        }

        if (activeWeapon != null && activeWeapon.ViewModelEntity != null && !isEditingViewModel)
        {
            activeWeapon.ViewModelEntity.EnableDrawing = true;
        }
    }

    private void CreateAttachmentModels()
    {
        if (!isEditingViewModel && ModelDisplay != null)
        {
            // World Model
            foreach (var sceneObj in activeWorldAttachments)
            {
                sceneObj.Delete();
            }

            // Create active attachment models on worlmodel
            foreach (var activeAttach in activeWeapon.ActiveAttachments)
            {
                var attach = (OffsetAttachment)activeWeapon.GetAttachment(activeAttach.Name);
                CreateAttachmentModel(attach.ModelPath, attach.WorldTransform, attach.WorldParentBone);
            }
        }

        // Create attachment model
        var pos = new Vector3(X, Y, Z);
        var ang = new Angles(Pitch, Yaw, Roll);
        var transform = new Transform(pos, Rotation.From(ang), Scale);

        CreateAttachmentModel(ModelInput.Text, transform, BoneName);
    }

    private void CreateAttachmentModel(string model, Transform transform, string bone)
    {
        if (!isValidAttachment || activeWeapon == null || bone == null || bone == "-1") return;

        if (isEditingViewModel)
        {
            // View Model
            if (activeModel != null)
            {
                activeModel.Delete();
                activeModel = null;
            }

            activeModel = new AttachmentModel(true);
            activeModel.Owner = activeWeapon.Owner;
            activeModel.SetModel(model);
            activeModel.SetParent(activeWeapon.ViewModelEntity, bone, transform);
        }
        else if (ModelDisplay != null)
        {
            // Bone base offsets
            var weaponTrans = activeWeapon.Transform;
            var attachTrans = activeWeapon.GetBoneTransform(bone);
            var editorOffsetPos = attachTrans.PointToWorld(transform.Position);
            var boneOffsetPos = weaponTrans.PointToLocal(editorOffsetPos);
            var boneOffetRot = weaponTrans.RotationToLocal(attachTrans.Rotation);

            var attachRot = boneOffetRot * transform.Rotation;
            var attachPos = boneOffsetPos;
            var modelTransform = new Transform(attachPos, attachRot, transform.Scale);

            var sceneObject = new SceneModel(ModelDisplay.SceneWorld, model, modelTransform);
            activeWorldAttachments.Add(sceneObject);
            ModelDisplay.SceneObject.AddChild("attach" + activeWorldAttachments.Count, sceneObject);
        }
    }

    public void OnModelChange(string modelPath)
    {
        var rx = new Regex(@"\.vmdl$", RegexOptions.IgnoreCase);

        if (!rx.IsMatch(modelPath) || (!FileSystem.Mounted.FileExists(modelPath) && !FileSystem.Mounted.FileExists(modelPath + "_c")))
        {
            ModelInput.AddClass("invalid");
            isValidAttachment = false;
            return;
        }

        isValidAttachment = true;
        ModelInput.AddClass("valid");
        CreateAttachmentModels();
    }

    public void OnSliderChange()
    {
        CreateAttachmentModels();
    }

    public void OnReset()
    {
        X = 0;
        Y = 0;
        Z = 0;
        Pitch = 0;
        Yaw = 0;
        Roll = 0;
        Scale = 1f;
    }

    public void OnCopy()
    {
        var dataStr = String.Format("new Transform {{ Position = new Vector3({3:0.###}f, {4:0.###}f, {5:0.###}f), Rotation = Rotation.From(new Angles({0:0.###}f, {1:0.###}f, {2:0.###}f)), Scale = {6:0.###}f }},", Pitch, Yaw, Roll, X, Y, Z, Scale);
        Clipboard.SetText(dataStr);
    }

    // Model switching
    public void OnEditViewModel()
    {
        if (isEditingViewModel) return;
        isEditingViewModel = true;

        ViewModelButton.SetClass("selected", true);
        WorldModelButton.SetClass("selected", false);

        activeWeapon.ViewModelEntity.EnableDrawing = true;

        if (ModelDisplay != null)
        {
            ModelDisplay.Delete(true);
            ModelDisplay = null;
        }

        FillBoneList(activeWeapon.ViewModelEntity.Model);
        CreateAttachmentModels();
    }

    public void OnEditWorldModel()
    {
        if (!isEditingViewModel) return;
        isEditingViewModel = false;

        ViewModelButton.SetClass("selected", false);
        WorldModelButton.SetClass("selected", true);

        activeWeapon.ViewModelEntity.EnableDrawing = false;

        ModelDisplay = new ModelDisplay(activeWeapon.WorldModelPath);
        ModelDisplay.Parent = this;
        defaultModelAngles = ModelDisplay.CamAngles;

        FillBoneList(activeWeapon.Model);
        CreateAttachmentModels();
    }


    // Weapon Rotation
    private int leftRotation = 0;
    private int rightRotation = 0;
    private int upRotation = 0;
    private int downRotation = 0;

    private int worldModelRotationAmount = 15;

    public void OnRotateReset()
    {
        if (activeWeapon == null) return;
        if (isEditingViewModel && activeWeapon.ViewModelEntity is ViewModelBase viewModel)
        {
            viewModel.EditorOffset = new AngPos();
        }
        else if (ModelDisplay != null)
        {
            ModelDisplay.CamAngles = defaultModelAngles;
        }

        leftRotation = 0;
        rightRotation = 0;
        upRotation = 0;
        downRotation = 0;
    }

    private void RotateWeapon(AngPos offset)
    {
        if (activeWeapon == null) return;
        if (isEditingViewModel && activeWeapon.ViewModelEntity is ViewModelBase viewModel)
        {
            viewModel.EditorOffset += offset;
        }
        else if (ModelDisplay != null)
        {
            ModelDisplay.CamAngles += offset.Angle;
        }
    }

    public void OnRotateLeft()
    {
        if (rightRotation != 0 && isEditingViewModel)
            OnRotateReset();

        leftRotation += 2;

        RotateWeapon(new AngPos
        {
            Angle = new Angles(0, isEditingViewModel ? 45 : -worldModelRotationAmount, 0),
            Pos = new Vector3(17, -5 * leftRotation, 0),
        });
    }

    public void OnRotateRight()
    {
        if (leftRotation != 0 && isEditingViewModel)
            OnRotateReset();

        rightRotation++;

        RotateWeapon(new AngPos
        {
            Angle = new Angles(0, isEditingViewModel ? -45 : worldModelRotationAmount, 0),
            Pos = new Vector3(-17 * rightRotation, -5 * rightRotation, 0),
        });
    }

    public void OnRotateUp()
    {
        if (downRotation != 0 && isEditingViewModel)
            OnRotateReset();

        upRotation++;

        RotateWeapon(new AngPos
        {
            Angle = new Angles(isEditingViewModel ? -45 : worldModelRotationAmount, 0, 0),
            Pos = new Vector3(0, -5 * upRotation, -15),
        });
    }

    public void OnRotateDown()
    {
        if (upRotation != 0 && isEditingViewModel)
            OnRotateReset();

        downRotation++;

        RotateWeapon(new AngPos
        {
            Angle = new Angles(isEditingViewModel ? 45 : -worldModelRotationAmount, 0, 0),
            Pos = new Vector3(0, -5 * downRotation, 20),
        });
    }

    protected override int BuildHash()
    {
        return HashCode.Combine(DateTime.Now.ToString());
    }
}
