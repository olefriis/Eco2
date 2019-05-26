using System;
using Eco2.Models;
using Eco2.Parsing;

namespace Eco2.Commands
{
    public class SetValue
    {
        const string SET_POINT_TEMPERATURE = "set-point-temperature";
        const string VACATION_PERIOD = "vacation-period";
        const string CANCEL_VACATION = "cancel-vacation";
        readonly string serial;
        readonly string attributeName;
        readonly string[] attributeValues;

        public SetValue(string serial, string attributeName, string[] attributeValues)
        {
            this.serial = serial;
            this.attributeName = attributeName;
            this.attributeValues = attributeValues;
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
            var parsedThermostat = new ParsedThermostat(thermostat);

            switch (attributeName)
            {
                case SET_POINT_TEMPERATURE:
                    SetSetPointTemperature(thermostat);
                    break;
                case VACATION_PERIOD:
                    SetVacationPeriod(thermostat);
                    break;
                case CANCEL_VACATION:
                    CancelVacation(thermostat);
                    break;
                default:
                    Console.Error.WriteLine($"Only setting {SET_POINT_TEMPERATURE}, {VACATION_PERIOD}, and {CANCEL_VACATION} supported for now");
                    Environment.Exit(1);
                    break;
            }

            thermostats.Write();
        }

        public void SetSetPointTemperature(Thermostat thermostat)
        {
            if (attributeValues.Length != 1)
            {
                Console.Error.WriteLine($"Expected 1 argument for set-point temperature, got {attributeValues.Length}");
                Environment.Exit(1);
            }
            var attributeValue = attributeValues[0];
            var isFloat = float.TryParse(attributeValue, out float value);
            if (!isFloat)
            {
                Console.Error.WriteLine($"{attributeValue} is an invalid value for {attributeName}");
                Environment.Exit(1);
            }

            thermostat.UpdatedSetPointTemperature = Temperature.FromDegreesCelcius(value);
        }

        public void SetVacationPeriod(Thermostat thermostat)
        {
            if (attributeValues.Length != 2)
            {
                Console.Error.WriteLine($"Expected 2 arguments for vacation period, got {attributeValues.Length}");
                Environment.Exit(1);
            }
            var from = ParseDate(attributeValues[0]);
            var to = ParseDate(attributeValues[1]);

            thermostat.UpdateVacation(from, to);
        }

        public void CancelVacation(Thermostat thermostat)
        {
            if (attributeValues.Length != 0)
            {
                Console.Error.WriteLine($"Expected no further arguments for cancel vacation, got {attributeValues.Length}");
                Environment.Exit(1);
            }

            thermostat.CancelVacation();
        }

        DateTime ParseDate(string date)
        {
            try
            {
                return DateTime.Parse(date);
            }
            catch (FormatException e)
            {
                Console.Error.WriteLine($"Could not parse {date}: {e}");
                Environment.Exit(1);
                // Sigh
                return DateTime.Now;
            }
        }
    }
}
