using Sandbox;
using Sandbox.UI;

/* 
 * Weapon base for sniper based zooming
*/

namespace SWB_Base
{
    public partial class WeaponBaseSniper : WeaponBase
    {
        public virtual string LensTexture => "/materials/swb/scopes/swb_lens_hunter.png"; // Path to the lens texture
        public virtual string ScopeTexture => "/materials/swb/scopes/swb_scope_hunter.png"; // Path to the scope texture
        public virtual string ZoomInSound => "swb_sniper.zoom_in"; // Sound to play when zooming in
        public virtual string ZoomOutSound => ""; // Sound to play when zooming out
        public virtual float ZoomAmount => 20f; // The amount to zoom in ( lower is more )
        public virtual bool UseRenderTarget => false; // EXPERIMENTAL - Use a render target instead of a full screen texture zoom

        private Panel SniperScopePanel;
        private bool switchBackToThirdP = false;
        private float lerpZoomAmount = 0;
        private float oldSpread = -1;

        public override void ActiveStart(Entity ent)
        {
            base.ActiveStart(ent);
        }

        public override void ActiveEnd(Entity ent, bool dropped)
        {
            base.ActiveEnd(ent, dropped);

            SniperScopePanel?.Delete();
        }

        public virtual void OnScopedStart()
        {
            IsScoped = true;

            if (oldSpread == -1)
                oldSpread = Primary.Spread;

            Primary.Spread = 0;

            if (IsServer)
            {
                if (Owner.Camera is ThirdPersonCamera)
                {
                    switchBackToThirdP = true;
                    Owner.Camera = new FirstPersonCamera();
                }
            }

            if (IsLocalPawn)
            {
                ViewModelEntity.RenderColor = Color.Transparent;

                if (!string.IsNullOrEmpty(ZoomInSound))
                    PlaySound(ZoomInSound);
            }
        }

        public virtual void OnScopedEnd()
        {
            IsScoped = false;
            Primary.Spread = oldSpread;
            lerpZoomAmount = 0;

            if (IsServer && switchBackToThirdP)
            {
                switchBackToThirdP = false;
                Owner.Camera = new ThirdPersonCamera();
            }

            if (IsLocalPawn)
            {
                ViewModelEntity.RenderColor = Color.White;

                if (!string.IsNullOrEmpty(ZoomOutSound))
                    PlaySound(ZoomOutSound);
            }
        }

        public override void Simulate(Client owner)
        {
            base.Simulate(owner);

            if ((Input.Pressed(InputButton.Attack2) && !IsReloading && !IsRunning) || (IsZooming && !IsScoped))
            {
                OnScopedStart();
            }

            if (Input.Released(InputButton.Attack2) || (IsScoped && IsRunning))
            {
                OnScopedEnd();
            }
        }
        public override void CreateHudElements()
        {
            base.CreateHudElements();

            if (Local.Hud == null) return;

            if (UseRenderTarget)
            {
                SniperScopePanel = new SniperScopeRT(LensTexture, ScopeTexture);
                SniperScopePanel.Parent = Local.Hud;
            }
            else
            {
                SniperScopePanel = new SniperScope(LensTexture, ScopeTexture);
                SniperScopePanel.Parent = Local.Hud;
            }
        }

        public override void PostCameraSetup(ref CameraSetup camSetup)
        {
            base.PostCameraSetup(ref camSetup);

            if (IsScoped)
            {
                if (lerpZoomAmount == 0)
                    lerpZoomAmount = camSetup.FieldOfView;

                lerpZoomAmount = MathUtil.FILerp(lerpZoomAmount, ZoomAmount, 10f);
                camSetup.FieldOfView = lerpZoomAmount;
            }
        }
    }
}
