using SWB.Base.Attachments;
using SWB.Shared;
using System.Collections.Generic;
using System.Linq;

namespace SWB.Base;

[Group( "SWB" )]
[Title( "Weapon" )]
public partial class Weapon : Component, IInventoryItem
{
	public IPlayerBase Owner { get; private set; }
	public ViewModelHandler ViewModelHandler { get; private set; }
	public PlayerCameraHandler CameraHandler { get; private set; }
	public SkinnedModelRenderer ViewModelRenderer { get; private set; }
	public SkinnedModelRenderer ViewModelHandsRenderer { get; private set; }
	public SkinnedModelRenderer WorldModelRenderer { get; private set; }
	public WeaponSettings Settings { get; private set; }
	public List<Attachment> Attachments = new();

	protected override void OnAwake()
	{
		Tags.Add( TagsHelper.Weapon );

		Attachments = Components.GetAll<Attachment>( FindMode.EverythingInSelf ).OrderBy( att => att.Name ).ToList();
		Settings = WeaponSettings.Instance;
		InitialPrimaryStats = StatsModifier.FromShootInfo( this, Primary );

		// Default BulletType
		if ( Primary is not null && Primary.BulletType is null )
			Primary.BulletType = Components.Create<HitScanBulletInfo>();
		if ( Secondary is not null && Secondary.BulletType is null )
			Secondary.BulletType = Components.Create<HitScanBulletInfo>();

		// Stats
		if ( Secondary is not null )
			InitialSecondaryStats = StatsModifier.FromShootInfo( this, Secondary );
		else
			InitialSecondaryStats = StatsModifier.Zero;

		// Hack: Hide weapon object until position is set when creating world model
		if ( !IsProxy )
		{
			WorldPosition = new( 0, 0, -999999 );
			Network.ClearInterpolation();
		}

		Owner = Components.GetInAncestors<IPlayerBase>( true );
		if ( !Owner.IsValid() )
		{
			Log.Error( $"{ClassName} cannot find owner, destroying!" );
			Destroy();
		}
	}

	protected override void OnDestroy()
	{
		ViewModelRenderer?.GameObject?.Destroy();
	}

	protected override void OnEnabled()
	{
		if ( IsProxy ) return;
		if ( ViewModelRenderer?.GameObject is not null )
			ViewModelRenderer.GameObject.Enabled = true;

		ClearState();

		if ( !Owner.IsBot )
			CreateUI();
	}

	protected override void OnDisabled()
	{
		if ( IsProxy ) return;
		if ( ViewModelRenderer?.GameObject is not null )
			ViewModelRenderer.GameObject.Enabled = false;

		if ( ViewModelHandler is not null )
			ViewModelHandler.ShouldDraw = false;

		// Attachments (VM + HUD)
		Attachments.ForEach( ( att ) =>
		{
			if ( att.IsValid() && att.Equipped )
			{
				if ( att.ViewModelRenderer.IsValid() )
					att.ViewModelRenderer.Enabled = false;

				if ( att.CreatedUI )
					att.DestroyHudElements();
			}
		} );

		ClearState();

		if ( Owner.IsValid() )
			Owner.HoldType = HoldTypes.None;

		DestroyUI();
	}

	protected virtual void ClearState()
	{
		IsReloading = false;
		IsScoping = false;
		IsAiming = false;
		IsCustomizing = false;
		SetScopeLensCenter( DefaultScopeLensCenter );
	}

	[Rpc.Broadcast]
	public virtual void OnCarryStart()
	{
		if ( !GameObject.IsValid() || !this.IsValid() ) return;
		GameObject.Enabled = true;
		TimeSinceDeployed = -999f;
	}

	[Rpc.Broadcast]
	public virtual void OnCarryStop()
	{
		if ( !GameObject.IsValid() || !this.IsValid() ) return;
		GameObject.Enabled = false;
	}

	public virtual bool CanCarryStop()
	{
		return Owner.IsBot || TimeSinceDeployed > 0;
	}

	public virtual (float delay, string anim) GetDrawInfo()
	{
		var delay = 0f;
		var anim = "";

		if ( Primary.Ammo == 0 && !string.IsNullOrEmpty( DrawEmptyAnim ) )
		{
			anim = DrawEmptyAnim;
			delay = DrawEmptyTime;
		}
		else if ( !string.IsNullOrEmpty( DrawAnim ) )
		{
			anim = DrawAnim;
			delay = DrawTime;
		}

		return (delay, anim);
	}

	public virtual void OnDeploy()
	{
		var drawInfo = GetDrawInfo();
		TimeSinceDeployed = -drawInfo.delay;

		// Sound
		if ( !IsProxy && DeploySound is not null )
			PlaySound( DeploySound );

		// Boltback
		if ( InBoltBack )
			AsyncBoltBack( drawInfo.delay );
	}

	public virtual void OnViewModelDeploy()
	{
		var drawInfo = GetDrawInfo();

		// Reset playback rate
		ViewModelRenderer?.PlaybackRate = 1;

		if ( !string.IsNullOrEmpty( drawInfo.anim ) )
			ViewModelRenderer?.Set( drawInfo.anim, true );

		// Start drawing (We delay by 1 frame to allow the animation to start first)
		async void ShouldDrawDelayed()
		{
			await GameTask.Delay( 100 );
			if ( ViewModelHandler.IsValid() )
			{
				ViewModelHandler.ShouldDraw = true;
				OnViewModelDrawn();
			}
		}
		ShouldDrawDelayed();
	}

	/// <summary>Called when the view model starts being drawn</summary>
	public virtual void OnViewModelDrawn() { }

	protected override void OnStart()
	{
		if ( !IsProxy && Owner.Camera is not null )
		{
			CameraHandler = Components.GetOrCreate<PlayerCameraHandler>();
			CameraHandler.Weapon = this;
		}

		CreateModels();

		// Attachments (enabled via property)
		if ( !IsProxy )
		{
			Attachments.ForEach( att =>
			{
				if ( att.Enable && !att.Equipped )
					att.EquipBroadCast();
			} );
		}

		// Attachments (load for clients joining late)
		if ( IsProxy )
		{
			// Log.Info( "Checking -> " + Network.Owner.DisplayName + "'s " + DisplayName + " for attachments" );
			Attachments.ForEach( att =>
			{
				// Log.Info( "[" + att.Name + "] equipped ->" + att.Equipped );
				if ( att is not null && att.Equipped )
					att.Equip();
			} );
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsProxy && !IsDeploying && Owner.IsValid() && !Owner.IsBot && TuckRange != -1 )
		{
			ShouldTuckVar = ShouldTuck( out TuckDist );
		}
	}

	protected override void OnUpdate()
	{
		if ( !Owner.IsValid() ) return;

		UpdateModels();
		Owner.HoldType = HoldType;

		if ( !IsProxy && !Owner.IsBot && !IsDeploying )
		{
			// Customization
			if ( WeaponSettings.Instance.Customization && !IsScoping && !IsAiming && Input.Pressed( InputButtonHelper.Menu ) && Attachments.Count > 0 )
			{
				if ( !IsCustomizing )
					OpenCustomizationMenu();
				else
					CloseCustomizationMenu();

				IsCustomizing = !IsCustomizing;
			}

			// Don't cancel reload when customizing
			if ( IsCustomizing && !IsReloading ) return;

			if ( IsRunning )
				TimeSinceRunning = 0;

			var wasAiming = IsAiming;
			IsAiming = !Owner.IsRunning && AimAnimData != AngPos.Zero && Input.Down( InputButtonHelper.SecondaryAttack ) && !ShouldTuckVar;

			if ( wasAiming != IsAiming )
			{
				if ( IsAiming )
					OnAimStart();
				else
					OnAimStop();
			}

			if ( IsScoping )
				Owner.InputSensitivity = ScopeInfo.Sensitivity;
			else if ( IsAiming )
				Owner.InputSensitivity = AimInfo.Sensitivity;
			else
				Owner.InputSensitivity = 1f;

			OnAimAssistUpdate();

			if ( IsAiming )
				OnAimUpdate();

			if ( Scoping )
			{
				if ( IsAiming && !IsScoping )
					OnScopeStart();
				else if ( !IsAiming && IsScoping )
					OnScopeEnd();
			}

			ResetBurstFireCount( Primary, InputButtonHelper.PrimaryAttack );
			ResetBurstFireCount( Secondary, InputButtonHelper.SecondaryAttack );
			BarrelHeatCheck();

			if ( CanPrimaryShoot() && !ShouldTuckVar )
			{
				if ( IsReloading && ShellReloading && ShellReloadingShootCancel )
					CancelShellReload();

				TimeSincePrimaryShoot = 0;
				Shoot( Primary, true );
			}
			else if ( CanSecondaryShoot() && !ShouldTuckVar )
			{
				TimeSinceSecondaryShoot = 0;
				Shoot( Secondary, false );
			}
			else if ( Input.Down( InputButtonHelper.Reload ) )
			{
				if ( ShellReloading )
					OnShellReload();
				else
					Reload();
			}

			if ( IsReloading && TimeSinceReload >= 0 )
			{
				if ( ShellReloading )
					OnShellReloadFinish();
				else
					OnReloadFinish();
			}
		}
	}

	protected virtual void UpdateModels()
	{
		// Should draw after deploy
		if ( (IsProxy || Owner.IsBot) && WorldModelRenderer is not null )
		{
			if ( WorldModelRenderer.RenderType != ModelRenderer.ShadowRenderType.On )
				WorldModelRenderer.RenderType = ModelRenderer.ShadowRenderType.On;

			if ( !WorldModelRenderer.RenderOptions.Game )
				WorldModelRenderer.RenderOptions.Game = true;
		}

		if ( !IsProxy && !Owner.IsBot && WorldModelRenderer is not null )
		{
			var worldModelRenderType = Owner.IsFirstPerson ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;

			if ( WorldModelRenderer.RenderType != worldModelRenderType )
				WorldModelRenderer.RenderType = worldModelRenderType;

			// Should draw after deploy
			if ( !Owner.IsFirstPerson && !WorldModelRenderer.RenderOptions.Game )
				WorldModelRenderer.RenderOptions.Game = true;

			// Attachments
			UpdateAttachments(worldModelRenderType);
		}
	}

	protected virtual void UpdateAttachments(ModelRenderer.ShadowRenderType worldModelRenderType)
	{
		Attachments.ForEach( ( att ) =>
		{
			if ( !att.Equipped ) return;

			if ( att.ViewModelRenderer.IsValid() )
				att.ViewModelRenderer.Enabled = Owner.IsFirstPerson && ViewModelHandler.ShouldDraw;

			if ( att.WorldModelRenderer.IsValid() && att.WorldModelRenderer.RenderType != worldModelRenderType )
				att.WorldModelRenderer.RenderType = worldModelRenderType;
		} );
	}

	/// <summary>Override to use a custom ViewModelHandler</summary>
	protected virtual ViewModelHandler CreateViewModelHandler( GameObject go )
	{
		return go.Components.Create<ViewModelHandler>();
	}

	protected virtual ViewModel CreateViewModel( Model model, bool createHandler = true )
	{
		var viewModelGO = new GameObject( true, "Viewmodel - " + ClassName );
		viewModelGO.SetParent( Owner.GameObject, false );
		viewModelGO.Tags.Add( TagsHelper.ViewModel );
		viewModelGO.NetworkMode = NetworkMode.Never;

		var viewModelRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
		viewModelRenderer.Model = model;
		viewModelRenderer.AnimationGraph = model.AnimGraph;
		viewModelRenderer.CreateBoneObjects = true;
		viewModelRenderer.CreateAttachments = true;
		viewModelRenderer.Enabled = false;
		viewModelRenderer.OnSoundEvent += ( sceneSound ) =>
		{
			var soundEvent = ResourceLibrary.Get<SoundEvent>( sceneSound.Name );
			if ( soundEvent is null ) return;

			using ( Rpc.FilterExclude( Owner.GameObject.Network.Owner ) )
			{
				PlaySound( soundEvent, 0.5f, 7500f, true );
			}
		};
		viewModelRenderer.OnComponentEnabled += async () =>
		{
			// Prevent flickering when enabling the component, this is controlled by the ViewModelHandler
			viewModelRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
			viewModelRenderer.ClearParameters();
			OnViewModelDeploy();

			// Deploy
			if ( WorldModel is null )
			{
				await GameTask.DelayRealtime( 1 );
				if ( this.IsValid() )
					OnDeploy();
			}
		};

		var viewModelCamera = Owner.ViewModelCamera;
		if ( Owner.ViewModelCamera is null )
		{
			var viewModelCameraGameObject = new GameObject();
			viewModelCameraGameObject.Name = "ViewModelCamera";
			viewModelCameraGameObject.SetParent( Owner.GameObject, false );

			// Setup the view model camera
			viewModelCamera = viewModelCameraGameObject.Components.Create<CameraComponent>();
			viewModelCamera.ClearFlags = ClearFlags.Depth | ClearFlags.Stencil;
			viewModelCamera.ZNear = 1;
			viewModelCamera.Priority = 2;
			viewModelCamera.TargetEye = StereoTargetEye.RightEye;
			viewModelCamera.RenderTags.Add( new TagSet() { TagsHelper.ViewModel, TagsHelper.Light } );

			Owner.ViewModelCamera = viewModelCamera;
		}

		Owner.Camera.RenderExcludeTags.Add( TagsHelper.ViewModel );

		SkinnedModelRenderer viewModelHandsRenderer = null;

		if ( ViewModelHands is not null )
		{
			viewModelHandsRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
			viewModelHandsRenderer.Model = ViewModelHands;
			viewModelHandsRenderer.BoneMergeTarget = viewModelRenderer;
			viewModelHandsRenderer.OnComponentEnabled += () =>
			{
				// Prevent flickering when enabling the component, this is controlled by the ViewModelHandler
				viewModelHandsRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
			};
		}

		ViewModelHandler handler = null;

		if ( createHandler )
		{
			handler = CreateViewModelHandler( viewModelGO );
			handler.Weapon = this;
			handler.ViewModelRenderer = viewModelRenderer;
			handler.Camera = viewModelCamera;
			handler.ViewModelHandsRenderer = viewModelHandsRenderer;
		}

		return new()
		{
			Renderer = viewModelRenderer,
			HandsRenderer = viewModelHandsRenderer,
			ModelHandler = handler
		};
	}

	protected virtual void CreateModels()
	{
		if ( !IsProxy && !Owner.IsBot && ViewModel.IsValid() && !ViewModelRenderer.IsValid() )
		{
			var viewmodel = CreateViewModel( ViewModel );
			ViewModelRenderer = viewmodel.Renderer;
			ViewModelHandler = viewmodel.ModelHandler;
		}

		if ( WorldModel is not null && WorldModelRenderer is null )
		{
			WorldModelRenderer = Components.Create<SkinnedModelRenderer>();
			WorldModelRenderer.Model = WorldModel;
			WorldModelRenderer.AnimationGraph = WorldModel.AnimGraph;
			WorldModelRenderer.CreateBoneObjects = true;
			WorldModelRenderer.CreateAttachments = true;

			async void OnComponentEnabled()
			{
				// Prevent flickering when enabling the component
				WorldModelRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
				WorldModelRenderer.RenderOptions.Game = false;

				// Deploy
				await GameTask.DelayRealtime( 1 );
				if ( this.IsValid() )
					OnDeploy();
			}

			WorldModelRenderer.OnComponentEnabled += () =>
			{
				// Called after weapon has been switched already
				OnComponentEnabled();
			};

			// Called when weapon models are created
			OnComponentEnabled();
			Owner.ParentToBone( GameObject, "hold_R", deleteOnFail: false );
		}
	}

	[Rpc.Broadcast]
	public void PlaySound( SoundEvent sound, float volume = float.NaN, float distance = float.NaN, bool shouldFollow = false )
	{
		if ( sound is null || !this.IsValid() ) return;

		if ( !shouldFollow )
			shouldFollow = CanSeeViewModel;

		if ( !float.IsNaN( volume ) )
			sound.Volume = volume;

		if ( !float.IsNaN( distance ) )
			sound.Distance = distance;

		var handle = Sound.Play( sound, WorldPosition );
		if ( shouldFollow )
		{
			handle?.Parent = this.GameObject;
			handle?.FollowParent = true;
		}
		else
		{
			handle?.Position = WorldPosition;
		}
	}

	[Rpc.Broadcast]
	public void PlayWorldSound( string eventName, float volume = 1, float distance = 7500 )
	{
		var handle = Sound.Play( eventName, WorldPosition );
		handle.Volume = volume;
		handle.Distance = distance;
	}
}
