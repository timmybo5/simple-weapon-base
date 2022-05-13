using System;
using Sandbox;

namespace SWB_Base.Attachments
{
    public class Laser : OffsetAttachment
    {
        public override string Name => "Laser";
        public override string Description => "Aids target acquisition by projecting a beam onto the target that provides a visual reference point.";
        public override string[] Positives => new string[]
        {
        };

        public override string[] Negatives => new string[]
        {
            "Visibible to enemies"
        };

        public override StatModifier StatModifier => new StatModifier
        {
            Spread = -0.05f,
        };

        public override string EffectAttachment => "laser_start"; // Laser start point

        /// <summary>Laser particle</summary>
        public string Particle { get; set; } = "particles/swb/laser/laser_small.vpcf";

        /// <summary>Laser Dot particle</summary>
        public string DotParticle { get; set; } = "particles/swb/laser/laser_dot.vpcf";

        /// <summary>Laser color</summary>
        public Color Color { get; set; } = Color.Red;

        /// <summary>Laser length</summary>
        public int Range { get; set; } = 20;

        /// <summary>Rainbow color override</summary>
        public bool RainbowColor { get; set; }

        private Particles laserParticle;
        private Particles laserDotParticle;
        private WeaponBase weapon;
        private float rainbowI;

        public Laser()
        {
            Event.Register(this);
        }

        ~Laser()
        {
            Event.Unregister(this);
        }

        private void CreateParticle()
        {
            DestroyParticle();

            laserParticle = Particles.Create(Particle);
            laserParticle?.SetPosition(3, Color);

            laserDotParticle = Particles.Create(DotParticle);
            laserDotParticle?.SetPosition(1, Color);
        }

        private void DestroyParticle()
        {
            laserParticle?.Destroy(true);
            laserParticle = null;

            laserDotParticle?.Destroy(true);
            laserDotParticle = null;
        }

        public override void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel)
        {
            this.weapon = weapon;

            if (Host.IsClient)
            {
                CreateParticle();
            }
        }

        public override void OnUnequip(WeaponBase weapon)
        {
            this.weapon = null;

            if (Host.IsClient)
            {
                DestroyParticle();
            }
        }

        [Event.Frame]
        public void OnFrame()
        {
            // Destroy laser when dropped or if weapon owner switches weapon
            if (weapon != null && (weapon.Owner == null || (weapon.Owner != null && weapon.Owner != Local.Pawn && weapon.Owner is PlayerBase player && player.ActiveChild != weapon)))
            {
                this.weapon = null;
                DestroyParticle();
                return;
            }

            // Create lasers for other clients
            if (laserParticle == null && weapon == null)
            {
                foreach (var entity in Entity.All)
                {
                    // Find weapon with active laser that has no weapon assigned
                    if (entity is WeaponBase weapon && entity.Owner != null && entity.Owner != Local.Pawn)
                    {
                        var activeAttach = weapon.GetActiveAttachment(Name);
                        if (activeAttach == null || activeAttach.WorldAttachmentModel == null) continue;

                        // Attachment weapon found
                        this.weapon = weapon;
                        CreateParticle();

                        break;
                    }
                }
            }

            // Update laser properties
            if (laserParticle != null && weapon != null && weapon.IsValid)
            {
                var activeAttach = weapon.GetActiveAttachment(Name);
                if (activeAttach == null)
                {
                    // Delete laser for other clients
                    if (weapon.Owner != Local.Pawn)
                    {
                        this.weapon = null;
                        DestroyParticle();
                    }

                    return;
                };

                Transform? laserAttach;

                // Color
                if (RainbowColor)
                {
                    rainbowI += 0.002f;

                    if (rainbowI > 1)
                        rainbowI = 0;

                    var col = ColorUtil.HSL2RGB(rainbowI, 0.5, 0.5);
                    laserParticle.SetPosition(3, col);
                    laserDotParticle.SetPosition(1, col);
                }

                var showLaser = !weapon.ShouldTuck() && !weapon.IsScoped;
                var rangeMultiplier = 1;
                var isOwner = Local.Pawn == weapon.Owner;

                laserParticle.EnableDrawing = showLaser;
                //laserDotParticle.EnableDrawing = showLaser; -> bugs out

                // Firstperson & Thirdperson
                if (isOwner && weapon.IsFirstPersonMode)
                {
                    if (activeAttach.ViewAttachmentModel == null || !activeAttach.ViewAttachmentModel.IsValid) return;
                    laserAttach = activeAttach.ViewAttachmentModel.GetAttachment(EffectAttachment);
                    rangeMultiplier = 2;
                }
                else
                {
                    if (activeAttach.WorldAttachmentModel == null || !activeAttach.WorldAttachmentModel.IsValid) return;
                    laserAttach = activeAttach.WorldAttachmentModel.GetAttachment(EffectAttachment);
                }

                var laserTrans = laserAttach.GetValueOrDefault();
                var laserStartPos = laserTrans.Position;

                laserParticle.SetPosition(0, laserStartPos);

                var owner = weapon.Owner;
                if (weapon.Owner == null) return;

                var tr = Trace.Ray(laserStartPos, laserStartPos + laserTrans.Rotation.Forward * Range * rangeMultiplier)
                                .Size(0.1f)
                                .Ignore(owner)
                                .UseHitboxes()
                                .Run();

                laserParticle.SetPosition(1, tr.EndPosition);

                //DebugOverlay.Line(laserStartPos, tr.EndPosition, 0, false);
                //LogUtil.Info(laserTrans.Rotation);
                //LogUtil.Info(laserTrans.Rotation.Forward);

                var eyePos = isOwner ? Local.Pawn.EyePosition : owner.EyePosition;
                var eyeRot = isOwner ? Local.Pawn.EyeRotation : owner.EyeRotation;
                Vector3 fromPos;
                Vector3 toPos;

                if (isOwner && weapon.IsScoped)
                {
                    // Firstperson scoped
                    fromPos = eyePos;
                    toPos = eyePos + eyeRot.Forward * 9999;
                }
                else if (isOwner && weapon.IsFirstPersonMode)
                {
                    // Firstperson
                    fromPos = laserStartPos;
                    toPos = laserStartPos + laserTrans.Rotation.Forward * 2000;
                }
                else
                {
                    // Thirdperson
                    fromPos = eyePos;
                    toPos = eyePos + eyeRot.Forward * 2000;
                }

                TraceResult trDot = Trace.Ray(fromPos, toPos)
                                    .Size(0.1f)
                                    .Ignore(owner)
                                    .UseHitboxes()
                                    .Run();

                laserDotParticle.SetPosition(0, trDot.EndPosition);
            }
        }
    }

    public class SmallLaser : Laser
    {
        public override string Name => "CMR-207 Laser";
        public override string IconPath => "attachments/swb/tactical/laser_small/ui/icon.png";
        public override string ModelPath => "attachments/swb/tactical/laser_small/w_laser_small.vmdl";
    }

    public class SmallLaserRed : SmallLaser
    {
        public override string Name => "CMR-207 Laser (red)";
        public override string IconPath => "attachments/swb/tactical/laser_small/ui/icon_red.png";
    }
    public class SmallLaserBlue : SmallLaser
    {
        public override string Name => "CMR-207 Laser (blue)";
        public override string IconPath => "attachments/swb/tactical/laser_small/ui/icon_blue.png";
    }
    public class SmallLaserGreen : SmallLaser
    {
        public override string Name => "CMR-207 Laser (green)";
        public override string IconPath => "attachments/swb/tactical/laser_small/ui/icon_green.png";
    }
    public class SmallLaserRainbow : SmallLaser
    {
        public override string Name => "CMR-207 Laser (rainbow)";
        public override string IconPath => "attachments/swb/tactical/laser_small/ui/icon_rainbow.png";
    }

    public class RifleLaser : Laser
    {
        public override string Name => "PEQ-15 Laser";
        public override string IconPath => "attachments/swb/tactical/laser_rifle/ui/icon.png";
        public override string ModelPath => "attachments/swb/tactical/laser_rifle/w_laser_rifle.vmdl";
    }

    public class RifleLaserRed : RifleLaser
    {
        public override string Name => "PEQ-15 Laser (red)";
        public override string IconPath => "attachments/swb/tactical/laser_rifle/ui/icon_red.png";
    }
    public class RifleLaserBlue : RifleLaser
    {
        public override string Name => "PEQ-15 Laser (blue)";
        public override string IconPath => "attachments/swb/tactical/laser_rifle/ui/icon_blue.png";
    }

    public class RifleLaserGreen : RifleLaser
    {
        public override string Name => "PEQ-15 Laser (green)";
        public override string IconPath => "attachments/swb/tactical/laser_rifle/ui/icon_green.png";
    }

    public class RifleLaserRainbow : RifleLaser
    {
        public override string Name => "PEQ-15 Laser (rainbow)";
        public override string IconPath => "attachments/swb/tactical/laser_rifle/ui/icon_rainbow.png";
    }
}
