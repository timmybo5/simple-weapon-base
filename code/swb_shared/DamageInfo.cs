using System;
using System.Linq;

namespace SWB.Shared;

public class DamageInfo
{
	public Guid AttackerId { get; set; }
	public string Inflictor { get; set; }
	public float Damage { get; set; }
	public Vector3 Origin { get; set; }
	public Vector3 Force { get; set; }
	public string[] Tags { get; set; }

	public static DamageInfo FromBullet( Guid attackerId, string inflictor, float damage, Vector3 origin, Vector3 force, string[] tags )
	{
		return new()
		{
			AttackerId = attackerId,
			Inflictor = inflictor,
			Damage = damage,
			Origin = origin,
			Force = force,
			Tags = new[] { "bullet" }.Concat( tags ).ToArray(),
		};
	}
}
