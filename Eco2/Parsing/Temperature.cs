namespace Eco2.Parsing
{
    public class Temperature
    {
        int value;

        public Temperature(int value)
        {
            this.value = value;
        }

        public override string ToString() => (value / 2.0) + "°C";
    }
}
