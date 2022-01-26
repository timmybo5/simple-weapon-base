using System.Collections.Generic;
using Sandbox;
using SWB_Base;
using SWB_Base.Attachments;

namespace SWB_WEAPONS
{
    [Library("swb_css_spas12", Title = "SPAS-12")]
    public class SPAS12 : WeaponBaseShotty
    {
        public override int Bucket => 2;
        public override HoldType HoldType => HoldType.Shotgun;
        public override string HandsModelPath => "weapons/swb/hands/swat/v_hands_swat.vmdl";
        public override string ViewModelPath => "weapons/swb/shotguns/spas/v_spas12.vmdl";
        public override string WorldModelPath => "weapons/swb/shotguns/spas/w_spas12.vmdl";
        public override string Icon => "/swb_weapons/textures/spas12.png";
        public override int FOV => 75;
        public override int ZoomFOV => 75;
        public override float WalkAnimationSpeedMod => 0.9f;

        public override float ShellReloadTimeStart => 0.4f;
        public override float ShellReloadTimeInsert => 0.65f;
        public override float ShellEjectDelay => 0.5f;

        public SPAS12()
        {
            Primary = new ClipInfo
            {
                Ammo = 8,
                AmmoType = AmmoType.Shotgun,
                ClipSize = 8,

                Bullets = 8,
                BulletSize = 2f,
                Damage = 15f,
                Force = 5f,
                Spread = 0.3f,
                Recoil = 2f,
                RPM = 80,
                FiringType = FiringType.semi,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 1.0f,
                    Rotation = 0.5f
                },

                DryFireSound = "swb_shotty.empty",
                ShootSound = "spas12.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_medium.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(0.08f, -0.06f, 0f),
                Pos = new Vector3(-5f, 0f, 1.95f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 50, 0),
                Pos = new Vector3(10, -2, 0)
            };

            CustomizeAnimData = new AngPos
            {
                Angle = new Angles(-2.25f, 51.84f, 0f),
                Pos = new Vector3(11.22f, -4.96f, 1.078f)
            };

            // Attachments //
            AttachmentCategories = new List<AttachmentCategory>()
            {
                new AttachmentCategory
                {
                    Name = AttachmentCategoryName.Muzzle,
                    BoneOrAttachment = "muzzle",
                    Attachments = new List<AttachmentBase>()
                    {
                        new ShotgunSilencer
                        {
                            MuzzleFlashParticle = "particles/swb/muzzle/flash_medium_silenced.vpcf",
                            ShootSound = "swb_shotgun.silenced.fire",
                            ViewParentBone = "shotgun",
                            ViewTransform = new Transform {
                                Position = new Vector3(0.004f, 3.4f, 36.623f),
                                Rotation = Rotation.From(new Angles(90f, 0f, -90f)),
                                Scale = 12f
                            },
                            WorldParentBone = "shotgun",
                            WorldTransform = new Transform {
                                Position = new Vector3(-0.011f, 3.4f, 36.616f),
                                Rotation = Rotation.From(new Angles(90f, 0f, -90f)),
                                Scale = 12f
                            },
                        }
                    }
                },
                new AttachmentCategory
                {
                    Name = AttachmentCategoryName.Tactical,
                    BoneOrAttachment = "",
                    Attachments = new List<AttachmentBase>()
                    {
                        new SmallLaserRed
                        {
                            Color = Color.Red,
                            ViewParentBone = "shotgun",
                            ViewTransform = new Transform {
                                Position = new Vector3(0f, 1.5f, 31.2f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 180f)),
                                Scale = 5.269f
                            },
                            WorldParentBone = "shotgun",
                            WorldTransform = new Transform {
                                Position = new Vector3(0f, 1.5f, 31.2f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 180f)),
                                Scale = 5.269f
                            },
                        },
                        new SmallLaserBlue
                        {
                            Color = Color.Blue,
                            ViewParentBone = "shotgun",
                            ViewTransform = new Transform {
                                Position = new Vector3(0f, 1.5f, 31.2f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 180f)),
                                Scale = 5.269f
                            },
                            WorldParentBone = "shotgun",
                            WorldTransform = new Transform {
                                Position = new Vector3(0f, 1.5f, 31.2f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 180f)),
                                Scale = 5.269f
                            },
                        },
                        new SmallLaserGreen
                        {
                            Color = Color.Green,
                            ViewParentBone = "shotgun",
                            ViewTransform = new Transform {
                                Position = new Vector3(0f, 1.5f, 31.2f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 180f)),
                                Scale = 5.269f
                            },
                            WorldParentBone = "shotgun",
                            WorldTransform = new Transform {
                                Position = new Vector3(0f, 1.5f, 31.2f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 180f)),
                                Scale = 5.269f
                            },
                        },
                        new SmallLaserRainbow
                        {
                            RainbowColor = true,
                            ViewParentBone = "shotgun",
                            ViewTransform = new Transform {
                                Position = new Vector3(0f, 1.5f, 31.2f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 180f)),
                                Scale = 5.269f
                            },
                            WorldParentBone = "shotgun",
                            WorldTransform = new Transform {
                                Position = new Vector3(0f, 1.5f, 31.2f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 180f)),
                                Scale = 5.269f
                            },
                        },
                    }
                }
            };
        }
    }
}
