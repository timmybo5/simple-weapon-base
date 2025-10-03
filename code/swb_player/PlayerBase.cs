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
	[Property] public Voice Voice { get; set; }

	[Sync] public bool IsBot { get; set; }
	public IInventory Inventory { get; set; }
	public ICameraMovement CameraMovement { get; set; }
	public bool IsFirstPerson => CameraMovement.IsFirstPerson;
	public string DisplayName => !IsBot ? (Network.Owner?.DisplayName ?? "Disconnected") : GameObject.Name;
	public SteamId SteamId => !IsBot ? Network.Owner.SteamId : new( 0 );
	public bool IsHost => !IsBot && Network.Owner.IsHost;
	public bool IsSpeaking => Voice.Amplitude > 0;

	public float InputSensitivity
	{
		get { return CameraMovement.InputSensitivity; }
		set { CameraMovement.InputSensitivity = value; }
	}

	public float FieldOfView
	{
		set
		{
			Camera.FieldOfView = value;
		}
	}

	Guid IPlayerBase.Id { get => GameObject.Id; }

	protected override void OnAwake()
	{
		Inventory = Components.Create<Inventory>();
		CameraMovement = Components.GetInChildren<CameraMovement>();

		if ( IsBot ) return;

		// Hack: Hide client until fully loaded in OnStart
		if ( !IsProxy )
		{
			WorldPosition = new( 0, 0, -999999 );
			Network.ClearInterpolation();
		}

		OnMovementAwake();
	}

	public virtual void OnNetworkSpawn( Connection connection ) { }

	protected override void OnStart()
	{
		ApplyClothes();

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

	[Rpc.Owner]
	public void Kill()
	{
		if ( !IsAlive ) return;
		Health = 0;
		OnDeath( new() { Attacker = GameObject } );
	}

	[Rpc.Owner]
	public void Kick( string reason )
	{
		Log.Info( reason );
		Game.Disconnect();
	}

	[Rpc.Broadcast]
	public virtual void OnDeath( Shared.DamageInfo info )
	{
		if ( !IsValid ) return;
		var attackerGO = info.Attacker;

		if ( attackerGO is not null && !attackerGO.IsProxy )
		{
			var attacker = attackerGO.Components.Get<PlayerBase>();

			if ( attacker is not null && attacker != this )
				attacker.Kills++;
		}

		if ( IsProxy ) return;

		Deaths++;
		Ragdoll( info.Force, info.Origin, CharacterController.Velocity );
		CharacterController.Velocity = 0;
		Inventory.Clear();
		RespawnWithDelay( 2 );
	}

	public async virtual void RespawnWithDelay( float delay )
	{
		await GameTask.DelaySeconds( delay );
		Respawn();
	}

	[Rpc.Broadcast]
	public void RespawnWithDelayBroadCast( float delay )
	{
		RespawnWithDelay( delay );
	}

	[Rpc.Broadcast]
	public void RespawnBroadCast()
	{
		Respawn();
	}

	public virtual void Respawn()
	{
		Inventory.Clear();
		Health = MaxHealth;

		var spawnLocation = GetSpawnLocation();
		WorldPosition = spawnLocation.Position;
		EyeAngles = spawnLocation.Rotation.Angles();
		Network.ClearInterpolation();
		Unragdoll();

		if ( IsBot )
		{
			Body.WorldRotation = new Angles( 0, EyeAngles.ToRotation().Yaw(), 0 ).ToRotation();
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
		if ( IsBot )
		{
			UpdateClothes();
			return;
		}

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

	public void TriggerAnimation( Animations animation )
	{
		string animationName = animation switch
		{
			Animations.Attack => "b_attack",
			Animations.Reload => "b_reload",
			_ => ""
		};

		if ( animationName == "" ) return;
		BodyRenderer.Set( animationName, true );
	}

	public void ApplyRecoilOffset( Angles recoilOffset )
	{
		CameraMovement.EyeAnglesOffset += recoilOffset;
	}

	public void ParentToBone( GameObject weaponObject, string boneName )
	{
		ModelUtil.ParentToBone( weaponObject, BodyRenderer, boneName );
	}

	public static PlayerBase GetLocal()
	{
		var players = GetAll();
		return players.FirstOrDefault( ( player ) => !player.IsProxy && !player.IsBot, null );
	}

	public static IEnumerable<PlayerBase> GetAll()
	{
		return Game.ActiveScene.GetAllComponents<PlayerBase>();
	}
}
