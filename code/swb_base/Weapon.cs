﻿using SWB.Base.Attachments;
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
		InitialPrimaryStats = StatsModifier.FromShootInfo( Primary );

		if ( Secondary is not null )
			InitialSecondaryStats = StatsModifier.FromShootInfo( Secondary );
		else
			InitialSecondaryStats = StatsModifier.Zero;

		// Hack: Hide weapon object until position is set when creating world model
		if ( !IsProxy )
		{
			WorldPosition = new( 0, 0, -999999 );
			Network.ClearInterpolation();
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
			if ( att.Equipped )
			{
				if ( att.ViewModelRenderer is not null )
					att.ViewModelRenderer.Enabled = false;

				if ( att.CreatedUI )
					att.DestroyHudElements();
			}
		} );

		IsReloading = false;
		IsScoping = false;
		IsAiming = false;
		IsCustomizing = false;

		DestroyUI();
	}

	[Rpc.Broadcast]
	public virtual void OnCarryStart()
	{
		if ( !IsValid ) return;
		GameObject.Enabled = true;
	}

	[Rpc.Broadcast]
	public virtual void OnCarryStop()
	{
		if ( !IsValid ) return;
		GameObject.Enabled = false;
	}

	public virtual bool CanCarryStop()
	{
		return TimeSinceDeployed > 0;
	}

	public virtual void OnDeploy()
	{
		var delay = 0f;

		if ( Primary.Ammo == 0 && !string.IsNullOrEmpty( DrawEmptyAnim ) )
		{
			ViewModelRenderer?.Set( DrawEmptyAnim, true );
			delay = DrawEmptyTime;
		}
		else if ( !string.IsNullOrEmpty( DrawAnim ) )
		{
			ViewModelRenderer?.Set( DrawAnim, true );
			delay = DrawTime;
		}

		TimeSinceDeployed = -delay;

		// Sound
		if ( DeploySound is not null )
			PlaySound( DeploySound.ResourceId );

		// Start drawing (We delay by 1 frame to allow the animation to start first)
		async void ShouldDrawDelayed()
		{
			await GameTask.Delay( 1 );
			if ( ViewModelHandler.IsValid() )
				ViewModelHandler.ShouldDraw = true;
		}
		ShouldDrawDelayed();

		// Boltback
		if ( InBoltBack )
			AsyncBoltBack( delay );
	}

	protected override void OnStart()
	{
		Owner = Components.GetInAncestors<IPlayerBase>( true );
		if ( !Owner.IsValid() )
		{
			Log.Error( $"{ClassName} cannot find owner, destroying!" );
			Destroy();
			return;
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

	protected override void OnUpdate()
	{
		if ( Owner is null ) return;

		UpdateModels();
		Owner.AnimationHelper.HoldType = HoldType;

		if ( !IsProxy )
		{
			if ( IsDeploying ) return;

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

			IsAiming = !Owner.IsRunning && AimAnimData != AngPos.Zero && Input.Down( InputButtonHelper.SecondaryAttack );

			if ( IsScoping )
				Owner.InputSensitivity = ScopeInfo.AimSensitivity;
			else if ( IsAiming )
				Owner.InputSensitivity = AimSensitivity;

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

			var shouldTuck = ShouldTuck();

			if ( CanPrimaryShoot() && !shouldTuck )
			{
				if ( IsReloading && ShellReloading && ShellReloadingShootCancel )
					CancelShellReload();

				TimeSincePrimaryShoot = 0;
				Shoot( Primary, true );
			}
			else if ( CanSecondaryShoot() && !shouldTuck )
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

	void UpdateModels()
	{
		if ( !IsProxy && WorldModelRenderer is not null )
		{
			var worldModelRenderType = Owner.IsFirstPerson ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
			WorldModelRenderer.RenderType = worldModelRenderType;

			// Attachments
			Attachments.ForEach( ( att ) =>
			{
				if ( !att.Equipped ) return;

				if ( att.ViewModelRenderer is not null )
					att.ViewModelRenderer.Enabled = Owner.IsFirstPerson && ViewModelHandler.ShouldDraw;

				if ( att.WorldModelRenderer is not null )
					att.WorldModelRenderer.RenderType = worldModelRenderType;
			} );
		}
	}

	void CreateModels()
	{
		if ( !IsProxy && ViewModel is not null && ViewModelRenderer is null )
		{
			var viewModelGO = new GameObject( true, "Viewmodel" );
			viewModelGO.SetParent( Owner.GameObject, false );
			viewModelGO.Tags.Add( TagsHelper.ViewModel );
			viewModelGO.Flags |= GameObjectFlags.NotNetworked;

			ViewModelRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
			ViewModelRenderer.Model = ViewModel;
			ViewModelRenderer.AnimationGraph = ViewModel.AnimGraph;
			ViewModelRenderer.CreateBoneObjects = true;
			ViewModelRenderer.Enabled = false;
			ViewModelRenderer.OnComponentEnabled += () =>
			{
				// Prevent flickering when enabling the component, this is controlled by the ViewModelHandler
				ViewModelRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
				ResetViewModelAnimations();
				OnDeploy();
			};

			ViewModelHandler = viewModelGO.Components.Create<ViewModelHandler>();
			ViewModelHandler.Weapon = this;
			ViewModelHandler.ViewModelRenderer = ViewModelRenderer;
			ViewModelHandler.Camera = Owner.ViewModelCamera;

			if ( ViewModelHands is not null )
			{
				ViewModelHandsRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
				ViewModelHandsRenderer.Model = ViewModelHands;
				ViewModelHandsRenderer.BoneMergeTarget = ViewModelRenderer;
				ViewModelHandsRenderer.OnComponentEnabled += () =>
				{
					// Prevent flickering when enabling the component, this is controlled by the ViewModelHandler
					ViewModelHandsRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
				};
			}

			ViewModelHandler.ViewModelHandsRenderer = ViewModelHandsRenderer;
		}

		if ( WorldModel is not null && WorldModelRenderer is null )
		{
			WorldModelRenderer = Components.Create<SkinnedModelRenderer>();
			WorldModelRenderer.Model = WorldModel;
			WorldModelRenderer.AnimationGraph = WorldModel.AnimGraph;
			WorldModelRenderer.CreateBoneObjects = true;

			var bodyRenderer = Owner.Body.Components.Get<SkinnedModelRenderer>();
			ModelUtil.ParentToBone( GameObject, bodyRenderer, "hold_R" );
		}
	}

	// Temp fix until https://github.com/Facepunch/sbox-issues/issues/5247 is fixed
	public virtual void ResetViewModelAnimations()
	{
		if ( Primary.IsValid() )
		{
			ViewModelRenderer?.Set( Primary.ShootAnim ?? "", false );
			ViewModelRenderer?.Set( Primary.ShootEmptyAnim ?? "", false );
			ViewModelRenderer?.Set( Primary.ShootAimedAnim ?? "", false );
		}

		if ( Secondary.IsValid() )
		{
			ViewModelRenderer?.Set( Secondary.ShootAnim ?? "", false );
			ViewModelRenderer?.Set( Secondary.ShootEmptyAnim ?? "", false );
			ViewModelRenderer?.Set( Secondary.ShootAimedAnim ?? "", false );
		}

		ViewModelRenderer?.Set( ReloadAnim ?? "", false );
		ViewModelRenderer?.Set( ReloadEmptyAnim ?? "", false );
		ViewModelRenderer?.Set( DrawAnim ?? "", false );
		ViewModelRenderer?.Set( DrawEmptyAnim ?? "", false );
	}

	[Rpc.Broadcast]
	public void PlaySound( int resourceID )
	{
		if ( !IsValid ) return;

		var sound = ResourceLibrary.Get<SoundEvent>( resourceID );
		if ( sound is null ) return;

		var isScreenSound = CanSeeViewModel;
		sound.UI = isScreenSound;

		if ( isScreenSound )
		{
			Sound.Play( sound );
		}
		else
		{
			Sound.Play( sound, WorldPosition );
		}
	}
}
