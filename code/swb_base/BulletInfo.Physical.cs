using System;

namespace SWB.Base;

/// <summary>
/// A physical bullet that simulates a bullet travelling over time through the air
/// </summary>
[Group( "SWB" )]
[Title( "Physical Bullet Info" )]
public class PhysicalBulletInfo : BulletInfo
{
	[Property( Name = "Velocity" ), Range( 0, 10000, false, true ), Step( 500f )]
	public float BulletVelocity { get; set; } = 4000f;

	[Property( Name = "Gravity" ), Range( 0, 200, false, true ), Step( 5f )]
	public float BulletGravity { get; set; } = 65;

	[Property( Name = "Drag" ), Range( 0, 0.02f, false, true ), Step( 0.001f )]
	public float BulletDrag { get; set; } = 0.01f;

	[Property( Name = "Prefab" )]
	public PrefabFile BulletTracerPrefab { get; set; }

	[Property( Name = "Bullet Prefab" )]
	public float BulletTracerChance { get; set; } = 0.5f;

	private bool ShouldSpawnTracer()
	{
		var random = new Random();
		var randVal = random.NextDouble();
		return randVal < BulletTracerChance;
	}

	public override void Shoot( Weapon weapon, ShootInfo shootInfo, Vector3 spreadOffset )
	{
		if ( !weapon.IsValid ) return;

		var player = weapon.Owner;
		if ( player is null ) return;

		var forward = player.EyeAngles.Forward + spreadOffset;
		forward = forward.Normal;
		var bulletVelocity = forward * BulletVelocity;

		GameObject bulletObject;

		var muzzleTransform = weapon.GetMuzzleTransform();
		var originPosition = muzzleTransform.HasValue ? muzzleTransform.Value.Position : player.EyePos;

		if ( BulletTracerPrefab is not null && ShouldSpawnTracer() )
		{
			bulletObject = GameObject.Clone( BulletTracerPrefab, new CloneConfig
			{
				Transform = new Transform( originPosition, player.EyeAngles ),
			} );
		}
		else
		{
			bulletObject = Scene.CreateObject();
			bulletObject.WorldTransform = new Transform( originPosition, player.EyeAngles );
		}

		bulletObject.GetOrAddComponent<PhysicalBulletMover>().Initialize( this, weapon, shootInfo, bulletVelocity );


		bulletObject.NetworkSpawn();
	}
}

