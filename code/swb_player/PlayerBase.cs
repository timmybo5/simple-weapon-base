using SWB.Base;
using SWB.Shared;
using System;
using System.Linq;

namespace SWB.Player;

[Group( "SWB" )]
[Title( "PlayerBase" )]
public partial class PlayerBase : Component, Component.INetworkSpawn, IPlayerBase
{
	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public CameraComponent Camera { get; set; }
	[Property] public CameraComponent ViewModelCamera { get; set; }

	public bool IsBot { get; set; }

	public IInventory Inventory { get; set; }
	public bool IsFirstPerson => cameraMovement.IsFirstPerson;
	public float InputSensitivity
	{
		get { return cameraMovement.InputSensitivity; }
		set { cameraMovement.InputSensitivity = value; }
	}
	public Angles EyeAnglesOffset
	{
		get { return cameraMovement.EyeAnglesOffset; }
		set { cameraMovement.EyeAnglesOffset = value; }
	}

	CameraMovement cameraMovement;

	protected override void OnAwake()
	{
		Inventory = new Inventory( this );
		cameraMovement = Components.GetInChildren<CameraMovement>();

		if ( IsBot ) return;

		OnMovementAwake();
	}

	public void OnNetworkSpawn( Connection connection )
	{
		ApplyClothes( connection );
	}

	protected override void OnStart()
	{
		if ( IsProxy || IsBot )
		{
			Camera.Enabled = false;
			ViewModelCamera.Enabled = false;
		}

		if ( IsBot )
		{
			var screenPanel = Components.GetInChildrenOrSelf<ScreenPanel>();
			screenPanel.Enabled = false;
		}

		if ( !IsProxy )
		{
			Respawn();
		}
	}

	public virtual void OnDeath( Shared.DamageInfo info )
	{

		CharacterController.Velocity = 0;
		Ragdoll( info.Force );
		Inventory.Clear();
		//Respawn();
	}

	public virtual void Respawn()
	{
		Unragdoll();
		Health = MaxHealth;

		var spawnLocation = GetSpawnLocation();
		Transform.Position = spawnLocation.Position;
		Transform.Rotation = spawnLocation.Rotation;

		// Give weapon
		if ( !IsBot )
		{
			var weaponRegistery = Scene.Components.GetInChildren<WeaponRegistry>();
			var weaponGO = weaponRegistery.Get( "swb_revolver" );
			var weapon = weaponGO.Components.Get<Weapon>( true );
			Inventory.AddClone( weaponGO, false );
			SetAmmo( weapon.Primary.AmmoType, 360 );

			weaponGO = weaponRegistery.Get( "swb_scarh" );
			weapon = weaponGO.Components.Get<Weapon>( true );
			Inventory.AddClone( weaponGO, false );
			SetAmmo( weapon.Primary.AmmoType, 360 );

			weaponGO = weaponRegistery.Get( "swb_remington" );
			weapon = weaponGO.Components.Get<Weapon>( true );
			Inventory.AddClone( weaponGO, false );
			SetAmmo( weapon.Primary.AmmoType, 360 );

			weaponGO = weaponRegistery.Get( "swb_colt" );
			weapon = weaponGO.Components.Get<Weapon>( true );
			Inventory.AddClone( weaponGO, false );
			SetAmmo( weapon.Primary.AmmoType, 360 );

			weaponGO = weaponRegistery.Get( "swb_l96a1" );
			weapon = weaponGO.Components.Get<Weapon>( true );
			Inventory.AddClone( weaponGO, true );
			SetAmmo( weapon.Primary.AmmoType, 360 );
		}
	}

	public virtual Transform GetSpawnLocation()
	{
		var spawnPoints = Scene.Components.GetAll<SpawnPoint>();

		if ( !spawnPoints.Any() )
			return new Transform();

		var rand = new Random();
		var randomSpawnPoint = spawnPoints.ElementAt( rand.Next( 0, spawnPoints.Count() - 1 ) );

		return randomSpawnPoint.Transform.World;
	}

	protected override void OnUpdate()
	{
		if ( IsBot ) return;
		if ( !IsProxy ) ViewModelCamera.Enabled = IsFirstPerson && IsAlive;

		if ( IsAlive )
			OnMovementUpdate();

		UpdateClothes();
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsAlive || IsBot ) return;
		OnMovementFixedUpdate();
	}
}
