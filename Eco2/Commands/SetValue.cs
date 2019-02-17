using System;
using Eco2.Models;
using Eco2.Parsing;

namespace Eco2.Commands
{
    public class SetValue
    {
        const string SET_POINT_TEMPERATURE = "set-point-temperature";
        readonly string serial;
        readonly string attributeName;
        readonly float attributeValue;

        public SetValue(string serial, string attributeName, string attributeValue)
        {
            if (attributeName != SET_POINT_TEMPERATURE)
            {
                Console.Error.WriteLine($"Only setting {SET_POINT_TEMPERATURE} supported for now");
                Environment.Exit(1);
            }
            float value;
            var isFloat = float.TryParse(attributeValue, out value);
            if (!isFloat)
            {
                Console.Error.WriteLine($"{attributeValue} is an invalid value for {attributeName}");
                Environment.Exit(1);
            }

            this.serial = serial;
            this.attributeName = attributeName;
            this.attributeValue = value;
        }

        public void Execute()
        {
            var thermostats = Thermostats.Read();
            var thermostat = thermostats.ThermostatWithSerial(serial);
            if (thermostat == null)
            {
                Console.Error.WriteLine($"Thermostat with serial {serial} not found. Have you run the read command first?");
                Environment.Exit(1);
            }

            var parsed = new ParsedThermostat(thermostat);
            parsed.SetPointTemperature = Temperature.FromDegreesCelcius(attributeValue);
            thermostats.Write();
        }
    }
}
