using System;
using Eco2.Models;

namespace Eco2.Commands
{
    public class ListThermostats
    {
        public void Execute()
        {
            var thermostats = Thermostats.Read();
            foreach (var thermostat in thermostats.thermostats)
            {
                Console.WriteLine(thermostat.Serial);
            }
        }
    }
}
