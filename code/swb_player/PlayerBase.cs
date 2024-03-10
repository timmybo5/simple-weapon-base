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

	public IInventory Inventory { get; set; }
	public bool IsFirstPerson => cameraMovement.IsFirstPerson;
	public float InputSensitivity
	{
		get { return cameraMovement.InputSensitivity; }
		set { cameraMovement.InputSensitivity = value; }
	}

	CameraMovement cameraMovement;

	protected override void OnAwake()
	{
		Inventory = new Inventory( this );
		cameraMovement = Components.GetInChildren<CameraMovement>();

		OnMovementAwake();
	}

	public void OnNetworkSpawn( Connection connection )
	{
		ApplyClothes( connection );

		if ( !IsProxy )
		{
			Respawn();
		}
	}

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			Camera.Enabled = false;
			ViewModelCamera.Enabled = false;
		}

		if ( !IsProxy )
		{
			var weaponRegistery = Scene.Components.GetInChildren<WeaponRegistry>();
			var weaponObj = weaponRegistery.Get( "swb_testweapon" );
			var weapon = weaponObj.Components.Get<Weapon>( true );
			Inventory.AddClone( weaponObj );

			SetAmmo( weapon.Primary.AmmoType, 360 );
		}
	}

	public virtual void Respawn()
	{
		Health = MaxHealth;

		var spawnLocation = GetSpawnLocation();
		Transform.Position = spawnLocation.Position;
		Transform.Rotation = spawnLocation.Rotation;
	}

	public virtual Transform GetSpawnLocation()
	{
		var spawnPoints = Scene.Components.GetAll<SpawnPoint>();
		var rand = new Random();
		var randomSpawnPoint = spawnPoints.ElementAt( rand.Next( 0, spawnPoints.Count() - 1 ) );

		if ( randomSpawnPoint is not null )
			return randomSpawnPoint.Transform.World;

		return new Transform();
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy ) ViewModelCamera.Enabled = IsFirstPerson;

		OnMovementUpdate();
		UpdateClothes();
	}

	protected override void OnFixedUpdate()
	{
		OnMovementFixedUpdate();
	}


}
