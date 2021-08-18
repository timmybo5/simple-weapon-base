using Sandbox;

namespace SWB_CSS
{
    [Library("swb_css_ak47_dual", Title = "AK-47")]
    public class AK47Dual : AK47
    {
        public override bool DualWield => true;

        public AK47Dual() : base() { }
    }
}
