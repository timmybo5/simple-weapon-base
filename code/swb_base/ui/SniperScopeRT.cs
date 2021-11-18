using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

/*
 * Very experimental, NOT ready for use yet
*/

/* Layla removed SceneCapture.Create
 * Remove bullshit SceneCapture, replaced with Render.DrawScene and UI.Scene control
 * TODO: find out if a proper renderscope can be made now!
*/

namespace SWB_Base
{

    public class SniperScopeRT : Panel
    {
        Image ScopeRT;
        Texture RTTexture;
        Texture ColorTexture;
        Texture DepthTexture;
        ScenePanel scene;

        public SniperScopeRT(string lensTexture, string scopeTexture)
        {
            StyleSheet.Load("/swb_base/ui/SniperScopeRT.scss");

            SceneWorld.SetCurrent(SceneWorld.Current);
            //sceneCapture = SceneCapture.Create( "worldTestScene", 500, 500 );
            ScopeRT = Add.Image("scene:worldTestScene");


            // TESTING
            ColorTexture = Texture.CreateRenderTarget().WithSize(500, 500).WithScreenFormat()
                           .WithScreenMultiSample()
                           .Create();

            DepthTexture = Texture.CreateRenderTarget().WithSize(500, 500).WithDepthFormat()
                           .WithScreenMultiSample()
                           .Create();
        }

        public override void OnDeleted()
        {
            base.OnDeleted();

            //sceneCapture?.Delete();
            //sceneCapture = null;
        }

        [Event("frame")]
        public void OnFrame()
        {

        }

        public override void Tick()
        {
            base.Tick();

            var player = Local.Pawn;
            if (player == null) return;

            var weapon = player.ActiveChild as WeaponBaseSniper;
            if (weapon == null) return;

            // Update render camera
            var targetPos = CurrentView.Position;
            var targetAng = CurrentView.Rotation.Angles();

            // sceneCapture.SetCamera( TargetPos, TargetAng, weapon.ZoomAmount );

            // RenderTarget on a panel
            var scopeBone = weapon.ViewModelEntity.GetBoneTransform("v_weapon_awm_bolt_action");
            var screenpos = scopeBone.Position.ToScreen();

            if (screenpos.z < 0)
                return;

            this.Style.Left = Length.Fraction(screenpos.x);
            this.Style.Top = Length.Fraction(screenpos.y);
            this.Style.Dirty();

            // RenderTarget inside a material
            var sceneObject = weapon.ViewModelEntity.SceneObject;
            //RTTexture = Texture.Load("scene:worldTestScene", false);

            // TESTING
            //Render.SetRenderTarget(RTTexture);
            //Render.DrawScene(Texture.White, DepthTexture, new Vector2(500, 500), SceneWorld.Current, targetPos, targetAng, weapon.ZoomAmount);

            //sceneObject.SetValue("ScopeRT", RTTexture);
        }

        public override void DrawBackground(ref RenderState state)
        {
            var player = Local.Pawn;
            if (player == null) return;

            var weapon = player.ActiveChild as WeaponBaseSniper;
            if (weapon == null) return;

            var targetPos = CurrentView.Position;
            var targetAng = CurrentView.Rotation.Angles();
            var sceneObject = weapon.ViewModelEntity.SceneObject;

            Render.SetRenderTarget(RTTexture);
            Render.DrawScene(ColorTexture, DepthTexture, new Vector2(500, 500), SceneWorld.Current, targetPos, targetAng, weapon.ZoomAmount);

            sceneObject.SetValue("ScopeRT", RTTexture);
        }
    }
}
