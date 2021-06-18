using Sandbox;
using System;
using System.Linq;
using SWB_Base;

partial class DmInventory : BaseInventory
{


    public DmInventory( PlayerBase player ) : base( player )
    {

    }

    public override bool Add( Entity ent, bool makeActive = false )
    {
        var player = Owner as PlayerBase;
        var weapon = ent as WeaponBase;

        //var notices = !player.SupressPickupNotices;
        bool notices = true;

        //
        // We don't want to pick up the same weapon twice
        // But we'll take the ammo from it Winky Face
        //
        if ( weapon != null && IsCarryingType( ent.GetType() ) )
        {
            var ammo = weapon.Primary.Ammo;
            var ammoType = weapon.Primary.AmmoType;

            if ( ammo > 0 )
            {
                player.GiveAmmo( ammoType, ammo );

                if ( notices )
                {
                    Sound.FromWorld( "dm.pickup_ammo", ent.Position );
                    PickupFeed.OnPickup( To.Single( player ), $"+{ammo} {ammoType}" );
                }
            }

            //ItemRespawn.Taken( ent );

            // Despawn it
            ent.Delete();
            return false;
        }

        if ( weapon != null && notices )
        {
            Sound.FromWorld( "dm.pickup_weapon", ent.Position );
            //PickupFeed.OnPickup( To.Single( player ), $"{ent.ClassInfo.Title}" ); 
        }

        //ItemRespawn.Taken( ent );
        return base.Add( ent, makeActive );
    }

    public bool IsCarryingType( Type t )
    {
        return List.Any( x => x.GetType() == t );
    }
}
