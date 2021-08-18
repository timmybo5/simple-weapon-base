using Sandbox;

namespace SWB_CSS
{
    [Library("swb_css_deagle_dual", Title = "Desert Eagle")]
    public class DeagleDual : Deagle
    {
        public override bool DualWield => true;

        public DeagleDual() : base()
        { }
    }
}
