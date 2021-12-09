using System;
using Sandbox;

// Sounds Effects: https://www.soundsnap.com/tags/silencer?filters=K190YWdzJTNBJTI3c2lsZW5jZXIlMjcrQU5EK3B1Ymxpc2hlZCslMjElM0QrMA==&otherfilter=K190YWdzJTNBJTI3c2lsZW5jZXIlMjc=&sorting=1&page=3&maxaudio=-10&minaudio=-100&filteredCategories=&filteredSubCategories=&filteredTags=c2lsZW5jZXI=&filteredLibraries=&audioLengthChanged=false&searchTerm=c2lsZW5jZXI=

namespace SWB_Base
{
    public class Silencer : OffsetAttachment
    {
        public override string Name => "Silencer";
        public override string Description => "Reduces firing sound";
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
        public override string IconPath => "path";
        public override string ModelPath => "attachments/swb/barrel/silencer_pistol/silencer_pistol.vmdl";
    }
}
