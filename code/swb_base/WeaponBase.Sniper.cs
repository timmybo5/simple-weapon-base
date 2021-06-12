using Sandbox;

/* 
 * Weapon base for weapons using shell based reloading 
*/

namespace SWB_Base
{
	public partial class WeaponBaseSniper : WeaponBase
	{
		public virtual string LensTexture => "/swb_base/textures/scopes/swb_lens_hunter.png";
		public virtual string ScopeTexture => "/swb_base/textures/scopes/swb_scope_hunter.png";
		public virtual float ZoomAmount => 20f;

		private SniperScope SniperScopePanel;
		private bool switchBackToThirdP = false;
		private float lerpZoomAmount = 0;

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			base.ActiveEnd( ent, dropped );

			SniperScopePanel?.Delete();
		}

		public virtual void OnScopedStart()
		{
			Primary.Spread /= 1000;

			var owner = GetClientOwner();
			if ( owner.Camera is ThirdPersonCamera )
			{
				switchBackToThirdP = true;
				owner.Camera = new FirstPersonCamera();
			}

			if ( IsLocalPawn )
			{
				ViewModelEntity.RenderAlpha = 0;
			}
		}

		public virtual void OnScopedEnd()
		{
			Primary.Spread *= 1000;
			lerpZoomAmount = 0;

			var owner = GetClientOwner();
			if ( switchBackToThirdP )
			{
				switchBackToThirdP = false;
				owner.Camera = new ThirdPersonCamera();
			}

			if ( IsLocalPawn && !(owner.Camera is ThirdPersonCamera))
			{
				ViewModelEntity.RenderAlpha = 1;
			}
		}

		public override void Simulate( Client owner )
		{
			base.Simulate( owner );

			if ( Input.Pressed( InputButton.Attack2 ) )
			{
				OnScopedStart();
				
			}

			if ( Input.Released( InputButton.Attack2 ) )
			{
				OnScopedEnd();
			}
		}
		public override void CreateHudElements()
		{
			base.CreateHudElements();

			if ( Local.Hud == null ) return;

			SniperScopePanel = new SniperScope(LensTexture, ScopeTexture);
			SniperScopePanel.Parent = Local.Hud;
		}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( IsZooming )
			{
				if ( lerpZoomAmount == 0 )
					lerpZoomAmount = camSetup.FieldOfView;

				lerpZoomAmount = MathZ.FILerp( lerpZoomAmount, ZoomAmount, 10f );
				camSetup.FieldOfView = lerpZoomAmount;
			}
		}
	}
}
