using Sandbox;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base;

public partial class WeaponBase : CarriableBase
{
    public override void Spawn()
    {
        base.Spawn();

        Tags.Add("weapon");

        SetModel(WorldModelPath);
    }

    public override void ActiveStart(Entity ent)
    {
        base.ActiveStart(ent);

        TimeSinceDeployed = 0;
        IsReloading = false;
        InstanceID++;

        // Draw animation
        if (IsLocalPawn)
        {
            if (Primary.Ammo == 0 && !string.IsNullOrEmpty(General.DrawEmptyAnim))
            {
                ViewModelEntity?.SetAnimParameter(General.DrawEmptyAnim, true);
            }
            else if (!string.IsNullOrEmpty(General.DrawAnim))
            {
                ViewModelEntity?.SetAnimParameter(General.DrawAnim, true);
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
        if (Game.IsServer && InBoltBack)
        {
            _ = AsyncBoltBack(General.DrawTime, General.BoltBackAnim, General.BoltBackTime, General.BoltBackEjectDelay, Primary.BulletEjectParticle, true);
        }

        // Save initial values
        if (InitialStats == null)
        {
            InitialStats = new StatModifier
            {
                Damage = Primary.Damage,
                Recoil = Primary.Recoil,
                Spread = Primary.Spread,
                RPM = Primary.RPM,
            };
        }

        // Attachments
        HandleAttachments(true);
    }

    public override void ActiveEnd(Entity ent, bool dropped)
    {
        // Attachments
        HandleAttachments(false);

        base.ActiveEnd(ent, dropped);
    }

    // BaseSimulate
    public void BaseSimulate(IClient player)
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
            using (LagCompensation())
            {
                TimeSincePrimaryAttack = 0;
                AttackPrimary();
            }
        }

        // AttackPrimary could have deleted us
        if (!player.IsValid())
            return;

        if (CanSecondaryAttack())
        {
            using (LagCompensation())
            {
                TimeSinceSecondaryAttack = 0;
                AttackSecondary();
            }
        }
    }

    public override void Simulate(IClient client)
    {
        if (IsAnimating) return;

        // Handle custom animation actions
        if (AnimatedActions != null && !IsReloading)
        {
            for (int i = 0; i < AnimatedActions.Count; i++)
            {
                if (AnimatedActions[i].Handle(client, this))
                    return;
            }
        }

        IsRunning = Input.Down(InputButton.Run) && RunAnimData != AngPos.Zero && Owner.Velocity.Length >= 200;

        if (Secondary == null && ZoomAnimData != AngPos.Zero && this is not WeaponBaseMelee)
            IsZooming = Input.Down(InputButton.SecondaryAttack) && !IsRunning && !IsReloading;

        if (TimeSinceDeployed < General.DrawTime)
            return;

        // Burst fire
        ResetBurstFireCount(Primary, InputButton.PrimaryAttack);
        ResetBurstFireCount(Secondary, InputButton.SecondaryAttack);

        if (!IsReloading || this is WeaponBaseShotty)
        {
            BaseSimulate(client);
        }

        if (IsReloading && TimeSinceReload >= 0)
        {
            OnReloadFinish();
        }

        if (Game.IsClient)
        {
            UISimulate(client);
        }
    }

    public virtual void Reload()
    {
        if (IsReloading || IsAnimating || InBoltBack || IsShooting())
            return;

        var maxClipSize = BulletCocking ? Primary.ClipSize + 1 : Primary.ClipSize;

        if (Primary.Ammo >= maxClipSize || Primary.ClipSize == -1)
            return;

        var isEmptyReload = General.ReloadEmptyTime > 0 && Primary.Ammo == 0;
        TimeSinceReload = -(isEmptyReload ? General.ReloadEmptyTime : General.ReloadTime);

        if (!isEmptyReload && Primary.Ammo == 0 && General.BoltBackTime > -1)
        {
            TimeSinceReload -= General.BoltBackTime;

            if (Game.IsServer)
                _ = AsyncBoltBack(General.ReloadTime, General.BoltBackAnim, General.BoltBackTime, General.BoltBackEjectDelay, Primary.BulletEjectParticle);
        }

        if (Owner is PlayerBase player)
        {
            if (player.AmmoCount(Primary.AmmoType) <= 0 && Primary.InfiniteAmmo != InfiniteAmmoType.reserve)
                return;
        }

        IsReloading = true;

        // Player anim
        (Owner as AnimatedEntity).SetAnimParameter("b_reload", true);

        StartReloadEffects(isEmptyReload);
    }

    public virtual void OnReloadFinish()
    {
        IsReloading = false;
        var maxClipSize = BulletCocking && Primary.Ammo > 0 ? Primary.ClipSize + 1 : Primary.ClipSize;

        if (Primary.InfiniteAmmo == InfiniteAmmoType.reserve)
        {
            Primary.Ammo = maxClipSize;
            return;
        }

        if (Owner is PlayerBase player)
        {
            var ammo = player.TakeAmmo(Primary.AmmoType, maxClipSize - Primary.Ammo);

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
            ViewModelEntity?.SetAnimParameter(reloadAnim, true);
        }
        else if (isEmpty && General.ReloadEmptyAnim != null)
        {
            ViewModelEntity?.SetAnimParameter(General.ReloadEmptyAnim, true);
        }
        else if (General.ReloadAnim != null)
        {
            ViewModelEntity?.SetAnimParameter(General.ReloadAnim, true);
        }

        // TODO - player third person model reload
    }

    public override void BuildInput()
    {
        if (Owner is not PlayerBase player) return;

        var enableZoomSens = ConsoleSystem.GetValue("swb_cl_enable_zoomsens");

        // Mouse sensitivity
        if (IsZooming && enableZoomSens == "1")
        {
            player.ViewAngles = Angles.Lerp(player.OriginalViewAngles, player.ViewAngles, General.AimSensitivity);
        }

        // Recoil
        if (doRecoil)
        {
            doRecoil = false;
            //var random = new Random(); -> might making aiming too difficult
            //var randVal = random.Next(-5, 5) / 10f;
            var recoilAngles = new Angles(IsZooming ? -Primary.Recoil * 0.4f : -Primary.Recoil, 0, 0);
            player.ViewAngles += recoilAngles;
        }
    }

    public virtual void UpdateCamera() { }

    public virtual void UpdateViewmodelCamera()
    {
        if (ViewModelEntity is ViewModelBase viewModel)
        {
            viewModel.UpdateCamera();
        }
    }

    public override void CreateViewModel()
    {
        Game.AssertClient();

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

    /// <summary>
    /// Tries to take ammo from the weapon, returns if the attempt was successful
    /// </summary>
    public bool TakeAmmo(int amount = 1)
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

    public override void OnCarryStart(Entity carrier)
    {
        TimeSinceActiveStart = 0;

        base.OnCarryStart(carrier);
    }

    public override void OnCarryDrop(Entity dropper)
    {
        if (!CanDrop)
        {
            this.Delete();
            return;
        }

        base.OnCarryDrop(dropper);

        if (Game.IsServer)
        {
            // Reattach attachments (they get removed in ActiveEnd) [TEMP]
            foreach (var activeAttach in ActiveAttachments)
            {
                var attach = GetAttachment(activeAttach.Name);
                attach.CreateModel(this);
            }
        }
    }

    public override void SimulateAnimator(PlayerBaseAnimator anim)
    {
        anim.SetAnimParameter("holdtype", (int)HoldType);
        anim.SetAnimParameter("aim_body_weight", 1.0f);
    }

    /// <summary>
    /// Plays a sound on the client
    /// </summary>
    [ClientRpc]
    public virtual void SendWeaponSound(string sound)
    {
        if (!string.IsNullOrEmpty(sound))
            PlaySound(sound);
    }

    /// <summary>
    /// Plays a weapon animation on the client
    /// </summary>
    [ClientRpc]
    public virtual void SendWeaponAnim(string anim, bool value = true)
    {
        if (!string.IsNullOrEmpty(anim))
            ViewModelEntity?.SetAnimParameter(anim, value);
    }
}
