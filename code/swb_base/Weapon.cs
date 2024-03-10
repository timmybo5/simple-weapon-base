using SWB.Shared;
using System.Linq;

namespace SWB.Base;

[Group( "SWB" )]
[Title( "Weapon" )]
public partial class Weapon : Component
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

	protected override void OnEnabled()
	{

	}

	protected override void OnDisabled()
	{
		if ( !IsProxy )
		{
			DestroyUI();
		}
	}

	protected override void OnStart()
	{
		Owner = Components.GetInAncestors<IPlayerBase>();

		if ( !IsProxy )
		{
			CreateUI();
		}

		CreateModels();
	}

	protected override void OnUpdate()
	{
		UpdateModels();

		Owner.AnimationHelper.HoldType = HoldType;

		if ( !IsProxy )
		{
			IsAiming = !Owner.IsRunning && AimAnimData != AngPos.Zero && Input.Down( InputButtonHelper.SecondaryAttack );

			if ( IsAiming )
				Owner.InputSensitivity = AimSensitivity;

			if ( CanPrimaryShoot() )
			{
				TimeSincePrimaryShoot = 0;
				Shoot( Primary, true );
			}
			else if ( CanSecondaryShoot() )
			{
				TimeSinceSecondaryShoot = 0;
				Shoot( Secondary, false );
			}
			else if ( Input.Down( InputButtonHelper.Reload ) )
			{
				Reload();
			}

			if ( IsReloading && TimeSinceReload >= 0 )
			{
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
		if ( !IsProxy && ViewModel is not null )
		{
			var viewModelGO = new GameObject( true, "Viewmodel" );
			viewModelGO.SetParent( Owner.GameObject );
			viewModelGO.Tags.Add( TagsHelper.ViewModel );

			ViewModelRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
			ViewModelRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
			ViewModelRenderer.Model = ViewModel;
			ViewModelRenderer.AnimationGraph = ViewModel.AnimGraph;

			ViewModelHandler = viewModelGO.Components.Create<ViewModelHandler>();
			ViewModelHandler.Weapon = this;
			ViewModelHandler.ViewModelRenderer = ViewModelRenderer;
			ViewModelHandler.Camera = Owner.ViewModelCamera;

			if ( ViewModelHands is not null )
			{
				ViewModelHandsRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
				ViewModelHandsRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
				ViewModelHandsRenderer.Model = ViewModelHands;
				ViewModelHandsRenderer.BoneMergeTarget = ViewModelRenderer;
			}

			ViewModelHandler.ViewModelHandsRenderer = ViewModelHandsRenderer;
		}

		if ( WorldModel is not null )
		{
			WorldModelRenderer = Components.Create<SkinnedModelRenderer>();
			WorldModelRenderer.Model = WorldModel;
			WorldModelRenderer.CreateBoneObjects = true;

			var bodyRenderer = Owner.Body.Components.Get<SkinnedModelRenderer>();
			var holdBone = bodyRenderer.Model.Bones.AllBones.FirstOrDefault( bone => bone.Name == "hold_R" );
			var holdBoneGO = bodyRenderer.GetBoneObject( holdBone );

			this.GameObject.SetParent( holdBoneGO );
			WorldModelRenderer.Transform.Position = holdBoneGO.Transform.Position;
			WorldModelRenderer.Transform.Rotation = holdBoneGO.Transform.Rotation;
		}
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
