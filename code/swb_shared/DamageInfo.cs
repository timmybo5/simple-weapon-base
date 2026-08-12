using System;
using System.Collections.Generic;

namespace SWB.Shared;

/// <summary>
/// An extended version of Sandbox.DamageInfo with additional properties for SWB
/// </summary>
public class DamageInfo : Sandbox.DamageInfo
{
	public string Inflictor { get; set; }
	public Vector3 Force { get; set; }
	public float HitFlinch { get; set; }
	public MovementImpact MovementImpact { get; set; }
	public Dictionary<string, string> Extra { get; set; }

	/// <summary>
	/// Correlates every hit produced by the same fired bullet (e.g. penetration/ricochet chains). Guid.Empty means "not shot-correlated".
	/// </summary>
	public Guid ShotId { get; set; }

	public static DamageInfo FromBullet(
		GameObject attacker,
		GameObject? weapon,
		Hitbox? hitbox,
		Vector3 Position,
		PhysicsShape? shape,
		string inflictor,
		float damage,
		Vector3 origin,
		Vector3 force,
		float hitFlinch,
		MovementImpact movementImpact,
		string[] tags,
		Dictionary<string, string> extra = null,
		Guid shotId = default )
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
			HitFlinch = hitFlinch,
			MovementImpact = movementImpact,
			Tags = [.. tags, TagsHelper.Bullet],
			Extra = extra,
			ShotId = shotId
		};
	}

	public static DamageInfo FromDamageInfo( Sandbox.DamageInfo info )
	{
		return new()
		{
			Attacker = info.Attacker,
			Weapon = info.Weapon,
			Damage = info.Damage,
			Origin = info.Origin,
			Position = info.Position,
			Tags = info.Tags,
		};
	}
}
