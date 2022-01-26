using Sandbox;

namespace SWB_Base
{
    public partial class ScreenShake : BaseNetworkable
    {
        /// <summary>Duration length</summary>
        [Net]
        public float Length { get; set; } = 0f;

        /// <summary>Shake speed</summary>
        [Net]
        public float Speed { get; set; } = 0f;

        /// <summary>Screen disposition amount</summary>
        [Net]
        public float Size { get; set; } = 0f;

        /// <summary>Screen rotation amount</summary>
        [Net]
        public float Rotation { get; set; } = 0f;
    }
}
