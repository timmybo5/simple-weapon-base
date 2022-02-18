using Sandbox;
using System;
using System.Collections.Generic;

namespace SWB_Base
{
    public enum AmmoType
    {
        Pistol,
        Revolver,
        Shotgun,
        SMG,
        Rifle,
        Sniper,
        LMG,
        Crossbow,
        RPG,
        Explosive,
        Grenade
    }

    public enum InfiniteAmmoType
    {
        /// <summary>Infinite clip ammo, no need to reload</summary>
        clip = 1,
        /// <summary>Infinite reserve ammo, can always reload</summary>
        reserve = 2
    }

    partial class PlayerBase
    {

        [Net]
        public List<int> Ammo { get; set; }

        public virtual void ClearAmmo()
        {
            Ammo.Clear();
        }

        public virtual int AmmoCount(AmmoType type)
        {
            var iType = (int)type;
            if (Ammo == null) return 0;
            if (Ammo.Count <= iType) return 0;

            return Ammo[(int)type];
        }

        public virtual bool SetAmmo(AmmoType type, int amount)
        {
            var iType = (int)type;
            if (!Host.IsServer) return false;
            if (Ammo == null) return false;

            while (Ammo.Count <= iType)
            {
                Ammo.Add(0);
            }

            Ammo[(int)type] = amount;
            return true;
        }

        public virtual bool GiveAmmo(AmmoType type, int amount)
        {
            if (!Host.IsServer) return false;
            if (Ammo == null) return false;

            SetAmmo(type, AmmoCount(type) + amount);
            return true;
        }

        public virtual int TakeAmmo(AmmoType type, int amount)
        {
            if (Ammo == null) return 0;

            var available = AmmoCount(type);
            amount = Math.Min(available, amount);

            SetAmmo(type, available - amount);

            return amount;
        }

        public virtual bool HasAmmo(AmmoType type)
        {
            return AmmoCount(type) > 0;
        }
    }
}
