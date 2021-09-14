using Sandbox;
using Sandbox.UI;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base
{

    public partial class WeaponBase : CarriableBase
    {
        public override void Spawn()
        {
            base.Spawn();

            CollisionGroup = CollisionGroup.Weapon;
            SetInteractsAs(CollisionLayer.Debris);

            SetModel(WorldModelPath);

            PickupTrigger = new PickupTrigger();
            PickupTrigger.Parent = this;
            PickupTrigger.Position = Position;
        }

        public override void ActiveStart(Entity ent)
        {
            base.ActiveStart(ent);

            TimeSinceDeployed = 0;
            IsReloading = false;

            // Draw animation
            if (IsLocalPawn)
            {
                var activeViewModel = !DualWield ? ViewModelEntity : dualWieldViewModel;

                if (Primary.Ammo == 0 && !string.IsNullOrEmpty(Primary.DrawEmptyAnim))
                {
                    activeViewModel?.SetAnimBool(Primary.DrawEmptyAnim, true);
                }
                else if (!string.IsNullOrEmpty(Primary.DrawAnim))
                {
                    activeViewModel?.SetAnimBool(Primary.DrawAnim, true);
                }
            }

            // Animated activity status will reset when weapon is switched out
            if (AnimatedActions != null)
            {
                for (int i = 0; i < AnimatedActions.Count; i++)
                {
                    if (AnimatedActions[i].isToggled)
                        AnimatedActions[i].HandleOnDeploy(this);
                }
            }

            // Dualwield setup
            if (DualWield)
            {
                if (!isDualWieldConverted)
                {
                    isDualWieldConverted = true;
                    Primary.Ammo *= 2;
                    Primary.ClipSize *= 2;
                    Primary.RPM = (int)(Primary.RPM * 1.25);
                    ZoomAnimData = null;
                    RunAnimData = null;
                }
            }
        }

        public override void ActiveEnd(Entity ent, bool dropped)
        {
            base.ActiveEnd(ent, dropped);

            if (DualWield && dualWieldViewModel != null)
            {
                dualWieldViewModel.Delete();
            }
        }

        // BaseSimulate
        public void BaseSimulate(Client player)
        {
            if (Input.Down(InputButton.Reload))
            {
                Reload();
            }

            // Reload could have deleted us
            if (!this.IsValid())
                return;

            if (CanPrimaryAttack())
            {
                TimeSincePrimaryAttack = 0;
                AttackPrimary();
            }

            // AttackPrimary could have deleted us
            if (!player.IsValid())
                return;

            if (CanSecondaryAttack())
            {
                TimeSinceSecondaryAttack = 0;
                AttackSecondary();
            }
        }

        public override void Simulate(Client owner)
        {

            if (IsAnimating) return;

            // Handle custom animation actions
            if (AnimatedActions != null && !IsReloading)
            {
                for (int i = 0; i < AnimatedActions.Count; i++)
                {
                    if (AnimatedActions[i].Handle(owner, this))
                        return;
                }
            }

            IsRunning = Input.Down(InputButton.Run) && RunAnimData != null && Owner.Velocity.Length >= 200;

            if (Secondary == null && ZoomAnimData != null && !(this is WeaponBaseMelee))
                IsZooming = Input.Down(InputButton.Attack2) && !IsRunning && !IsReloading;

            if (TimeSinceDeployed < 0.6f)
                return;

            if (!IsReloading)
            {
                BaseSimulate(owner);
            }

            if (IsReloading && TimeSinceReload >= 0)
            {
                OnReloadFinish();
            }
        }

        public virtual void Reload()
        {
            if (IsReloading || IsAnimating)
                return;

            var maxClipSize = BulletCocking ? Primary.ClipSize + 1 : Primary.ClipSize;

            if (Primary.Ammo >= maxClipSize || Primary.ClipSize == -1)
                return;

            var isEmptyReload = Primary.ReloadEmptyTime > 0 ? Primary.Ammo == 0 : false;
            TimeSinceReload = -(isEmptyReload ? Primary.ReloadEmptyTime : Primary.ReloadTime);

            if (Owner is PlayerBase player)
            {
                if (player.AmmoCount(Primary.AmmoType) <= 0 && Primary.InfiniteAmmo != InfiniteAmmoType.reserve)
                    return;
            }

            IsReloading = true;

            // Player anim
            (Owner as AnimEntity).SetAnimBool("b_reload", true);

            StartReloadEffects(isEmptyReload);
        }

        public virtual void OnReloadFinish()
        {
            IsReloading = false;

            // Dual wield
            if (DualWield && !dualWieldShouldReload)
            {
                dualWieldShouldReload = true;
                Reload();
                return;
            }

            dualWieldShouldReload = false;

            if (Primary.InfiniteAmmo == InfiniteAmmoType.reserve)
            {
                var newAmmo = Primary.ClipSize;

                if (BulletCocking && Primary.Ammo > 0)
                    newAmmo += 1;

                Primary.Ammo = newAmmo;
                return;
            }

            if (Owner is PlayerBase player)
            {
                var ammo = player.TakeAmmo(Primary.AmmoType, Primary.ClipSize - Primary.Ammo);
                if (ammo == 0)
                    return;

                Primary.Ammo += ammo;
            }
        }

        [ClientRpc]
        public virtual void StartReloadEffects(bool isEmpty)
        {
            var reloadingViewModel = DualWield && dualWieldShouldReload ? dualWieldViewModel : ViewModelEntity;

            if (isEmpty && Primary.ReloadEmptyAnim != null)
            {
                reloadingViewModel?.SetAnimBool(Primary.ReloadEmptyAnim, true);
            }
            else if (Primary.ReloadAnim != null)
            {
                reloadingViewModel?.SetAnimBool(Primary.ReloadAnim, true);
            }

            // TODO - player third person model reload
        }

        public override void BuildInput(InputBuilder input)
        {
            // Mouse sensitivity
            if (IsZooming)
            {
                input.ViewAngles = MathUtil.FILerp(input.OriginalViewAngles, input.ViewAngles, AimSensitivity * 90);
            }

            // Recoil
            if (doRecoil)
            {
                doRecoil = false;
                var recoilAngles = new Angles(IsZooming ? -Primary.Recoil * 0.4f : -Primary.Recoil, 0, 0);
                input.ViewAngles += recoilAngles;
            }
        }

        public override void CreateViewModel()
        {
            Host.AssertClient();

            if (string.IsNullOrEmpty(ViewModelPath))
                return;

            ViewModelEntity = new ViewModelBase(this);
            ViewModelEntity.Position = Position; // --> Does not seem to do anything
            ViewModelEntity.Owner = Owner;
            ViewModelEntity.EnableViewmodelRendering = true;
            ViewModelEntity.SetModel(ViewModelPath);

            if (DualWield)
            {
                dualWieldViewModel = new ViewModelBase(this, true);
                dualWieldViewModel.Owner = Owner;
                dualWieldViewModel.EnableViewmodelRendering = true;
                dualWieldViewModel.SetModel(ViewModelPath);
            }
        }

        public override void CreateHudElements()
        {
            if (Local.Hud == null) return;

            if (DrawCrosshair)
            {
                CrosshairPanel = new Crosshair();
                CrosshairPanel.Parent = Local.Hud;
            }

            if (DrawHitmarker)
            {
                Panel HitmarkerPanel = new Hitmarker();
                HitmarkerPanel.Parent = Local.Hud;
            }
        }

        public virtual float GetRealSpread(float baseSpread = -1)
        {
            float spread = baseSpread != -1 ? baseSpread : Primary.Spread;
            float floatMod = 1f;

            // Ducking
            if (Input.Down(InputButton.Duck) && !IsZooming)
                floatMod -= 0.25f;

            // Aiming
            if (IsZooming && this is not WeaponBaseShotty)
                floatMod /= 4;

            // Jumping
            if (Owner.GroundEntity == null)
                floatMod += 0.5f;

            return spread * floatMod;
        }

        public bool TakeAmmo(int amount)
        {
            if (Primary.InfiniteAmmo == InfiniteAmmoType.clip)
                return true;

            if (Primary.ClipSize == -1)
            {
                if (Owner is PlayerBase player)
                {
                    return player.TakeAmmo(Primary.AmmoType, amount) > 0;
                }
                return true;
            }

            if (Primary.Ammo < amount)
                return false;

            Primary.Ammo -= amount;
            return true;
        }

        public int AvailableAmmo()
        {
            var owner = Owner as PlayerBase;
            if (owner == null) return 0;

            // Show clipsize as the available ammo
            if (Primary.InfiniteAmmo == InfiniteAmmoType.reserve)
                return Primary.ClipSize;

            return owner.AmmoCount(Primary.AmmoType);
        }

        public bool IsUsable()
        {
            if (Primary.Ammo > 0) return true;
            return AvailableAmmo() > 0;
        }

        public override void OnCarryStart(Entity carrier)
        {
            base.OnCarryStart(carrier);

            if (PickupTrigger.IsValid())
            {
                PickupTrigger.EnableTouch = false;
            }
        }

        public override void OnCarryDrop(Entity dropper)
        {
            if (!DropWeaponOnDeath)
            {
                this.Delete();
                return;
            }

            base.OnCarryDrop(dropper);

            if (PickupTrigger.IsValid())
            {
                PickupTrigger.EnableTouch = true;
            }
        }

        public override void SimulateAnimator(PawnAnimator anim)
        {
            anim.SetParam("holdtype", (int)HoldType);
            anim.SetParam("aimat_weight", 1.0f);
        }

        [ClientRpc]
        public virtual void SendWeaponAnim(string anim, bool value = true)
        {
            ViewModelEntity?.SetAnimBool(anim, value);
        }
    }
}
