namespace Eco2.Models
{
    public class Thermostat
    {
        public string Serial;

        // Hex-encoded, and sometimes encrypted, values, as they have been read from the peripheral
        public string SecretKey;
        public string BatteryLevel;
        public string Temperature;
        public string Name;
        public string Settings;
        public string Schedule1;
        public string Schedule2;
        public string Schedule3;
    }
}
