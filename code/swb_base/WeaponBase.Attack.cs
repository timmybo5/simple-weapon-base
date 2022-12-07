using System.Collections.Generic;
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
    public virtual bool CanAttack(ClipInfo clipInfo, TimeSince lastAttackTime, InputButton inputButton)
    {
        if (IsAnimating || InBoltBack) return false;
        if (clipInfo == null || !Owner.IsValid() || !Input.Down(inputButton)) return false;

        if (!HasAmmo())
        {
            if (Input.Pressed(inputButton))
            {
                if (IsClient)
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
        return CanAttack(Primary, TimeSincePrimaryAttack, InputButton.PrimaryAttack);
    }

    /// <summary>
    /// Checks if weapon can do the secondary attack
    /// </summary>
    public virtual bool CanSecondaryAttack()
    {
        return CanAttack(Secondary, TimeSinceSecondaryAttack, InputButton.SecondaryAttack);
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
        var bulletEjectParticle = General.BoltBackTime > -1 ? "" : clipInfo.BulletEjectParticle;

        if (clipInfo.Ammo > 0 && General.BoltBackTime > -1)
        {
            if (IsServer)
                _ = AsyncBoltBack(GetRealRPM(clipInfo.RPM), General.BoltBackAnim, General.BoltBackTime, General.BoltBackEjectDelay, clipInfo.BulletEjectParticle, true);
        }

        // Shotgun
        if (this is WeaponBaseShotty shotty)
        {
            if (IsServer)
                _ = shotty.EjectShell(bulletEjectParticle);

            bulletEjectParticle = "";
        }

        // Player anim
        (Owner as AnimatedEntity).SetAnimParameter("b_attack", true);

        // Shoot effects
        if (IsLocalPawn)
            ScreenUtil.Shake(clipInfo.ScreenShake);

        ShootEffects(clipInfo.MuzzleFlashParticle, bulletEjectParticle, GetShootAnimation(clipInfo));

        // Barrel smoke
        if (IsServer && BarrelSmoking)
        {
            AddBarrelHeat();
            if (barrelHeat >= clipInfo.ClipSize * 0.75)
            {
                ShootEffects(clipInfo.BarrelSmokeParticle, null, null);
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
            realSpread = IsZooming ? clipInfo.Spread / 4 : clipInfo.Spread;
        }

        if (IsServer)
        {
            for (int i = 0; i < clipInfo.Bullets; i++)
            {
                ShootBullet(realSpread, clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize, isPrimary);
            }
        }

        // Recoil
        doRecoil = true;
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

        if (Owner is not PlayerBase owner) return;
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

        ShootEffects(clipInfo.MuzzleFlashParticle, clipInfo.BulletEjectParticle, null);

        if (clipInfo.ShootSound != null)
            PlaySound(clipInfo.ShootSound);

        // Shoot the bullets
        if (IsServer)
        {
            for (int i = 0; i < clipInfo.Bullets; i++)
            {
                ShootBullet(GetRealSpread(clipInfo.Spread), clipInfo.Force, clipInfo.Damage, clipInfo.BulletSize, isPrimary);
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
        List<string> withoutTags = new() { "trigger" };

        if (startsInWater)
            withoutTags.Add("water");

        var tr = Trace.Ray(start, end)
                .UseHitboxes()
                .WithoutTags(withoutTags.ToArray())
                .Ignore(Owner)
                .Ignore(this)
                .Size(radius)
                .Run();

        return tr;
    }

    /// <summary>
    /// Shoot a single bullet (server only)
    /// </summary>
    public virtual void ShootBullet(float spread, float force, float damage, float bulletSize, bool isPrimary)
    {
        var player = Owner as PlayerBase;

        // Spread
        var forward = player.EyeRotation.Forward;
        forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
        forward = forward.Normal;
        var endPos = player.EyePosition + forward * 999999;

        // Server Bullet
        if (isPrimary)
        {
            Primary.BulletType.FireSV(this, player.EyePosition, endPos, forward, spread, force, damage, bulletSize, isPrimary);
        }
        else
        {
            Secondary.BulletType.FireSV(this, player.EyePosition, endPos, forward, spread, force, damage, bulletSize, isPrimary);
        }

        // Client bullet
        ShootClientBullet(player.EyePosition, endPos, forward, spread, force, damage, bulletSize, isPrimary);
    }

    /// <summary>
    /// Shoot a single bullet (client only)
    /// </summary>
    [ClientRpc]
    public virtual void ShootClientBullet(Vector3 startPos, Vector3 endPos, Vector3 forward, float spread, float force, float damage, float bulletSize, bool isPrimary)
    {
        if (Owner is not PlayerBase player) return;

        if (isPrimary)
        {
            Primary.BulletType.FireCL(this, player.EyePosition, endPos, forward, spread, force, damage, bulletSize, isPrimary);
        }
        else
        {
            Secondary.BulletType.FireCL(this, player.EyePosition, endPos, forward, spread, force, damage, bulletSize, isPrimary);
        }
    }

    /// <summary>
    /// Plays the bolt back animation
    /// </summary>
    async Task AsyncBoltBack(float boltBackDelay, string boltBackAnim, float boltBackTime, float boltBackEjectDelay, string bulletEjectParticle, bool force = false)
    {
        var player = Owner as PlayerBase;
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
        ShootEffects(null, bulletEjectParticle, null);

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

            if (CanSeeViewModel())
            {
                effectEntity = activeAttachment.ViewAttachmentModel;
            }
            else
            {
                effectEntity = activeAttachment.WorldAttachmentModel;
            }
        }

        return (effectEntity, particleAttachment);
    }

    /// <summary>
    /// Shows shooting effects
    /// </summary>
    [ClientRpc]
    protected virtual void ShootEffects(string muzzleFlashParticle, string bulletEjectParticle, string shootAnim)
    {
        Host.AssertClient();

        ModelEntity firingViewModel = GetEffectModel();

        if (firingViewModel == null) return;

        if (!string.IsNullOrEmpty(muzzleFlashParticle))
        {
            var effectData = GetMuzzleEffectData(firingViewModel);
            Particles.Create(muzzleFlashParticle, effectData.Item1, effectData.Item2);
        }

        if (!string.IsNullOrEmpty(bulletEjectParticle))
            Particles.Create(bulletEjectParticle, firingViewModel, "ejection_point");

        if (!string.IsNullOrEmpty(shootAnim))
        {
            ViewModelEntity?.SetAnimParameter(shootAnim, true);
            crosshair?.CreateEvent("fire", (60f / Primary.RPM));
        }
    }
}
