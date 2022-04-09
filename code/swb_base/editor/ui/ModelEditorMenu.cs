using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB_Base;
using SWB_Base.Attachments;

namespace SWB_Base.Editor
{
    [UseTemplate]
    public class ModelEditorMenu : Panel
    {
        private enum DragMode
        {
            pos = 0,
            angle = 1,
        }

        public virtual bool InvertedX => false;

        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Z { get; set; } = 0f;

        public float Pitch { get; set; } = 0f;
        public float Yaw { get; set; } = 0f;
        public float Roll { get; set; } = 0f;
        public float Sensitivity { get; set; } = 0f;

        public Label DragModeLabel { get; set; }
        public Button AttachmentAnimDataButton { get; set; }

        private DragMode dragMode = DragMode.pos;

        private float startX;
        private float startY;
        private float xOrigin;
        private float zOrigin;
        private float pitchOrigin;
        private float yawOrigin;
        private AngPos sightAttachmentAnimData;

        private WeaponBase activeWeapon;

        public ModelEditorMenu()
        {
            DragModeLabel.Text = "x/z";

            // Get data from active sight attachment
            var showButton = false;
            var player = Local.Pawn as PlayerBase;
            var weapon = player.ActiveChild as WeaponBase;
            var activeAttach = weapon.GetActiveAttachmentFromCategory(AttachmentCategoryName.Sight);

            if (activeAttach != null && AttachmentAnimDataButton != null)
            {
                var attach = weapon.GetAttachment(activeAttach.Name);
                if (attach is Sight sight)
                {
                    showButton = true;
                    AttachmentAnimDataButton.Text = sight.Name;
                    sightAttachmentAnimData = sight.ZoomAnimData;
                }
            }

            if (!showButton && AttachmentAnimDataButton != null)
                AttachmentAnimDataButton.Delete();
        }

        public virtual void OnReset()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Pitch = 0;
            Yaw = 0;
            Roll = 0;
        }

        public virtual void OnCopy()
        {
            var dataStr = String.Format("new AngPos {{ Angle = new Angles({0:0.###}f, {1:0.###}f, {2:0.###}f), Pos = new Vector3({3:0.###}f, {4:0.###}f, {5:0.###}f) }};", Pitch, Yaw, Roll, X, Y, Z);
            Clipboard.SetText(dataStr);
        }

        // No dragging if not directly on base panel
        public virtual bool CanDragOnPanel(Panel p)
        {
            return p.ElementName == "modeleditormenu";
        }

        public void SetZoomAnimData()
        {
            SetFromAngPos(activeWeapon.ZoomAnimData);
        }

        public void SetRunAnimData()
        {
            SetFromAngPos(activeWeapon.RunAnimData);
        }

        public void SetCustomizeAnimData()
        {
            SetFromAngPos(activeWeapon.CustomizeAnimData);
        }

        public void SetSightAttachmentAnimData()
        {
            SetFromAngPos(sightAttachmentAnimData);
        }

        private void SetFromAngPos(AngPos angPos)
        {
            X = angPos.Pos.x;
            Y = angPos.Pos.y;
            Z = angPos.Pos.z;
            Pitch = angPos.Angle.pitch;
            Yaw = angPos.Angle.yaw;
            Roll = angPos.Angle.roll;
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

        public override void Tick()
        {
            var player = Local.Pawn as PlayerBase;
            if (player == null) return;

            activeWeapon = player.ActiveChild as WeaponBase;
            bool isValidWeapon = activeWeapon != null;

            if (!isValidWeapon) return;

            if (activeWeapon.ViewModelEntity is ViewModelBase viewModel)
            {
                viewModel.EditorOffset = new AngPos
                {
                    Angle = new Angles(Pitch, Yaw, Roll),
                    Pos = new Vector3(X, Y, Z)
                };
            }
        }
    }
}
