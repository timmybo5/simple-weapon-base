
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
        //SceneCapture sceneCapture;

        public SniperScopeRT(string lensTexture, string scopeTexture)
        {
            StyleSheet.Load("/swb_base/ui/SniperScopeRT.scss");

            SceneWorld.SetCurrent(SceneWorld.Current);
            //sceneCapture = SceneCapture.Create( "worldTestScene", 500, 500 );
            ScopeRT = Add.Image("scene:worldTestScene");
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
            var TargetPos = CurrentView.Position;
            var TargetAng = CurrentView.Rotation.Angles();

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
            RTTexture = Texture.Load("scene:worldTestScene", false);
            sceneObject.SetValue("ScopeRT", RTTexture);
        }
    }
}
