using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SWB_Base.Editor;

public class ModelDisplay : Panel
{
    public SceneWorld SceneWorld { get; private set; }
    public SceneObject SceneObject { get; private set; }
    public Model ActiveModel { get { return SceneObject.Model; } }

    public Angles CamAngles = new(50f, 450f, 0.0f);
    public float CamDistance = 150;
    public Vector3 CamPos => Vector3.Up * 10 + CamAngles.Forward * -CamDistance;

    private readonly ScenePanel scene;
    private bool canMouseDrag;

    public ModelDisplay(string modelPath, bool canMouseDrag = false)
    {
        StyleSheet.Load("/swb_base/editor/ui/ModelDisplay.scss");

        this.canMouseDrag = canMouseDrag;
        SetMouseCapture(canMouseDrag);
        Style.PointerEvents = canMouseDrag ? PointerEvents.All : PointerEvents.None;

        Style.FlexWrap = Wrap.Wrap;
        Style.JustifyContent = Justify.Center;
        Style.AlignItems = Align.Center;
        Style.AlignContent = Align.Center;

        SceneWorld = new SceneWorld();

        SceneObject = new SceneModel(SceneWorld, modelPath, Transform.Zero);

        var maxX = SceneObject.Model.Bounds.Maxs.x;
        var maxZ = SceneObject.Model.Bounds.Maxs.z;
        SceneObject.Position += new Vector3(-maxX / 2, 0, maxZ / 2);

        new SceneModel(SceneWorld, "models/room.vmdl", new Transform(SceneObject.Model.Bounds.Mins - 10));

        new SceneLight(SceneWorld, Vector3.Up * 150.0f, 9999, Color.White);
        new SceneLight(SceneWorld, Vector3.Up * 10.0f + Vector3.Forward * 100.0f, 9999, Color.White);
        new SceneLight(SceneWorld, Vector3.Up * 10.0f + Vector3.Backward * 100.0f, 9999, Color.White);
        new SceneLight(SceneWorld, Vector3.Up * 10.0f + Vector3.Left * 100.0f, 9999, Color.White);
        new SceneLight(SceneWorld, Vector3.Up * 10.0f + Vector3.Right * 100.0f, 9999, Color.White);

        scene = Add.ScenePanel(SceneWorld, CamPos, Rotation.From(CamAngles), 45);
    }

    public override void OnMouseWheel(float value)
    {
        CamDistance += value;
        CamDistance = CamDistance.Clamp(10, 200);

        base.OnMouseWheel(value);
    }

    public override void OnButtonEvent(ButtonEvent e)
    {
        if (e.Button == "mouseleft")
        {
            SetMouseCapture(e.Pressed);
        }

        base.OnButtonEvent(e);
    }

    public override void Tick()
    {
        base.Tick();

        if (canMouseDrag && HasMouseCapture)
        {
            CamAngles.pitch += Mouse.Delta.y;
            CamAngles.yaw -= Mouse.Delta.x;
            CamAngles.pitch = CamAngles.pitch.Clamp(0, 90);
        }

        scene.Camera.Position = MathUtil.FILerp(scene.Camera.Position, CamPos, 10);

        var newAngles = MathUtil.FILerp(scene.Camera.Rotation.Angles(), CamAngles, 10);
        scene.Camera.Rotation = Rotation.From(newAngles);
    }
}
