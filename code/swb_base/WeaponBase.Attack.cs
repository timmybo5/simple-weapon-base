using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace SWB_Base;

public partial class WeaponBase
{
    /// <summary>
    /// Checks if the weapon can do the provided attack
    /// </summary>
    /// <param name="clipInfo">Attack information</param>
    /// <param name="lastAttackTime">Time since this attack</param>
    /// <param name="inputButton">The input button for this attack</param>
    /// <returns></returns>
    public virtual bool CanAttack(ClipInfo clipInfo, TimeSince lastAttackTime, string inputButton)
    {
        if (IsAnimating || InBoltBack) return false;
        if (clipInfo == null || !Owner.IsValid() || !Input.Down(inputButton)) return false;

        if (!HasAmmo())
        {
            if (Input.Pressed(inputButton))
            {
                if (Game.IsClient)
                    PlaySound(clipInfo.DryFireSound);

                // Check for auto reloading
                if (AutoReloadSV > 0)
                {
                    TimeSincePrimaryAttack = 999;
                    TimeSinceSecondaryAttack = 999;
                    timeSinceFired = 999;
                    Reload();
                }
            }

            return false;
        }

        if (clipInfo.FiringType == FiringType.semi && !Input.Pressed(inputButton)) return false;
        if (clipInfo.FiringType == FiringType.burst)
        {
            if (burstCount > 2) return false;

            if (Input.Down(inputButton) && lastAttackTime > GetRealRPM(clipInfo.RPM))
            {
                burstCount++;
                return true;
            }

            return false;
        };

        if (clipInfo.RPM <= 0) return true;

        return lastAttackTime > GetRealRPM(clipInfo.RPM);
    }

    /// <summary>
    /// Checks if weapon can do the primary attack
    /// </summary>
    public virtual bool CanPrimaryAttack()
    {
        return CanAttack(Primary, TimeSincePrimaryAttack, InputButtonHelper.PrimaryAttack);
    }

    /// <summary>
    /// Checks if weapon can do the secondary attack
    /// </summary>
    public virtual bool CanSecondaryAttack()
    {
        return CanAttack(Secondary, TimeSinceSecondaryAttack, InputButtonHelper.SecondaryAttack);
    }

    /// <summary>
    /// Shoot the weapon
    /// </summary>
    /// <param name="clipInfo">Attack information</param>
    /// <param name="isPrimary">If this is the primary attack</param>
    public virtual void Attack(ClipInfo clipInfo, bool isPrimary)
    {
        if (IsRunning || ShouldTuck()) return;

        TimeSincePrimaryAttack = 0;
        TimeSinceSecondaryAttack = 0;
        timeSinceFired = 0;

        // Take ammo
        TakeAmmo();

        // Boltback
        var bulletEjectParticle = General.BoltBackTime > -1 ? null : clipInfo.BulletEjectParticle;

        if (Game.IsServer && clipInfo.Ammo > 0 && General.BoltBackTime > -1)
        {
            _ = AsyncBoltBack(GetRealRPM(clipInfo.RPM), General.BoltBackAnim, General.BoltBackTime, General.BoltBackEjectDelay, clipInfo.BulletEjectParticle, true);
        }

        // Shotgun
        if (this is WeaponBaseShotty shotty)
        {
            if (Game.IsServer && !string.IsNullOrEmpty(bulletEjectParticle?.Path))
                _ = shotty.EjectShell(bulletEjectParticle);

            bulletEjectParticle = null;
        }

        // Player anim
        (Owner as AnimatedEntity).SetAnimParameter("b_attack", true);

        // Shoot effects
        if (IsLocalPawn)
            ScreenUtil.Shake(clipInfo.ScreenShake);

        ShootEffects(clipInfo.MuzzleFlashParticle?.Serialize(), bulletEjectParticle?.Serialize(), GetShootAnimation(clipInfo));

        // Barrel smoke
        if (Game.IsServer && BarrelSmoking && !string.IsNullOrEmpty(clipInfo.BarrelSmokeParticle?.Path))
        {
            AddBarrelHeat();
            if (barrelHeat >= clipInfo.ClipSize * 0.75)
            {
                DoMuzzleEffect(clipInfo.BarrelSmokeParticle.Path, clipInfo.BarrelSmokeParticle.VMScale, clipInfo.BarrelSmokeParticle.WMScale);
            }
        }

        if (clipInfo.ShootSound != null)
            PlaySound(clipInfo.ShootSound);

        // Shoot the bullets
        float realSpread;

        if (this is WeaponBaseShotty)
        {
            realSpread = clipInfo.Spread;
        }
        else
        {
            realSpread = IsZooming ? clipInfo.Spread * General.ZoomSpreadMod : clipInfo.Spread;
        }

        for (int i = 0; i < clipInfo.Bullets; i++)
        {
            Game.SetRandomSeed(Time.Tick + i);
            ShootBullet(realSpread, clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize, clipInfo.BulletTracerChance, isPrimary);
        }

        if (Owner is ISWBPlayer player)
        {
            player.OnRecoil();
        }
    }

    /// <summary>
    /// Shoot the weapon with a delay
    /// </summary>
    /// <param name="clipInfo">Attack information</param>
    /// <param name="isPrimary">If this is the primary attack</param>
    /// <param name="delay">Bullet firing delay</param>
    public virtual async Task AsyncAttack(ClipInfo clipInfo, bool isPrimary, float delay)
    {
        TimeSincePrimaryAttack -= delay;
        TimeSinceSecondaryAttack -= delay;

        // Player anim
        (Owner as AnimatedEntity).SetAnimParameter("b_attack", true);

        // Play pre-fire animation

        ShootEffects(null, null, GetShootAnimation(clipInfo));

        if (Owner is not ISWBPlayer owner) return;
        var activeWeapon = owner.ActiveChild;
        var instanceID = InstanceID;

        await GameTask.DelaySeconds(delay);

        // Check if owner and weapon are still valid
        if (!IsAsyncValid(activeWeapon, instanceID)) return;

        // Take ammo
        TakeAmmo();

        // Shoot effects
        if (IsLocalPawn)
            ScreenUtil.Shake(clipInfo.ScreenShake);


        ShootEffects(clipInfo.MuzzleFlashParticle?.Serialize(), clipInfo.BulletEjectParticle?.Serialize(), null);

        if (clipInfo.ShootSound != null)
            PlaySound(clipInfo.ShootSound);

        // Shoot the bullets
        if (Game.IsServer)
        {
            for (int i = 0; i < clipInfo.Bullets; i++)
            {
                Game.SetRandomSeed(Time.Tick + i);
                ShootBullet(GetRealSpread(clipInfo.Spread), clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize, clipInfo.BulletTracerChance, isPrimary);
            }
        }
    }

    public virtual void DelayedAttack(ClipInfo clipInfo, bool isPrimary, float delay)
    {
        _ = AsyncAttack(Primary, isPrimary, PrimaryDelay);
    }

    /// <summary>
    /// Do the primary attack
    /// </summary>
    public virtual void AttackPrimary()
    {
        if (PrimaryDelay > 0)
        {
            DelayedAttack(Primary, true, PrimaryDelay);
        }
        else
        {
            Attack(Primary, true);
        }
    }

    /// <summary>
    /// Do the secondary attack
    /// </summary>
    public virtual void AttackSecondary()
    {
        if (Secondary != null)
        {
            if (SecondaryDelay > 0)
            {
                DelayedAttack(Secondary, false, SecondaryDelay);
            }
            else
            {
                Attack(Secondary, false);
            }
            return;
        }
    }

    /// <summary>
    /// A single bullet trace from start to end with a certain radius.
    /// </summary>
    public virtual TraceResult TraceBullet(Vector3 start, Vector3 end, float radius = 2.0f)
    {
        var startsInWater = SurfaceUtil.IsPointWater(start);
        List<string> withoutTags = new() { "trigger", "playerclip", "passbullets" };

        if (startsInWater)
            withoutTags.Add("water");

        var tr = Trace.Ray(start, end)
                .UseHitboxes()
                .WithoutTags(withoutTags.ToArray())
                .Ignore(Owner)
                .Ignore(this)
                .Size(radius)
                .Run();

        //Log.Info("entity: " + tr.Entity.Name);
        //Log.Info("> tags: " + string.Join(", ", tr.Tags));

        return tr;
    }

    /// <summary>
    /// Shoot a single bullet (shared)
    /// </summary>
    public virtual void ShootBullet(float spread, float force, float damage, float bulletSize, float bulletTracerChance, bool isPrimary)
    {
        var player = Owner as ISWBPlayer;

        // Spread
        var forward = player.EyeRotation.Forward;
        forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
        forward = forward.Normal;
        var endPos = player.EyePosition + forward * 999999;

        // Bullet
        var clipInfo = isPrimary ? Primary : Secondary;
        clipInfo.BulletType.Fire(this, player.EyePosition, endPos, forward, spread, force, damage, bulletSize, bulletTracerChance, isPrimary);

        // Other players
        if (Game.IsServer)
        {
            var targets = Entity.All.OfType<IClient>().Where(ply => ply != Owner.Client);
            ShootClientBullet(To.Multiple(targets), player.EyePosition, endPos, forward, spread, force, damage, bulletSize, bulletTracerChance, isPrimary);
        }
    }

    [ClientRpc]
    public virtual void ShootClientBullet(Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, float bulletTracerChance, bool isPrimary)
    {
        if (Owner is not ISWBPlayer player || !this.IsValid) return;

        var clipInfo = isPrimary ? Primary : Secondary;
        clipInfo.BulletType.Fire(this, player.EyePosition, endPos, forward, spread, force, damage, bulletSize, bulletTracerChance, isPrimary);
    }

    /// <summary>
    /// Plays the bolt back animation
    /// </summary>
    async Task AsyncBoltBack(float boltBackDelay, string boltBackAnim, float boltBackTime, float boltBackEjectDelay, ParticleData bulletEjectParticle, bool force = false)
    {
        Game.AssertServer();

        var player = Owner as ISWBPlayer;
        var activeWeapon = player.ActiveChild;
        var instanceID = InstanceID;
        InBoltBack = force;

        // Start boltback
        await GameTask.DelaySeconds(boltBackDelay);
        if (!IsAsyncValid(activeWeapon, instanceID)) return;
        SendWeaponAnim(boltBackAnim);

        // Eject shell
        await GameTask.DelaySeconds(boltBackEjectDelay);
        if (!IsAsyncValid(activeWeapon, instanceID)) return;
        DoBullectEjectEffect(bulletEjectParticle.Path, bulletEjectParticle.VMScale, bulletEjectParticle.WMScale);

        // Finished
        await GameTask.DelaySeconds(boltBackTime - boltBackEjectDelay);
        if (!IsAsyncValid(activeWeapon, instanceID)) return;
        InBoltBack = false;
    }

    /// <summary>
    /// Gets the data on where to show the muzzle effect
    /// </summary>
    public virtual (ModelEntity, string) GetMuzzleEffectData(ModelEntity effectEntity)
    {
        var activeAttachment = GetActiveAttachmentFromCategory(AttachmentCategoryName.Muzzle);
        var particleAttachment = "muzzle";

        if (activeAttachment != null)
        {
            var attachment = GetAttachment(activeAttachment.Name);
            particleAttachment = attachment.EffectAttachment;

            if (attachment is OffsetAttachment)
            {
                if (CanSeeViewModel())
                {
                    effectEntity = activeAttachment.ViewAttachmentModel;
                }
                else
                {
                    effectEntity = activeAttachment.WorldAttachmentModel;
                }
            }
        }

        return (effectEntity, particleAttachment);
    }

    /// <summary>
    /// Networks shooting effects
    /// </summary>
    protected virtual void ShootEffects(byte[] muzzleFlashData, byte[] bulletEjectData, string shootAnim)
    {
        var muzzleFlashParticle = ParticleData.Deserialize(muzzleFlashData);
        var bulletEjectParticle = ParticleData.Deserialize(bulletEjectData);

        if (!string.IsNullOrEmpty(muzzleFlashParticle?.Path))
            DoMuzzleEffect(muzzleFlashParticle.Path, muzzleFlashParticle.VMScale, muzzleFlashParticle.WMScale);

        if (!string.IsNullOrEmpty(bulletEjectParticle?.Path))
            DoBullectEjectEffect(bulletEjectParticle.Path, bulletEjectParticle.VMScale, bulletEjectParticle.WMScale);

        if (!string.IsNullOrEmpty(shootAnim))
            DoShootAnimation(shootAnim);
    }

    /// <summary>
    /// Handles shooting effects
    /// </summary>
    [ClientRpc]
    protected virtual void DoShootAnimation(string shootAnim)
    {
        Game.AssertClient();
        ViewModelEntity?.SetAnimParameter(shootAnim, true);
        crosshair?.CreateEvent("fire", GetRealRPM(Primary.RPM));
    }

    [ClientRpc]
    protected virtual void DoMuzzleEffect(string effect, float vmScale = 1f, float wmScale = 1f)
    {
        ModelEntity firingViewModel = GetEffectModel();
        if (firingViewModel == null) return;

        var isViewModel = IsLocalPawn && IsFirstPersonMode;
        var effectData = GetMuzzleEffectData(firingViewModel);
        var particle = Particles.Create(effect, effectData.Item1, effectData.Item2);
        var scale = isViewModel ? vmScale : wmScale;

        particle.Set("scale", scale);
    }

    [ClientRpc]
    protected virtual void DoBullectEjectEffect(string effect, float vmScale = 1f, float wmScale = 1f)
    {
        DoEffect(effect, "ejection_point", vmScale, wmScale);
    }


    [ClientRpc]
    protected virtual void DoEffect(string effect, string attachment, float vmScale = 1f, float wmScale = 1f)
    {
        ModelEntity firingViewModel = GetEffectModel();
        if (firingViewModel == null) return;

        var isViewModel = IsLocalPawn && IsFirstPersonMode;
        var scale = isViewModel ? vmScale : wmScale;
        var particle = Particles.Create(effect, firingViewModel, attachment);

        particle.Set("scale", scale);
    }
}
