using Sandbox;
using SWB_Base;

/*
 * This serves as a code example, css models are not included! 
*/

namespace SWB_CSS;

[Library("swb_css_knife", Title = "Knife")]
public class Knife : WeaponBaseMelee
{
    public override int Bucket => 0;
    public override HoldType HoldType => HoldType.Pistol;
    public override string ViewModelPath => "weapons/swb/css/knife/css_v_knife.vmdl";
    public override string WorldModelPath => "weapons/swb/css/knife/css_w_knife.vmdl";
    public override string Icon => "code/swb_css/textures/ui/css_icon_knife.png";

    public override string PrimaryHitAnimation => "swing";
    public override string PrimaryMissAnimation => "swing_miss";
    public override string SecondaryHitAnimation => "stab";
    public override string SecondaryMissAnimation => "stab_miss";
    public override string PrimarySound => "css_knife.hit";
    public override string SecondarySound => "css_knife.stab";
    public override string MissSound => "css_knife.slash";
    public override string HitWorldSound => "css_knife.hitwall";
    public override float PrimarySpeed => 0.6f;
    public override float SecondarySpeed => 1.29f;
    public override float PrimaryDamage => 25f;
    public override float SecondaryDamage => 100f;
    public override float PrimaryForce => 25f;
    public override float SecondaryForce => 50f;
    public override float DamageDistance => 35f;
    public override float DamageSize => 10f;

    public Knife()
    {
        General = new WeaponInfo
        {
            FOV = 75,
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
