using Sandbox;
using SWB_Base;

namespace SWB_WEAPONS;

[Library("swb_bayonet", Title = "Bayonet")]
public class Bayonet : WeaponBaseMelee
{
    public override int Bucket => 0;
    public override HoldType HoldType => HoldType.Fists; // just use fists for now
    public override string HandsModelPath => "weapons/swb/hands/swat/v_hands_swat.vmdl";
    public override string ViewModelPath => "weapons/swb/melee/bayonet/v_bayonet.vmdl";
    public override AngPos ViewModelOffset => new()
    {
        Angle = new Angles(0, -15, 0),
        Pos = new Vector3(-3, 0, 0)
    };
    public override string WorldModelPath => "weapons/swb/melee/bayonet/w_bayonet.vmdl";
    public override string Icon => "code/swb_weapons/textures/bayonet.png";

    public override string PrimaryHitAnimation => "swing";
    public override string PrimaryMissAnimation => "swing_miss";
    public override string SecondaryHitAnimation => "stab";
    public override string SecondaryMissAnimation => "stab_miss";
    public override string PrimarySound => "bayonet.hit";
    public override string SecondarySound => "bayonet.stab";
    public override string MissSound => "bayonet.slash";
    public override string HitWorldSound => "bayonet.hitwall";
    public override float PrimarySpeed => 0.5f;
    public override float SecondarySpeed => 0.8f;
    public override float PrimaryDamage => 25f;
    public override float SecondaryDamage => 100f;
    public override float PrimaryForce => 25f;
    public override float SecondaryForce => 50f;
    public override float DamageDistance => 35f;
    public override float DamageSize => 10f;

    public Bayonet()
    {
        General = new WeaponInfo
        {
            FOV = 80,
            WalkAnimationSpeedMod = 1.35f,
        };

        UISettings = new UISettings
        {
            ShowCrosshairLines = false,
            ShowHitmarker = false,
            ShowAmmoCount = false,
            ShowFireMode = false
        };
    }
}
