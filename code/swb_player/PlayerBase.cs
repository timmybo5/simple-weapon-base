using SWB.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SWB.Player;

[Group( "SWB" )]
[Title( "PlayerBase" )]
public partial class PlayerBase : Component, Component.INetworkSpawn, IPlayerBase
{
	public static PlayerBase Local { get; private set; }
	public static List<PlayerBase> All { get; private set; } = new();

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
	public bool IsFirstPerson => !IsBot ? CameraMovement.IsFirstPerson : false;
	public string DisplayName => !IsBot ? (Network.Owner?.DisplayName ?? "Disconnected") : GameObject.Name;
	public SteamId SteamId => !IsBot ? Network.Owner.SteamId : new( 0 );
	public bool IsHost => !IsBot && Network.Owner.IsHost;
	public bool IsSpeaking => !IsBot ? Voice.Amplitude > 0 : false;
	bool taggedLights;

	public float InputSensitivity
	{
		get { return CameraMovement.InputSensitivity; }
		set { CameraMovement.InputSensitivity = value; }
	}

	public float FieldOfView
	{
		get
		{
			return Camera.FieldOfView;
		}
		set
		{
			Camera.FieldOfView = value;
		}
	}

	Guid IPlayerBase.Id { get => GameObject.Id; }

	protected override void OnAwake()
	{
		All.Add( this );
		Inventory = Components.Create<Inventory>();
		CameraMovement = Components.GetInChildren<CameraMovement>();
		OnMovementAwake();

		// Reset body animations, only works here
		BodyRenderer.OnComponentDisabled += () =>
		{
			BodyRenderer.ClearParameters();
		};

		// Hack: Hide client until fully loaded in OnStart
		WorldPosition = new( 0, 0, -999999 );
		Network.ClearInterpolation();
	}

	public virtual void OnNetworkSpawn( Connection connection ) { }

	protected override void OnDestroy()
	{
		if ( RagdollGO.IsValid() )
			RagdollGO.Destroy();

		if ( Local == this )
			Local = null;

		All.Remove( this );
	}

	protected override void OnStart()
	{
		if ( !IsProxy && !IsBot )
		{
			Local = this;
			OnInputDeviceSwitch();
		}

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

	public async virtual void RespawnWithDelay( float delay, Transform? respawnAt = null )
	{
		await GameTask.DelaySeconds( delay );
		Respawn( respawnAt );
	}

	[Rpc.Broadcast]
	public void RespawnWithDelayBroadCast( float delay, Transform? respawnAt = null )
	{
		RespawnWithDelay( delay, respawnAt );
	}

	[Rpc.Broadcast]
	public void RespawnBroadCast( Transform? respawnAt = null )
	{
		Respawn( respawnAt );
	}

	public virtual void Respawn( Transform? respawnAt = null )
	{
		// Only works when player has spawned
		if ( !taggedLights && !IsBot )
		{
			taggedLights = true;
			MapUtil.TagLights();
			OverlayUIWorkaround();
		}

		if ( IsUsingController )
			stickyActiveButtons.Clear();

		Ammo.Clear();
		Inventory.Clear();
		Health = MaxHealth;

		var spawnPos = Vector3.Zero;
		var spawnRot = Rotation.Identity;

		if ( respawnAt.HasValue )
		{
			spawnPos = respawnAt.Value.Position;
			spawnRot = respawnAt.Value.Rotation.Angles();
		}
		else
		{
			var spawnLocation = GetSpawnLocation();
			spawnPos = spawnLocation.Position;
			spawnRot = spawnLocation.Rotation;
		}

		WorldPosition = spawnPos;
		if ( IsFirstPerson || IsBot )
			EyeAngles = spawnRot.Angles();
		else
			Camera.WorldRotation = spawnRot;

		Network.ClearInterpolation();
		Unragdoll();
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

	async void OverlayUIWorkaround()
	{
		Camera.IsMainCamera = false;
		await GameTask.Delay( 1000 );
		if ( Camera.IsValid() )
			Camera.IsMainCamera = true;
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy && !IsBot )
		{
			ViewModelCamera.Enabled = IsFirstPerson && IsAlive;
			HandleFlinch();
			HandleScreenShake();
		}

		if ( IsAlive )
			OnMovementUpdate();

		UpdateClothes();
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsAlive ) return;
		HandleMovementImpacts();
		OnMovementFixedUpdate();

		if ( !IsProxy && !IsBot && IsUsingController != Input.UsingController )
		{
			OnInputDeviceSwitch();
		}
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

	public void ApplyEyeAnglesOffset( Angles offset )
	{
		CameraMovement?.EyeAnglesOffset += offset;
	}

	public void ParentToBone( GameObject weaponObject, string boneName )
	{
		ModelUtil.ParentToBone( weaponObject, BodyRenderer, boneName );
	}
}
