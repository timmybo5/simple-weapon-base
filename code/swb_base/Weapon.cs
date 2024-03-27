using SWB.Shared;
using System.Linq;

namespace SWB.Base;

[Group( "SWB" )]
[Title( "Weapon" )]
public partial class Weapon : Component, IInventoryItem
{
	public IPlayerBase Owner { get; set; }
	public ViewModelHandler ViewModelHandler { get; set; }
	public SkinnedModelRenderer ViewModelRenderer { get; private set; }
	public SkinnedModelRenderer ViewModelHandsRenderer { get; private set; }
	public SkinnedModelRenderer WorldModelRenderer { get; private set; }

	protected override void OnAwake()
	{
		Tags.Add( TagsHelper.Weapon );
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

		IsReloading = false;
		IsScoping = false;
		IsAiming = false;

		DestroyUI();
	}

	[Broadcast]
	public void OnCarryStart()
	{
		GameObject.Enabled = true;
	}

	[Broadcast]
	public void OnCarryStop()
	{
		GameObject.Enabled = false;
	}

	public bool CanCarryStop()
	{
		return TimeSinceDeployed > 0;
	}

	public void OnDeploy()
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

		// Start drawing
		ViewModelHandler.ShouldDraw = true;

		// Boltback
		if ( InBoltBack )
			AsyncBoltBack( delay );
	}

	protected override void OnStart()
	{
		Owner = Components.GetInAncestors<IPlayerBase>();

		CreateModels();
	}

	protected override void OnUpdate()
	{
		UpdateModels();
		Owner.AnimationHelper.HoldType = HoldType;

		if ( !IsProxy )
		{
			if ( IsDeploying ) return;

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
			WorldModelRenderer.RenderType = Owner.IsFirstPerson ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
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
			var holdBone = bodyRenderer.Model.Bones.AllBones.FirstOrDefault( bone => bone.Name == "hold_R" );
			var holdBoneGO = bodyRenderer.GetBoneObject( holdBone );

			this.GameObject.SetParent( holdBoneGO );
			WorldModelRenderer.Transform.Position = holdBoneGO.Transform.Position;
			WorldModelRenderer.Transform.Rotation = holdBoneGO.Transform.Rotation;
		}
	}

	// Temp fix until https://github.com/Facepunch/sbox-issues/issues/5247 is fixed
	void ResetViewModelAnimations()
	{
		ViewModelRenderer?.Set( Primary.ShootAnim, false );
		ViewModelRenderer?.Set( Primary.ShootEmptyAnim, false );
		ViewModelRenderer?.Set( Primary.ShootAimedAnim, false );

		if ( Secondary is not null )
		{
			ViewModelRenderer?.Set( Secondary.ShootAnim, false );
			ViewModelRenderer?.Set( Secondary.ShootEmptyAnim, false );
			ViewModelRenderer?.Set( Secondary.ShootAimedAnim, false );
		}

		ViewModelRenderer?.Set( ReloadAnim, false );
		ViewModelRenderer?.Set( ReloadEmptyAnim, false );
		ViewModelRenderer?.Set( DrawAnim, false );
		ViewModelRenderer?.Set( DrawEmptyAnim, false );
	}

	[Broadcast]
	void PlaySound( int resourceID )
	{
		var sound = ResourceLibrary.Get<SoundEvent>( resourceID );
		var isScreenSound = CanSeeViewModel;
		sound.UI = isScreenSound;

		if ( isScreenSound )
		{
			Sound.Play( sound );
		}
		else
		{
			Sound.Play( sound, Transform.Position );
		}
	}
}
