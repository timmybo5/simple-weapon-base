
namespace SWB_Base.Attachments;

public class Silencer : OffsetAttachment
{
    public override string Name => "Silencer";
    public override string Description => "Reduces the acoustic intensity of the muzzle report and the recoil when a gun is discharged by modulating the speed and pressure of the propellant gas from the muzzle.";
    public override string[] Positives => new string[]
    {
        "Reduce sound",
        "Reduce muzzle flash",
    };

    public override string[] Negatives => new string[]
    {
    };

    public override StatModifier StatModifier { get; set; } = new StatModifier
    {
        Spread = -0.05f,
        BulletVelocity = -0.05f,
    };

    public override string EffectAttachment => "muzzle2"; // New muzzle flash effect point

    /// <summary>New particle used for the muzzle flash</summary>
    public string MuzzleFlashParticle { get; set; }

    /// <summary>New sound used for firing</summary>
    public string ShootSound { get; set; }

    private string oldMuzzleFlashParticle;
    private string oldShootSound;

    public override void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel)
    {
        oldMuzzleFlashParticle = weapon.Primary.MuzzleFlashParticle;
        oldShootSound = weapon.Primary.ShootSound;

        weapon.Primary.MuzzleFlashParticle = MuzzleFlashParticle;
        weapon.Primary.ShootSound = ShootSound;
    }

    public override void OnUnequip(WeaponBase weapon)
    {
        weapon.Primary.MuzzleFlashParticle = oldMuzzleFlashParticle;
        weapon.Primary.ShootSound = oldShootSound;
    }
}

public class PistolSilencer : Silencer
{
    public override string Name => "SR8 Silencer";
    public override string IconPath => "attachments/swb/muzzle/silencer_pistol/ui/icon.png";
    public override string ModelPath => "attachments/swb/muzzle/silencer_pistol/silencer_pistol.vmdl";
}

public class RifleSilencer : Silencer
{
    public override string Name => "ATS4 Silencer";
    public override string IconPath => "attachments/swb/muzzle/silencer_rifle/ui/icon.png";
    public override string ModelPath => "attachments/swb/muzzle/silencer_rifle/silencer_rifle.vmdl";
}

public class SniperSilencer : Silencer
{
    public override string Name => "ATS5 Silencer";
    public override string IconPath => "attachments/swb/muzzle/silencer_sniper/ui/icon.png";
    public override string ModelPath => "attachments/swb/muzzle/silencer_sniper/silencer_sniper.vmdl";
}

public class ShotgunSilencer : Silencer
{
    public override string Name => "Salvo 12G Silencer";
    public override string IconPath => "attachments/swb/muzzle/silencer_shotgun/ui/icon.png";
    public override string ModelPath => "attachments/swb/muzzle/silencer_shotgun/silencer_shotgun.vmdl";
}
