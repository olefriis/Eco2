using System;
using Eco2.Parsing;
using Eco2.Models;

namespace Eco2.Commands
{
    public class Show
    {
        readonly string serial;

        public Show(string serial)
        {
            this.serial = serial;
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

            Console.WriteLine($"Device name: {parsed.DeviceName}");
            Console.WriteLine($"Battery level: {parsed.BatteryLevelPercent}%");
            Console.WriteLine("");
            Console.WriteLine($"Set-point/room temperature: {parsed.SetPointTemperature} / {parsed.RoomTemperature}");
            Console.WriteLine($"Home/away temperature: {parsed.HomeTemperature} / {parsed.AwayTemperature}");
            Console.WriteLine($"Vacation/frost protection temperature: {parsed.VacationTemperature} / {parsed.FrostProtectionTemperature}");
            Console.WriteLine($"Schedule mode: {parsed.ScheduleMode}");
            if (parsed.VacationFrom != null && parsed.VacationTo != null)
            {
                Console.WriteLine($"Vacation: {parsed.VacationFrom} - {parsed.VacationTo}");
            }
            Console.WriteLine("");
            Console.WriteLine($"Monday:\n{parsed.MondaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Tuesday:\n{parsed.TuesdaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Wednesday:\n{parsed.WednesdaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Thursday:\n{parsed.ThursdaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Friday:\n{parsed.FridaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Saturday:\n{parsed.SaturdaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Sunday:\n{parsed.SundaySchedule}");
        }
    }
}
