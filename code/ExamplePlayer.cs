using System.Linq;
using Sandbox;
using SWB_Base;

public partial class ExamplePlayer : PlayerBase
{
    public bool SupressPickupNotices { get; set; }
    TimeSince timeSinceDropped;

    public ExamplePlayer()
    {
        Inventory = new ExampleInventory(this);
    }

    public override void Respawn()
    {
        base.Respawn();

        // Initialize player
        SetModel("models/citizen/citizen.vmdl");

        Controller = new WalkController();
        Animator = new StandardPlayerAnimator();
        CameraMode = new FirstPersonCamera();

        EnableAllCollisions = true;
        EnableDrawing = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;

        Health = 100;

        ClearAmmo();

        // Give weapons and ammo
        SupressPickupNotices = true;

        Inventory.Add(new SWB_WEAPONS.Bayonet());
        Inventory.Add(new SWB_WEAPONS.DEAGLE());
        Inventory.Add(new SWB_WEAPONS.SPAS12());
        Inventory.Add(new SWB_WEAPONS.UMP45());
        Inventory.Add(new SWB_WEAPONS.FAL());
        Inventory.Add(new SWB_WEAPONS.L96A1());

        Inventory.Add(new SWB_EXPLOSIVES.RPG7());

        GiveAmmo(AmmoType.SMG, 100);
        GiveAmmo(AmmoType.Pistol, 60);
        GiveAmmo(AmmoType.Revolver, 60);
        GiveAmmo(AmmoType.Rifle, 60);
        GiveAmmo(AmmoType.Shotgun, 60);
        GiveAmmo(AmmoType.Sniper, 60);
        GiveAmmo(AmmoType.Grenade, 60);
        GiveAmmo(AmmoType.RPG, 60);

        SupressPickupNotices = false;
    }

    public override void Simulate(Client cl)
    {
        base.Simulate(cl);

        // Input requested a weapon switch
        if (Input.ActiveChild != null)
        {
            ActiveChild = Input.ActiveChild;
        }

        if (LifeState != LifeState.Alive)
            return;

        TickPlayerUse();

        if (Input.Pressed(InputButton.View))
        {
            if (CameraMode is ThirdPersonCamera)
            {
                CameraMode = new FirstPersonCamera();
            }
            else
            {
                CameraMode = new ThirdPersonCamera();
            }
        }

        if (Input.Pressed(InputButton.Drop))
        {
            var dropped = Inventory.DropActive();
            if (dropped != null)
            {
                if (dropped.PhysicsGroup != null)
                {
                    dropped.PhysicsGroup.Velocity = Velocity + (EyeRotation.Forward + EyeRotation.Up) * 300;
                }

                timeSinceDropped = 0;
                SwitchToBestWeapon();
            }
        }

        SimulateActiveChild(cl, ActiveChild);

        //
        // If the current weapon is out of ammo and we last fired it over half a second ago
        // lets try to switch to a better wepaon
        //
        if (ActiveChild is WeaponBase weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f && weapon.TimeSinceSecondaryAttack > 0.5f)
        {
            SwitchToBestWeapon();
        }
    }

    public override void StartTouch(Entity other)
    {
        if (timeSinceDropped < 1) return;

        base.StartTouch(other);
    }

    public override void OnKilled()
    {
        base.OnKilled();

        Inventory.DropActive();
        Inventory.DeleteContents();

        BecomeRagdollOnClient(LastDamage.Force, GetHitboxBone(LastDamage.HitboxIndex));

        Controller = null;
        CameraMode = new SpectateRagdollCamera();

        EnableAllCollisions = false;
        EnableDrawing = false;
    }

    public void SwitchToBestWeapon()
    {
        var best = Children.Select(x => x as WeaponBase)
            .Where(x => x.IsValid() && x.IsUsable())
            .OrderByDescending(x => x.BucketWeight)
            .FirstOrDefault();

        if (best == null) return;

        ActiveChild = best;
    }
}
