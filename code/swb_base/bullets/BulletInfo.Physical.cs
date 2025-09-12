using SWB.Shared;
using System;

namespace SWB.Base;

/// <summary>
/// A physical bullet that simulates a bullet travelling over time through the air
/// </summary>
[Group( "SWB" )]
[Title( "Physical Bullet Info" )]
public class PhysicalBulletInfo : BulletInfo
{
	[Property, Range( 0, 20000, false, true ), Step( 500f )]
	public float Velocity { get; set; } = 4000f;

	[Property, Range( 0, 1000, false, true ), Step( 5f )]
	public float Gravity { get; set; } = 65;

	[Property, Range( 0, 0.04f, false, true ), Step( 0.001f )]
	public float Drag { get; set; } = 0.01f;

	public override void Shoot( Weapon weapon, ShootInfo shootInfo, Vector3 spreadOffset )
	{
		if ( IsProxy || !weapon.IsValid ) return;

		var player = weapon.Owner;
		if ( player is null ) return;

		var forward = player.EyeAngles.Forward + spreadOffset;
		forward = forward.Normal;
		var bulletVelocity = forward * Velocity;

		GameObject bulletObject;

		var muzzleTransform = weapon.GetMuzzleTransform();
		var originPosition = muzzleTransform.HasValue && player.IsFirstPerson ? muzzleTransform.Value.Position : player.EyePos;
		var bulletTransform = new Transform( originPosition, player.EyeAngles );

		if ( ShouldSpawnTracer( shootInfo ) )
		{
			bulletObject = shootInfo.BulletTracerParticle.Clone( new CloneConfig
			{
				Transform = bulletTransform,
			} );
		}
		else
		{
			bulletObject = Scene.CreateObject();
			bulletObject.WorldTransform = bulletTransform;
		}

		bulletObject.Name = TagsHelper.PhysicalBullet;
		bulletObject.Tags.Add( TagsHelper.PhysicalBullet );
		bulletObject.GetOrAddComponent<PhysicalBulletMover>().Initialize( this, weapon, shootInfo, bulletVelocity );
		bulletObject.NetworkInterpolation = false;
		bulletObject.NetworkSpawn();
	}
}
