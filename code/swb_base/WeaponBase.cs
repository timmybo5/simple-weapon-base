using System;
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
            InstanceID++;

            // Attachments
            HandleAttachments(true);

            // Draw animation
            if (IsLocalPawn)
            {
                if (Primary.Ammo == 0 && !string.IsNullOrEmpty(General.DrawEmptyAnim))
                {
                    ViewModelEntity?.SetAnimBool(General.DrawEmptyAnim, true);
                }
                else if (!string.IsNullOrEmpty(General.DrawAnim))
                {
                    ViewModelEntity?.SetAnimBool(General.DrawAnim, true);
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

            // Check if boltback was not completed
            if (IsServer && InBoltBack)
            {
                if (IsServer)
                    _ = AsyncBoltBack(General.DrawTime, General.BoltBackAnim, General.BoltBackTime, General.BoltBackEjectDelay, Primary.BulletEjectParticle, true);
            }
        }

        public override void ActiveEnd(Entity ent, bool dropped)
        {
            // Attachments
            HandleAttachments(false);

            base.ActiveEnd(ent, dropped);
        }

        // BaseSimulate
        public void BaseSimulate(Client player)
        {
            // DEBUG
            //foreach (var att in ActiveAttachments)
            //{
            //    LogUtil.Info("[" + ActiveAttachments.Count + "] name=" + att.ToString());
            //}

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

            IsRunning = Input.Down(InputButton.Run) && RunAnimData != AngPos.Zero && Owner.Velocity.Length >= 200;

            if (Secondary == null && ZoomAnimData != AngPos.Zero && !(this is WeaponBaseMelee))
                IsZooming = Input.Down(InputButton.Attack2) && !IsRunning && !IsReloading;

            if (TimeSinceDeployed < General.DrawTime)
                return;

            // Burst fire
            ResetBurstFireCount(Primary, InputButton.Attack1);
            ResetBurstFireCount(Secondary, InputButton.Attack2);

            if (!IsReloading || this is WeaponBaseShotty)
            {
                BaseSimulate(owner);
            }

            if (IsReloading && TimeSinceReload >= 0)
            {
                OnReloadFinish();
            }
        }

        public virtual void ResetBurstFireCount(ClipInfo clipInfo, InputButton inputButton)
        {
            if (clipInfo == null || clipInfo.FiringType != FiringType.burst) return;

            if (Input.Released(inputButton))
            {
                burstCount = 0;
            }
        }

        public virtual void Reload()
        {
            if (IsReloading || IsAnimating || InBoltBack || IsShooting())
                return;

            var maxClipSize = BulletCocking ? Primary.ClipSize + 1 : Primary.ClipSize;

            if (Primary.Ammo >= maxClipSize || Primary.ClipSize == -1)
                return;

            var isEmptyReload = General.ReloadEmptyTime > 0 ? Primary.Ammo == 0 : false;
            TimeSinceReload = -(isEmptyReload ? General.ReloadEmptyTime : General.ReloadTime);

            if (!isEmptyReload && Primary.Ammo == 0 && General.BoltBackTime > -1)
            {
                TimeSinceReload -= General.BoltBackTime;

                if (IsServer)
                    _ = AsyncBoltBack(General.ReloadTime, General.BoltBackAnim, General.BoltBackTime, General.BoltBackEjectDelay, Primary.BulletEjectParticle);
            }

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
        public virtual void StartReloadEffects(bool isEmpty, string reloadAnim = null)
        {
            if (reloadAnim != null)
            {
                ViewModelEntity?.SetAnimBool(reloadAnim, true);
            }
            else if (isEmpty && General.ReloadEmptyAnim != null)
            {
                ViewModelEntity?.SetAnimBool(General.ReloadEmptyAnim, true);
            }
            else if (General.ReloadAnim != null)
            {
                ViewModelEntity?.SetAnimBool(General.ReloadAnim, true);
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
                //var random = new Random(); -> might making aiming too difficult
                //var randVal = random.Next(-5, 5) / 10f;
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
            ViewModelEntity.Owner = Owner;
            ViewModelEntity.EnableViewmodelRendering = true;
            ViewModelEntity.SetModel(ViewModelPath);

            // Bonemerge hands
            if (!string.IsNullOrEmpty(HandsModelPath))
            {
                HandsModel = new BaseViewModel();
                HandsModel.Owner = Owner;
                HandsModel.EnableViewmodelRendering = true;
                HandsModel.SetModel(HandsModelPath);
                HandsModel.SetParent(ViewModelEntity, true);
            }
        }

        public virtual ModelEntity GetEffectModel()
        {
            ModelEntity effectModel = ViewModelEntity;

            // We don't want to change the world effect origin if we or others can see it
            if ((IsLocalPawn && !Owner.IsFirstPersonMode) || !IsLocalPawn)
            {
                effectModel = EffectEntity;
            }

            return effectModel;
        }

        // Pass the active child from before the delay
        protected bool IsAsyncValid(Entity activeChild, int instanceID)
        {
            return Owner != null && activeChild == Owner.ActiveChild && instanceID == InstanceID;
        }

        protected bool IsShooting()
        {
            if (Secondary == null)
                return GetRealRPM(Primary.RPM) > TimeSincePrimaryAttack;

            return GetRealRPM(Primary.RPM) > TimeSincePrimaryAttack || GetRealRPM(Secondary.RPM) > TimeSinceSecondaryAttack;
        }

        protected bool CanSeeViewModel()
        {
            return IsLocalPawn && Owner.IsFirstPersonMode;
        }

        protected float GetRealRPM(int rpm)
        {
            return (60f / rpm);
        }

        public virtual float GetRealSpread(float baseSpread = -1)
        {
            if (!Owner.IsValid()) return 0;

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
