namespace Eco2.Parsing
{
    public class Temperature
    {
        public static Temperature Parse(byte b)
        {
            return new Temperature(b);
        }

        public static Temperature FromDegreesCelcius(float degreesCelcius)
        {
            return new Temperature((byte)(degreesCelcius * 2));
        }

        Temperature(byte value)
        {
            this.Value = value;
        }

        public float InDegreesCelcius
        {
            get { return Value / 2F; }
        }

        public byte Value { get; }

        public override string ToString() => (Value / 2.0) + "°C";
    }
}
