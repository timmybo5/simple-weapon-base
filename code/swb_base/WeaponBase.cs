using Sandbox;
using System.Collections.Generic;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base
{

    public partial class WeaponBase : BaseCarriable
    {
        public override void Spawn()
        {
            base.Spawn();

            CollisionGroup = CollisionGroup.Weapon;
            SetInteractsAs( CollisionLayer.Debris );

            SetModel( WorldModelPath );

            PickupTrigger = new PickupTrigger();
            PickupTrigger.Parent = this;
            PickupTrigger.Position = Position;
        }

        public override void ActiveStart( Entity ent )
        {
            base.ActiveStart( ent );

            TimeSinceDeployed = 0;
            IsReloading = false;

            // Animated activity status will reset when weapon is switched out
            if ( AnimatedActions != null )
            {
                for ( int i = 0; i < AnimatedActions.Count; i++ )
                {
                    if ( AnimatedActions[i].isToggled )
                        AnimatedActions[i].HandleOnDeploy( this );
                }
            }

            // Dualwield setup
            if ( DualWield )
            {
                if ( !isDualWieldConverted )
                {
                    isDualWieldConverted = true;
                    Primary.Ammo *= 2;
                    Primary.ClipSize *= 2;
                    Primary.RPM = (int)(Primary.RPM * 1.25);
                    ZoomAnimData = null;
                    RunAnimData = null;
                }

                if ( IsLocalPawn )
                    dualWieldViewModel?.SetAnimBool( "deploy", true );
            }
        }

        public override void ActiveEnd( Entity ent, bool dropped )
        {
            base.ActiveEnd( ent, dropped );

            if ( DualWield && dualWieldViewModel != null )
            {
                dualWieldViewModel.Delete();
            }
        }

        // BaseSimulate
        public void BaseSimulate( Client player )
        {
            if ( Input.Down( InputButton.Reload ) )
            {
                Reload();
            }

            // Reload could have deleted us
            if ( !this.IsValid() )
                return;

            if ( CanPrimaryAttack() )
            {
                TimeSincePrimaryAttack = 0;
                AttackPrimary();
            }

            // AttackPrimary could have deleted us
            if ( !player.IsValid() )
                return;

            if ( CanSecondaryAttack() )
            {
                TimeSinceSecondaryAttack = 0;
                AttackSecondary();
            }
        }

        public override void Simulate( Client owner )
        {

            if ( IsAnimating ) return;

            // Handle custom animation actions
            if ( AnimatedActions != null && !IsReloading )
            {
                for ( int i = 0; i < AnimatedActions.Count; i++ )
                {
                    if ( AnimatedActions[i].Handle( owner, this ) )
                        return;
                }
            }

            IsRunning = Input.Down( InputButton.Run ) && RunAnimData != null && Owner.Velocity.Length >= 200;

            if ( Secondary == null && ZoomAnimData != null && !(this is WeaponBaseMelee) )
                IsZooming = Input.Down( InputButton.Attack2 ) && !IsRunning && !IsReloading;

            if ( TimeSinceDeployed < 0.6f )
                return;

            if ( !IsReloading )
            {
                BaseSimulate( owner );
            }

            if ( IsReloading && TimeSinceReload > Primary.ReloadTime )
            {
                OnReloadFinish();
            }
        }

        public virtual void Reload()
        {
            if ( IsReloading || IsAnimating )
                return;

            if ( Primary.Ammo >= Primary.ClipSize )
                return;

            TimeSinceReload = 0;

            if ( Owner is PlayerBase player )
            {
                if ( player.AmmoCount( Primary.AmmoType ) <= 0 && Primary.InfiniteAmmo != InfiniteAmmoType.reserve )
                    return;
            }

            IsReloading = true;

            // Player anim
            (Owner as AnimEntity).SetAnimBool( "b_reload", true );

            StartReloadEffects();
        }

        public virtual void OnReloadFinish()
        {
            IsReloading = false;

            // Dual wield
            if ( DualWield && !dualWieldShouldReload )
            {
                dualWieldShouldReload = true;
                Reload();
                return;
            }

            dualWieldShouldReload = false;

            if ( Primary.InfiniteAmmo == InfiniteAmmoType.reserve )
            {
                Primary.Ammo = Primary.ClipSize;
                return;
            }

            if ( Owner is PlayerBase player )
            {
                var ammo = player.TakeAmmo( Primary.AmmoType, Primary.ClipSize - Primary.Ammo );
                if ( ammo == 0 )
                    return;

                Primary.Ammo += ammo;
            }
        }

        [ClientRpc]
        public virtual void StartReloadEffects()
        {
            var reloadingViewModel = DualWield && dualWieldShouldReload ? dualWieldViewModel : ViewModelEntity;

            if ( Primary.ReloadAnim != null )
                reloadingViewModel?.SetAnimBool( Primary.ReloadAnim, true );

            // TODO - player third person model reload
        }

        public override void BuildInput( InputBuilder input )
        {
            // Mouse sensitivity
            if ( IsZooming )
            {
                input.ViewAngles = MathZ.FILerp( input.OriginalViewAngles, input.ViewAngles, AimSensitivity * 90 );
            }

            // Recoil
            if ( doRecoil )
            {
                doRecoil = false;
                var recoilAngles = new Angles( IsZooming ? -Primary.Recoil * 0.4f : -Primary.Recoil, 0, 0 );
                input.ViewAngles += recoilAngles;
            }
        }

        public override void CreateViewModel()
        {
            Host.AssertClient();

            if ( string.IsNullOrEmpty( ViewModelPath ) )
                return;

            ViewModelEntity = new ViewModelBase( this );
            ViewModelEntity.Position = Position; // --> Does not seem to do anything
            ViewModelEntity.Owner = Owner;
            ViewModelEntity.EnableViewmodelRendering = true;
            ViewModelEntity.SetModel( ViewModelPath );

            if ( DualWield )
            {
                dualWieldViewModel = new ViewModelBase( this, true );
                dualWieldViewModel.Owner = Owner;
                dualWieldViewModel.EnableViewmodelRendering = true;
                dualWieldViewModel.SetModel( ViewModelPath );
            }

        }

        public override void CreateHudElements()
        {
            if ( Local.Hud == null ) return;

            if ( DrawCrosshair )
            {
                CrosshairPanel = new Crosshair();
                CrosshairPanel.Parent = Local.Hud;
                CrosshairPanel.AddClass( ClassInfo.Name );
            }
        }

        public bool TakeAmmo( int amount )
        {
            if ( Primary.InfiniteAmmo == InfiniteAmmoType.clip )
                return true;

            if ( Primary.Ammo < amount )
                return false;

            Primary.Ammo -= amount;
            return true;
        }

        public int AvailableAmmo()
        {
            var owner = Owner as PlayerBase;
            if ( owner == null ) return 0;

            // Show clipsize as the available ammo
            if ( Primary.InfiniteAmmo == InfiniteAmmoType.reserve )
                return Primary.ClipSize;

            return owner.AmmoCount( Primary.AmmoType );
        }

        public bool IsUsable()
        {
            if ( Primary.Ammo > 0 ) return true;
            return AvailableAmmo() > 0;
        }

        public override void OnCarryStart( Entity carrier )
        {
            base.OnCarryStart( carrier );

            if ( PickupTrigger.IsValid() )
            {
                PickupTrigger.EnableTouch = false;
            }
        }

        public override void OnCarryDrop( Entity dropper )
        {

            if ( !DropWeaponOnDeath )
            {
                this.Delete();
                return;
            }

            base.OnCarryDrop( dropper );

            if ( PickupTrigger.IsValid() )
            {
                PickupTrigger.EnableTouch = true;
            }
        }

        public override void SimulateAnimator( PawnAnimator anim )
        {
            anim.SetParam( "holdtype", (int)HoldType );
            anim.SetParam( "aimat_weight", 1.0f );
        }

        [ClientRpc]
        public virtual void SendWeaponAnim( string anim, bool value = true )
        {
            ViewModelEntity?.SetAnimBool( anim, value );
        }
    }
}
