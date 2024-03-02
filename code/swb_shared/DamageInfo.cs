namespace SWB.Shared;

public class DamageInfo
{
	public float Damage { get; set; }
	public Vector3 Force { get; set; }
	public string[] Tags { get; set; }

	public static DamageInfo FromBullet( float damage, Vector3 force )
	{
		return new()
		{
			Damage = damage,
			Force = force,
			Tags = new[] { "bullet" },
		};
	}
}
