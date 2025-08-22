using Sandbox.Citizen;
using System;

namespace SWB.Shared;

public interface IPlayerBase : IValid
{
	/// <summary>
	/// The camera to use when renderering the weapon's view model on the client side
	/// If none is provided then no view model will be rendered
	/// </summary>
	public CameraComponent? ViewModelCamera { get; }

	/// <summary>
	/// The camera used for rendering the player's view
	/// </summary>
	public CameraComponent Camera { get; }

	/// <summary>
	/// The game object which represents the player
	/// </summary>
	public GameObject GameObject { get; }

	/// <summary>
	/// Whether the player is in first person view
	/// </summary>
	public bool IsFirstPerson { get; }

	/// <summary>
	/// The player's current velocity
	/// </summary>
	public Vector3 Velocity { get; }

	public bool IsCrouching { get; }

	public bool IsRunning { get; }

	public bool IsOnGround { get; }

	public bool IsAlive { get; }

	public Guid Id { get; }

	/// <summary>View angles</summary>
	public Angles EyeAngles { get; }

	/// <summary>View position</summary>
	public Vector3 EyePos { get; }

	/// <summary>
	/// Called when the weapon wants to know how much ammo is available
	/// </summary>
	/// <param name="type">The type of ammo</param>
	/// <returns>How much ammo is available</returns>
	public int AmmoCount( string type );

	/// <summary>Input sensitivity modifier</summary>
	public float InputSensitivity { set; }

	/// <summary>
	/// Called when the weapon wants to trigger an animation
	/// </summary>
	/// <param name="animationName">The name of the animation to trigger, could be b_attack or b_reload</param>
	void TriggerAnimation( string animationName );

	/// <summary>
	/// Called when the weapon wants to change the hold type of the player
	/// </summary>
	/// <param name="holdType">The new hold type to set</param>
	void SetHoldType( CitizenAnimationHelper.HoldTypes holdType );

	/// <summary>
	/// Triggered when the weapon wants to an angular offset to the player's view
	/// to simulate recoil. Called when a weapon is fired.
	/// </summary>
	/// <param name="recoilOffset">The suggested angular offset to apply</param>
	public void ApplyRecoilOffset( Angles recoilOffset );

	/// <summary>
	/// Called when the weapon object should be attached/parented to the player's body
	/// </summary>
	/// <param name="weaponObject">The game object representing the weapon</param>
	/// <param name="boneName">The suggested bone to parent to</param>
	public void ParentWeaponToBone( GameObject weaponObject, string boneName );

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
