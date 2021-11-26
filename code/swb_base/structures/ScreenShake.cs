using Sandbox;

namespace SWB_Base
{
    public partial class ScreenShake : BaseNetworkable
    {
        [Net]
        public float Length { get; set; } = 0f;
        [Net]
        public float Speed { get; set; } = 0f;
        [Net]
        public float Size { get; set; } = 0f;
        [Net]
        public float Rotation { get; set; } = 0f;
    }
}
