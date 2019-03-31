using Eco2.Models;

namespace Eco2.Commands
{
    public class Forget
    {
        readonly string serial;

        public Forget(string serial)
        {
            this.serial = serial;
        }

        public void Execute()
        {
            var thermostats = Thermostats.Read();
            thermostats.RemoveThermostatWithSerial(serial);
            thermostats.Write();
        }
    }
}
