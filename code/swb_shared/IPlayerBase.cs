using Sandbox.Citizen;
using System;

namespace SWB.Shared;

public interface IPlayerBase : IValid
{
	public CameraComponent ViewModelCamera { get; }
	public CameraComponent Camera { get; }
	public GameObject Body { get; }
	void TriggerAnimation( string animationName );
	void SetHoldType( CitizenAnimationHelper.HoldTypes holdType );
	public GameObject GameObject { get; }
	public IInventory Inventory { get; }
	public bool IsFirstPerson { get; }
	public Vector3 Velocity { get; }
	public bool IsCrouching { get; }
	public bool IsRunning { get; }
	public bool IsOnGround { get; }
	public bool IsAlive { get; }
	public Guid Id { get; }

	/// <summary>Input sensitivity modifier</summary>
	public float InputSensitivity { set; }

	/// <summary>View angles</summary>
	public Angles EyeAngles { get; }

	/// <summary>View position</summary>
	public Vector3 EyePos { get; }

	public void ApplyRecoilOffset( Angles recoilOffset );

	/// <summary>
	/// Called when the weapon wants to know how much ammo is available
	/// </summary>
	/// <param name="type">The type of ammo</param>
	/// <returns>How much ammo is available</returns>
	public int AmmoCount( string type );

	/// <summary>
	/// Called when the weapon is trying to take ammo.
	/// </summary>
	/// <param name="type">The type of ammo</param>
	/// <param name="amount">The amount of ammo requested</param>
	/// <returns>How much ammo was actually taken</returns>
	public int TakeAmmo( string type, int amount );

	/// <summary>
	/// Called when the player takes damage
	/// </summary>
	/// <param name="info">Information about the damage</param>
	public void TakeDamage( DamageInfo info );

	/// <summary>
	/// Shakes the camera
	/// </summary>
	/// <param name="screenShake">Information about the shake</param>
	public void ShakeScreen( ScreenShake screenShake );
}
