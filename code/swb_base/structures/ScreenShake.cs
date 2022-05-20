using System;
using Sandbox;

namespace SWB_Base
{
    public partial class ScreenShake : BaseNetworkable
    {
        /// <summary>Duration length (s)</summary>
        [Net]
        public float Length { get; set; } = 0f;

        /// <summary>Delay between shakes (s)</summary>
        [Net]
        public float Delay { get; set; } = 0f;

        /// <summary>Screen disposition amount</summary>
        [Net]
        public float Size { get; set; } = 0f;

        /// <summary>Screen rotation amount</summary>
        [Net]
        public float Rotation { get; set; } = 0f;

        public override string ToString()
        {
            return String.Format("Length: {0}, Speed: {1}, Size: {2}, Rotation: {3} ", Length, Delay, Size, Rotation);
        }
    }

    // For client usage
    public struct ScreenShakeStruct
    {
        public float Length { get; set; } = 0f;
        public float Delay { get; set; } = 0f;
        public float Size { get; set; } = 0f;
        public float Rotation { get; set; } = 0f;
    }
}
