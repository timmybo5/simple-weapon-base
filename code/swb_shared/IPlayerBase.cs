using System;

namespace SWB.Shared;



/// <summary>
/// Implement this interface on a `Component` to integrate with SWB
/// </summary>
public interface IPlayerBase : IValid, Sandbox.Component.IDamageable
{
	/// <summary>
	/// Unique identifier for the player
	/// Typically implemented by `Component`
	/// </summary>
	public Guid Id { get; }

	/// <summary>
	/// The game object that owns this component
	/// Typically implemented by `Component`
	/// </summary>
	public GameObject GameObject { get; }

	/// <summary>
	/// The camera to use when renderering the weapon's view model on the client side
	/// If none is provided and first person mode is enabled, then a camera will be created
	/// </summary>
	public CameraComponent? ViewModelCamera { get; set; }

	/// <summary>
	/// The camera used for rendering the player's first person view
	/// Used to calculate view model sway
	/// The Render exclude tag "viewmodel" will be automatically applied to prevent render issues
	/// </summary>
	public CameraComponent? Camera { get; }

	/// <summary>
	/// Whether the player is in first person view
	/// </summary>
	public bool IsFirstPerson { get; }

	/// <summary>
	/// The player's current velocity
	/// </summary>
	public Vector3 Velocity { get; }

	/// <summary>
	/// Whether the player is crouching, this will effect aim and recoil
	/// </summary>
	public bool IsCrouching { get; }

	/// <summary>
	/// Whether the player is running, this will effect aim and recoil
	/// </summary>
	public bool IsRunning { get; }

	/// <summary>
	/// Whether the player is on the ground, this will effect aim and recoil
	/// </summary>
	public bool IsOnGround { get; }

	/// <summary>
	/// Whether the player is alive.
	/// Damage will not be dealt to dead players.
	/// </summary>
	public bool IsAlive { get; }

	/// <summary>
	/// View angle of the player, used to determine the direction to shoot a bullet
	/// </summary>
	public Angles EyeAngles { get; }

	/// <summary>
	/// View position of the player, used to determine the origin point of a fired bullet
	/// </summary>
	public Vector3 EyePos { get; }

	/// <summary>
	/// Input sensitivity modifier based on player ADS (aim down sights) state
	/// </summary>
	public float InputSensitivity { set; }

	/// <summary>
	/// The suggested FOV to be used by the player camera, affected by a weapon zoom
	/// This assumes a first-person perspective and will default to `Preferences.FieldOfView`
	/// </summary>
	public float FieldOfView { set; }

	/// <summary>
	/// The Hold Type for the currently equipped weapon
	/// </summary>
	public HoldTypes HoldType { set; }

	/// <summary>
	/// Called when the weapon wants to know how much ammo is available
	/// </summary>
	/// <param name="type">The type of ammo</param>
	/// <returns>How much ammo is available</returns>
	public int AmmoCount( string type );

	/// <summary>
	/// Called when the weapon wants to trigger an animation on the player object
	/// </summary>
	/// <param name="animation">The animation to trigger</param>
	void TriggerAnimation( Animations animation );

	/// <summary>
	/// Triggered when the weapon wants to apply an angular offset to the player's view to simulate recoil.
	/// Called when a weapon is fired.
	/// </summary>
	/// <param name="recoilOffset">The suggested angular offset to apply</param>
	public void ApplyRecoilOffset( Angles recoilOffset );

	/// <summary>
	/// Called when the weapon object should be attached/parented to the player's body
	/// </summary>
	/// <param name="object">The game object to parent</param>
	/// <param name="boneName">The suggested bone to parent to</param>
	public void ParentToBone( GameObject @object, string boneName );

	/// <summary>
	/// Called when the weapon is trying to take ammo.
	/// </summary>
	/// <param name="type">The type of ammo</param>
	/// <param name="amount">The amount of ammo requested</param>
	/// <returns>How much ammo was actually taken</returns>
	public int TakeAmmo( string type, int amount );

	/// <summary>
	/// Shakes the camera
	/// </summary>
	/// <param name="screenShake">Information about the shake</param>
	public void ShakeScreen( ScreenShake screenShake );
}
