namespace Eco2.Parsing
{
    public class Temperature
    {
        readonly int value;

        public Temperature(int value)
        {
            this.value = value;
        }

        public float ValueInDegreesCelcius
        {
            get { return value / 2F; }
        }

        public override string ToString() => (value / 2.0) + "°C";
    }
}
