using SWB.Shared;
using System;
using System.Collections.Generic;
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
	[Property] public PanelComponent RootDisplay { get; set; }

	[Sync] public bool IsBot { get; set; }
	public IInventory Inventory { get; set; }
	public bool IsFirstPerson => cameraMovement.IsFirstPerson;
	public string DisplayName => !IsBot ? (Network.OwnerConnection?.DisplayName ?? "Disconnected") : GameObject.Name;
	public ulong SteamId => !IsBot ? Network.OwnerConnection.SteamId : 0;
	public bool IsHost => !IsBot && Network.OwnerConnection.IsHost;
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

	Guid IPlayerBase.Id { get => GameObject.Id; }
	CameraMovement cameraMovement;

	protected override void OnAwake()
	{
		Inventory = new Inventory( this );
		cameraMovement = Components.GetInChildren<CameraMovement>();

		if ( IsBot ) return;

		// Hide client until fully loaded in OnStart
		if ( !IsProxy )
		{
			Transform.Position = new( 0, 0, -999999 );
			Network.ClearInterpolation();
		}

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
			if ( Camera is not null )
				Camera.Enabled = false;

			if ( ViewModelCamera is not null )
				ViewModelCamera.Enabled = false;
		}

		if ( IsBot )
		{
			var screenPanel = Components.GetInChildrenOrSelf<ScreenPanel>();

			if ( screenPanel is not null )
				screenPanel.Enabled = false;
		}

		if ( !IsProxy )
			Respawn();
	}

	[Broadcast]
	public virtual void OnDeath( Shared.DamageInfo info )
	{
		if ( !IsValid ) return;
		var attackerGO = Scene.Directory.FindByGuid( info.AttackerId );

		if ( attackerGO is not null && !attackerGO.IsProxy )
		{
			var attacker = attackerGO.Components.Get<PlayerBase>();

			if ( attacker is not null )
				attacker.Kills += 1;
		}

		if ( IsProxy ) return;

		Deaths += 1;
		CharacterController.Velocity = 0;
		Ragdoll( info.Force, info.Origin );
		Inventory.Clear();
		RespawnWithDelay( 2 );
	}

	public async void RespawnWithDelay( float delay )
	{
		await GameTask.DelaySeconds( delay );
		Respawn();
	}

	public virtual void Respawn()
	{
		Unragdoll();
		Health = MaxHealth;

		var spawnLocation = GetSpawnLocation();
		Transform.Position = spawnLocation.Position;
		EyeAngles = spawnLocation.Rotation.Angles();
		Network.ClearInterpolation();

		if ( IsBot )
		{
			Body.Transform.Rotation = new Angles( 0, EyeAngles.ToRotation().Yaw(), 0 ).ToRotation();
			AnimationHelper.WithLook( EyeAngles.ToRotation().Forward, 1f, 0.75f, 0.5f );
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

		HandleFlinch();
		HandleScreenShake();
		UpdateClothes();
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsAlive || IsBot ) return;
		OnMovementFixedUpdate();
	}

	public static PlayerBase GetLocal()
	{
		var players = GetAll();
		return players.First( ( player ) => !player.IsProxy && !player.IsBot );
	}

	public static IEnumerable<PlayerBase> GetAll()
	{
		return Game.ActiveScene.GetAllComponents<PlayerBase>();
	}
}
