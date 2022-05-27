using System;
using Sandbox;
using SWB_Base.UI;

namespace SWB_Base
{
    public partial class PlayerBase
    {
        public BulletSimulator BulletSimulator { get; private set; } = new();
        public DamageInfo LastDamage;

        private ScreenShakeStruct lastScreenShake;
        private TimeSince timeSinceShake;
        private float nextShake;

        public override void Simulate(Client client)
        {
            SimulateBase(client);
            BulletSimulator.Simulate();
        }

        public override void TakeDamage(DamageInfo info)
        {
            LastDamage = info;

            // Headshot double damage
            if (GetHitboxGroup(info.HitboxIndex) == 1)
            {
                info.Damage *= 2.0f;
            }

            TakeDamageBase(info);

            if (info.Attacker is PlayerBase attacker && attacker != this)
            {
                // Note - sending this only to the attacker!
                attacker.DidDamage(To.Single(attacker), info.Position, info.Damage, Health, ((float)Health).LerpInverse(100, 0));

                // Hitmarker
                var weapon = info.Weapon as WeaponBase;
                var uiSettings = weapon.UISettings;
                if (weapon != null && uiSettings.ShowHitmarker && !uiSettings.HideAll)
                    attacker.ShowHitmarker(To.Single(attacker), !Alive(), uiSettings.PlayHitmarkerSound);

                TookDamage(To.Single(this), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position);
            }
        }

        public override void PostCameraSetup(ref CameraSetup camSetup)
        {
            PostCameraSetupBase(ref camSetup);

            if (timeSinceShake < lastScreenShake.Length && timeSinceShake > nextShake)
            {
                var random = new Random();
                camSetup.Position += new Vector3(random.Float(0, lastScreenShake.Size), random.Float(0, lastScreenShake.Size), random.Float(0, lastScreenShake.Size));
                camSetup.Rotation *= Rotation.From(new Angles(random.Float(0, lastScreenShake.Rotation), random.Float(0, lastScreenShake.Rotation), 0));

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

        [ClientRpc]
        public virtual void DidDamage(Vector3 pos, float amount, float health, float healthinv)
        {
            Sound.FromScreen("dm.ui_attacker")
                .SetPitch(1 + healthinv * 1);
        }

        [ClientRpc]
        public virtual void TookDamage(Vector3 pos)
        {
            //DebugOverlay.Sphere( pos, 5.0f, Color.Red, false, 50.0f );

            DamageIndicator.Current?.OnHit(pos);
        }

        [ClientRpc]
        public virtual void ShowHitmarker(bool isKill, bool playSound)
        {
            Hitmarker.Current?.Create(isKill);

            if (playSound)
                PlaySound("swb_hitmarker");
        }
    }
}
