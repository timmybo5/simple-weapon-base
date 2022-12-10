using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;

namespace SWB_Base
{
    public class AnimatedAction
    {
        public List<InputButton> ActionButtons { get; set; }
        public string OnAnimation { get; set; }
        public float OnAnimationDuration { get; set; } = 2f;
        public string OffAnimation { get; set; }
        public float OffAnimationDuration { get; set; } = 2f;
        public string AnimationStatus { get; set; }

        public string NewViewModel { get; set; }
        public string NewWorldModel { get; set; }
        public string NewShootSound { get; set; }

        private ClipInfo clipInfo = new ClipInfo();
        private float canNextHandle = 0f;
        public bool isToggled = false;

        private void HandleChanges(WeaponBase weaponBase)
        {
            if (!string.IsNullOrEmpty(NewShootSound))
            {
                if (isToggled)
                    clipInfo.ShootSound = weaponBase.Primary.ShootSound;

                weaponBase.Primary.ShootSound = isToggled ? NewShootSound : clipInfo.ShootSound;
            }

            if (!string.IsNullOrEmpty(NewViewModel))
                weaponBase?.ViewModelEntity?.SetModel(isToggled ? NewViewModel : weaponBase.ViewModelPath);

            if (!string.IsNullOrEmpty(NewWorldModel))
                weaponBase.SetModel(isToggled ? NewWorldModel : weaponBase.WorldModelPath);
        }

        public void HandleOnDeploy(WeaponBase weaponBase)
        {
            weaponBase.SendWeaponAnim(AnimationStatus);
        }

        async private Task ResetAnimating(WeaponBase weaponBase, float delay)
        {
            await GameTask.DelaySeconds(delay);
            weaponBase.IsAnimating = false;
        }

        public bool Handle(IClient owner, WeaponBase weaponBase)
        {
            if (RealTime.Now < canNextHandle) return false;

            // Check if animated keys are down
            for (int i = 0; i < ActionButtons.Count; i++)
            {
                if (!Input.Down(ActionButtons[i]))
                    return false;

                // Reload will fuck with animations, IsReload is still false here
                if (ActionButtons[i] == InputButton.Reload && weaponBase.Primary.Ammo < weaponBase.Primary.ClipSize)
                    return false;
            }

            isToggled = !isToggled;

            var canNextAnimateDelay = isToggled ? OnAnimationDuration : OffAnimationDuration;
            canNextHandle = RealTime.Now + canNextAnimateDelay;

            // Reset animating after the delay
            weaponBase.IsAnimating = true;
            _ = ResetAnimating(weaponBase, canNextAnimateDelay);

            // Handle shared changes
            HandleChanges(weaponBase);

            if (Game.IsClient)
            {
                var viewModelEntity = weaponBase.ViewModelEntity;

                if (isToggled)
                {
                    viewModelEntity?.SetAnimParameter(OnAnimation, true);
                }
                else
                {
                    viewModelEntity?.SetAnimParameter(OffAnimation, true);
                }

                viewModelEntity?.SetAnimParameter(AnimationStatus, isToggled);
            }

            return true;
        }
    }
}
