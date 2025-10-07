using System;
using System.Linq;

namespace SWB.Shared;

/// <summary>
/// An extended version of Sandbox.DamageInfo with additional properties for SWB
/// </summary>
public class DamageInfo : Sandbox.DamageInfo
{
	public string Inflictor { get; set; }
	public Vector3 Force { get; set; }
	private static readonly string[] bulletTag = ["bullet"];

	public static DamageInfo FromBullet( GameObject attacker, GameObject? weapon, Hitbox? hitbox, Vector3 Position, PhysicsShape? shape, string inflictor, float damage, Vector3 origin, Vector3 force, string[] tags )
	{
		return new()
		{
			Attacker = attacker,
			Weapon = weapon,
			Hitbox = hitbox,
			Position = Position,
			Shape = shape,
			Inflictor = inflictor,
			Damage = damage,
			Origin = origin,
			Force = force,
			Tags = [.. bulletTag.Concat( tags )],
		};
	}
}
