using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SWB_Base.Editor
{
    [Library("swb_Popup")]
    public partial class Popup : Panel
    {
        // For keyboard navigation
        public Panel PopupSource { get; set; }
        public Panel SelectedChild { get; set; }

        public PositionMode Position { get; set; }
        public float PopupSourceOffset { get; set; }

        public enum PositionMode
        {
            AboveLeft,
            BelowLeft,
            BelowCenter,
            BelowStretch
        }

        public Popup(Panel sourcePanel, PositionMode position, float offset)
        {
            StyleSheet.Load("/swb_base/editor/ui/components/Popup.scss");

            Parent = sourcePanel.FindPopupPanel();
            PopupSource = sourcePanel;
            Position = position;
            PopupSourceOffset = offset;

            AllPopups.Add(this);
            AddClass("popup-panel");
            PositionMe();

            switch (Position)
            {
                case PositionMode.AboveLeft:
                    AddClass("above-left");
                    break;

                case PositionMode.BelowLeft:
                    AddClass("below-left");
                    break;

                case PositionMode.BelowCenter:
                    AddClass("below-center");
                    break;

                case PositionMode.BelowStretch:
                    AddClass("below-stretch");
                    break;
            }
        }

        public override void OnDeleted()
        {
            base.OnDeleted();

            AllPopups.Remove(this);
        }


        protected Panel Header;
        protected Label TitleLabel;
        protected IconPanel IconPanel;

        void CreateHeader()
        {
            if (Header != null) return;

            Header = Add.Panel("header");

            IconPanel = Header.Add.Icon(null);
            TitleLabel = Header.Add.Label(null, "title");
        }


        public string Title
        {
            get => TitleLabel?.Text;
            set
            {
                CreateHeader();
                TitleLabel.Text = value;
            }
        }

        public string Icon
        {
            get => IconPanel?.Text;
            set
            {
                CreateHeader();
                IconPanel.Text = value;
            }
        }

        /// <summary>
        /// Closes all panels, marks this one as a success and closes it.
        /// </summary>
        public void Success()
        {
            AddClass("success");
            Popup.CloseAll();
        }

        /// <summary>
        /// Closes all panels, marks this one as a failure and closes it.
        /// </summary>
        public void Failure()
        {
            AddClass("failure");
            Popup.CloseAll();
        }

        public Panel AddOption(string text, Action action = null)
        {
            return Add.Button(text, () =>
           {
               CloseAll();
               action?.Invoke();
           });
        }

        public Panel AddOption(string text, string icon, Action action = null)
        {
            return Add.ButtonWithIcon(text, icon, null, () =>
           {
               CloseAll();
               action?.Invoke();
           });
        }

        public void MoveSelection(int dir)
        {
            var currentIndex = GetChildIndex(SelectedChild);

            if (currentIndex >= 0) currentIndex += dir;
            else if (currentIndex < 0) currentIndex = dir == 1 ? 0 : -1;

            SelectedChild?.SetClass("active", false);
            SelectedChild = GetChild(currentIndex, true);
            SelectedChild?.SetClass("active", true);
        }

        public override void Tick()
        {
            base.Tick();

            PositionMe();
        }

        public override void OnLayout(ref Rect layoutRect)
        {
            var padding = 10;
            var h = Screen.Height - padding;
            var w = Screen.Width - padding;

            if (layoutRect.bottom > h)
            {
                layoutRect.top -= layoutRect.bottom - h;
                layoutRect.bottom -= layoutRect.bottom - h;
            }

            if (layoutRect.right > w)
            {
                layoutRect.left -= layoutRect.right - w;
                layoutRect.right -= layoutRect.right - w;
            }
        }

        void PositionMe()
        {
            var rect = PopupSource.Box.Rect * PopupSource.ScaleFromScreen;

            Style.MaxHeight = Screen.Height - 50;

            switch (Position)
            {
                case PositionMode.AboveLeft:
                    {
                        Style.Left = rect.left;
                        Style.Bottom = Parent.Box.Rect.Height - rect.top + PopupSourceOffset;
                        Style.BackgroundColor = Color.Red;
                        break;
                    }

                case PositionMode.BelowLeft:
                    {
                        Style.Left = rect.left;
                        Style.Top = rect.bottom + PopupSourceOffset;
                        break;
                    }

                case PositionMode.BelowCenter:
                    {
                        Style.Left = rect.Center.x; // centering is done via styles
                        Style.Top = rect.bottom + PopupSourceOffset;
                        break;
                    }

                case PositionMode.BelowStretch:
                    {
                        Style.Left = rect.left;
                        Style.Width = rect.Width;
                        Style.Top = rect.bottom + PopupSourceOffset;
                        break;
                    }
            }

            Style.Dirty();
        }

    }


}
