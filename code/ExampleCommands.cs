using Sandbox;
using SWB_Base;
using SWB_Player;

internal class ExampleCommands
{
    [ConCmd.Server("kill")]
    public static void KillCommand()
    {
        var client = ConsoleSystem.Caller;

        if (client != null && client.Pawn is PlayerBase ply && ply.ActiveChild is WeaponBase weapon)
        {
            var damageInfo = new DamageInfo
            {
                Damage = ply.Health,
                Weapon = weapon
            };

            ply.TakeDamage(damageInfo);
        }
    }

    [ConCmd.Server("noclip")]
    public static void NoclipCommand()
    {
        var client = ConsoleSystem.Caller;

        if (client != null && client.Pawn is PlayerBase ply)
        {
            if (ply.DevController is PlayerNoclipController)
            {
                Log.Info("Noclip Mode Off");
                ply.DevController = null;
            }
            else
            {
                Log.Info("Noclip Mode On");
                ply.DevController = new PlayerNoclipController();
            }
        }
    }

    [ConCmd.Server("damageme", Help = "damage me")]
    public static void DamageCommand(int amount = 10)
    {
        var client = ConsoleSystem.Caller;

        if (client != null && client.Pawn is PlayerBase ply && ply.ActiveChild is WeaponBase weapon)
        {
            var damageInfo = new DamageInfo();
            damageInfo.Damage = amount;
            damageInfo.Weapon = weapon;

            ply.TakeDamage(damageInfo);
        }
    }
}
