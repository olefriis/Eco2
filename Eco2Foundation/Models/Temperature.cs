namespace Eco2.Models
{
    public class Temperature
    {
        // Gotta be read-write for XML serialization to work
        public byte Value;

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

        private Temperature()
        {
            // Constructor required for XML serialization to work
        }

        public float InDegreesCelcius
        {
            get { return Value / 2F; }
        }

        public override string ToString() => (Value / 2.0) + "°C";
    }
}
