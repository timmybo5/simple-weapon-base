using System;
using Sandbox;
using SWB_Base;
using SWB_Base.UI;

namespace SWB_Player;

public partial class PlayerBase : ISWBPlayer
{
    public BulletSimulator BulletSimulator { get; private set; } = new();
    public DamageInfo LastDamage;

    private ScreenShakeStruct lastScreenShake;
    private RealTimeSince timeSinceShake;
    private float nextShake;

    private bool isLoweringFlinch;
    private float currFlinch;
    private float targetFlinch;
    private float flinchSpeed;

    private bool scopeOutToThirdPerson;

    public override void Simulate(IClient client)
    {
        SimulateBase(client);
        BulletSimulator.Simulate();

        if (CameraMode is ThirdPersonCamera)
        {
            EyeRotation = ThirdPersonCamera.GetEyeRotation(this);
        }
    }
    private void HandleFlinch()
    {
        if (!Alive())
        {
            targetFlinch = 0;
            isLoweringFlinch = false;
            return;
        }

        if (currFlinch == targetFlinch)
        {
            targetFlinch = 0;
            isLoweringFlinch = true;
        }
        else
        {
            currFlinch = currFlinch.Approach(targetFlinch, flinchSpeed);
        }

        if (currFlinch > 0)
        {
            var flinchAngles = new Angles(isLoweringFlinch ? currFlinch : -currFlinch, 0, 0);
            ViewAngles += flinchAngles;
        }
    }

    [ClientRpc]
    public virtual void DoHitFlinch(float amount)
    {
        isLoweringFlinch = false;
        flinchSpeed = amount / 4f;
        targetFlinch = amount;
    }

    public override void TakeDamage(DamageInfo info)
    {
        LastDamage = info;

        var weapon = info.Weapon as WeaponBase;

        // Hit flinch
        if (weapon != null && weapon.Primary.HitFlinch > 0)
            DoHitFlinch(To.Single(this), weapon.Primary.HitFlinch);

        // Headshot double damage
        if (info.Hitbox.HasTag("head"))
        {
            info.Damage *= 2.0f;
        }

        TakeDamageBase(info);

        if (info.Attacker is PlayerBase attacker && attacker != this)
        {
            // Note - sending this only to the attacker!
            attacker.DidDamage(To.Single(attacker), info.Position, info.Damage, Health, ((float)Health).LerpInverse(100, 0));

            // Hitmarker
            var uiSettings = weapon.UISettings;
            if (weapon != null && uiSettings.ShowHitmarker && !uiSettings.HideAll)
                attacker.ShowHitmarker(To.Single(attacker), !Alive(), uiSettings.PlayHitmarkerSound);
        }
    }

    public virtual void UpdateCamera()
    {
        if (timeSinceShake < lastScreenShake.Length && timeSinceShake > nextShake)
        {
            var random = new Random();
            var randomPos = new Vector3(random.Float(0, lastScreenShake.Size), random.Float(0, lastScreenShake.Size), random.Float(0, lastScreenShake.Size));
            var randomRot = Rotation.From(new Angles(random.Float(0, lastScreenShake.Rotation), random.Float(0, lastScreenShake.Rotation), 0));
            Camera.Position += randomPos;
            Camera.Rotation *= randomRot;

            // Make viewmodel 'follow' the shake
            if (ActiveChild is WeaponBase weapon && weapon.ViewModelEntity != null && weapon.ViewModelEntity.IsValid)
            {
                weapon.ViewModelEntity.Position += randomPos;
                weapon.ViewModelEntity.Rotation *= randomRot;
            }

            nextShake = timeSinceShake + lastScreenShake.Delay;
        }
    }

    public virtual void ScreenShake(ScreenShakeStruct screenShake)
    {
        lastScreenShake = screenShake;
        timeSinceShake = 0;
        nextShake = 0;
    }


    public virtual bool Alive()
    {
        return Health > 0;
    }

    public virtual void OnScopeStart()
    {
        if (!Game.IsServer) return;

        if (CameraMode is ThirdPersonCamera)
        {
            scopeOutToThirdPerson = true;
            CameraMode = new FirstPersonCamera();
        }
    }

    public virtual void OnScopeEnd()
    {
        if (!Game.IsServer) return;

        if (scopeOutToThirdPerson)
        {
            scopeOutToThirdPerson = false;
            CameraMode = new ThirdPersonCamera();
        }
    }

    [ClientRpc]
    public virtual void DidDamage(Vector3 pos, float amount, float health, float healthinv)
    {
        Sound.FromScreen("dm.ui_attacker")
            .SetPitch(1 + healthinv * 1);
    }

    [ClientRpc]
    public virtual void ShowHitmarker(bool isKill, bool playSound)
    {
        Hitmarker.Current?.Create(isKill);

        if (playSound)
            PlaySound("hitmarker");
    }
}
