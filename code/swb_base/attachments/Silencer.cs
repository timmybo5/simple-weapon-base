using System;
using Sandbox;

// Sounds Effects: https://www.soundsnap.com/tags/silencer?filters=K190YWdzJTNBJTI3c2lsZW5jZXIlMjcrQU5EK3B1Ymxpc2hlZCslMjElM0QrMA==&otherfilter=K190YWdzJTNBJTI3c2lsZW5jZXIlMjc=&sorting=1&page=3&maxaudio=-10&minaudio=-100&filteredCategories=&filteredSubCategories=&filteredTags=c2lsZW5jZXI=&filteredLibraries=&audioLengthChanged=false&searchTerm=c2lsZW5jZXI=

namespace SWB_Base
{
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

        public override StatModifier StatModifier => new StatModifier
        {
            Spread = -0.05f,
            BulletVelocity = -0.05f,
        };

        public override string EffectAttachment => "muzzle2"; // New muzzle flash effect point

        public string MuzzleFlashParticle { get; set; }
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
        public override string IconPath => "attachments/swb/barrel/silencer_pistol/ui/icon.png";
        public override string ModelPath => "attachments/swb/barrel/silencer_pistol/silencer_pistol.vmdl";
    }

    public class TestSilencer : PistolSilencer
    {
        public override string Name => "Big Silencer";
        public override string Description => "This shit is OP AF, do NOT equip!!";

        public override StatModifier StatModifier => new StatModifier
        {
            Damage = 0.1f,
            Spread = -0.1f,
        };

        public override string[] Positives => new string[]
        {
        };

        public override string[] Negatives => new string[]
        {
            "Good luck aiming with this monstrosity",
        };
    }

    public class TempSilencer : PistolSilencer
    {
        public override string Name => "Temp Silencer";
    }
}
