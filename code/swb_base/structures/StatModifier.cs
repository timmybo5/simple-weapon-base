namespace SWB_Base
{
    public class StatModifier
    {
        public float Damage { get; set; } = 0;
        public float Recoil { get; set; } = 0;
        public float Spread { get; set; } = 0;
        public float RPM { get; set; } = 0;
        public float BulletVelocity { get; set; } = 0;

        public static readonly StatModifier Zero = new();

        public void Apply(WeaponBase weapon)
        {
            weapon.Primary.Damage += weapon.InitialStats.Damage * Damage;
            weapon.Primary.Recoil += weapon.InitialStats.Recoil * Recoil;
            weapon.Primary.Spread += weapon.InitialStats.Spread * Spread;
            weapon.Primary.RPM += (int)(weapon.InitialStats.RPM * RPM);

            // Temp bullet velocity
            weapon.Primary.Damage += weapon.InitialStats.Damage * BulletVelocity;
        }

        public void Remove(WeaponBase weapon)
        {
            weapon.Primary.Damage -= weapon.InitialStats.Damage * Damage;
            weapon.Primary.Recoil -= weapon.InitialStats.Recoil * Recoil;
            weapon.Primary.Spread -= weapon.InitialStats.Spread * Spread;
            weapon.Primary.RPM -= (int)(weapon.InitialStats.RPM * RPM);

            // Temp bullet velocity
            weapon.Primary.Damage -= weapon.InitialStats.Damage * BulletVelocity;
        }
    }
}
