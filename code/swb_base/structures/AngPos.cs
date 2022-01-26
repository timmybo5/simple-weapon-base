namespace SWB_Base
{
    public struct AngPos
    {
        public Angles Angle { get; set; } = new Angles();
        public Vector3 Pos { get; set; } = new Vector3();

        public static readonly AngPos Zero = new();

        public AngPos()
        {
        }

        public AngPos(Angles angle, Vector3 pos)
        {
            Angle = angle;
            Pos = pos;
        }

        public bool Equals(AngPos angPos)
        {
            return Angle == angPos.Angle && Pos == angPos.Pos;
        }

        public static AngPos operator +(AngPos x, AngPos y)
        {
            return new AngPos(x.Angle + y.Angle, x.Pos + y.Pos);
        }

        public static AngPos operator -(AngPos x, AngPos y)
        {
            return new AngPos(x.Angle - y.Angle, x.Pos - y.Pos);
        }

        public static AngPos operator -(AngPos x)
        {
            return new AngPos(x.Angle * -1, -x.Pos);
        }

        public static bool operator ==(AngPos x, AngPos y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(AngPos x, AngPos y)
        {
            return !x.Equals(y);
        }
    }
}
