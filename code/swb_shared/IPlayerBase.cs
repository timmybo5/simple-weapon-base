using Sandbox.Citizen;
using System;

namespace SWB.Shared;

public interface IPlayerBase : IValid
{
	public CameraComponent ViewModelCamera { get; set; }
	public CameraComponent Camera { get; set; }
	public GameObject Body { get; set; }
	public SkinnedModelRenderer BodyRenderer { get; set; }
	public CharacterController CharacterController { get; set; }
	public CitizenAnimationHelper AnimationHelper { get; set; }
	public GameObject GameObject { get; }
	public IInventory Inventory { get; set; }
	public bool IsFirstPerson { get; }
	public Vector3 Velocity { get; }
	public bool IsCrouching { get; set; }
	public bool IsRunning { get; set; }
	public bool IsOnGround { get; }
	public bool IsAlive { get; }
	public int MaxHealth { get; set; }
	public int Health { get; set; }
	public int Kills { get; set; }
	public int Deaths { get; set; }
	public Guid Id { get; }

	/// <summary>Input sensitivity modifier</summary>
	public float InputSensitivity { get; set; }

	/// <summary>EyeAngles offset (should reset after being applied)</summary>
	public Angles EyeAnglesOffset { get; set; }

	/// <summary>View angles</summary>
	public Angles EyeAngles { get; set; }

	/// <summary>View position</summary>
	public Vector3 EyePos { get; }

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
}
