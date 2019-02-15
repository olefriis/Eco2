using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Eco2.Parsing;
using Eco2.Encryption;
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

            var batteryLevelBytes = Conversion.HexStringToByteArray(thermostat.BatteryLevel);
            Trace.Assert(batteryLevelBytes.Length == 1, "Expected battery level to be 1 byte, got " + batteryLevelBytes.Length);
            var batteryLevel = batteryLevelBytes[0];

            var xxTea = new Encryption.Encryption(thermostat.SecretKey);
            var deviceNameBytes = xxTea.Decrypt(thermostat.Name);
            var deviceName = Encoding.ASCII.GetString((byte[])(Array)deviceNameBytes);

            // No idea how to parse most of the settings bytes. That will require
            // some fiddling and comparing the bytes.
            var settings = xxTea.Decrypt(thermostat.Settings);
            Trace.Assert(settings.Length == 16, "Expected settings to be 16 bytes, got " + settings.Length);
            var frostProtectionTemperature = new Temperature(settings[3]);
            var vacationTemperature = new Temperature(settings[5]);
            var scheduleMode = ParseScheduleMode(settings[4]);
            var vacationFrom = Timestamp.Parse(settings.Skip(6).Take(4));
            var vacationTo = Timestamp.Parse(settings.Skip(10).Take(4));

            var temperature = xxTea.Decrypt(thermostat.Temperature);
            Trace.Assert(temperature.Length == 8, "Expected temperature to be 8 bytes, got " + temperature.Length);
            var setPointTemperature = new Temperature(temperature[0]);
            var roomTemperature = new Temperature(temperature[1]);

            var schedule1 = xxTea.Decrypt(thermostat.Schedule1);
            Trace.Assert(schedule1.Length == 20, "Expected schedule1 to be 20 bytes, got " + schedule1.Length);
            var schedule2 = xxTea.Decrypt(thermostat.Schedule2);
            Trace.Assert(schedule2.Length == 12, "Expected schedule2 to be 12 bytes, got " + schedule2.Length);
            var schedule3 = xxTea.Decrypt(thermostat.Schedule3);
            Trace.Assert(schedule3.Length == 12, "Expected schedule3 to be 12 bytes, got " + schedule3.Length);

            var homeTemperature = new Temperature(schedule1[0]);
            var awayTemperature = new Temperature(schedule1[1]);
            var mondaySchedule = DailySchedule.Parse(schedule1.Skip(2).Take(6));
            var tuesdaySchedule = DailySchedule.Parse(schedule1.Skip(8).Take(6));
            var wednesdaySchedule = DailySchedule.Parse(schedule1.Skip(14).Take(6));
            var thursdaySchedule = DailySchedule.Parse(schedule2.Take(6));
            var fridaySchedule = DailySchedule.Parse(schedule2.Skip(6).Take(6));
            var saturdaySchedule = DailySchedule.Parse(schedule3.Take(6));
            var sundaySchedule = DailySchedule.Parse(schedule3.Skip(6).Take(6));

            var unknown = xxTea.Decrypt(thermostat.Unknown);

            Console.WriteLine($"Device name: {deviceName}");
            Console.WriteLine($"Battery level: {batteryLevel}%");
            Console.WriteLine("");
            Console.WriteLine($"Set-point/room temperature: {setPointTemperature} / {roomTemperature}");
            Console.WriteLine($"Home/away temperature: {homeTemperature} / {awayTemperature}");
            Console.WriteLine($"Vacation/frost protection temperature: {vacationTemperature} / {frostProtectionTemperature}");
            Console.WriteLine($"Schedule mode: {scheduleMode}");
            if (vacationFrom != null && vacationTo != null)
            {
                Console.WriteLine($"Vacation: {vacationFrom} - {vacationTo}");
            }
            Console.WriteLine("");
            Console.WriteLine($"Monday:\n{mondaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Tuesday:\n{tuesdaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Wednesday:\n{wednesdaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Thursday:\n{thursdaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Friday:\n{fridaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Saturday:\n{saturdaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Sunday:\n{sundaySchedule}");
            Console.WriteLine("");
            Console.WriteLine($"Settings bytes: {FormatByteArray(settings)}");
            Console.WriteLine($"Unknown bytes: {FormatByteArray(unknown)}");
        }

        string ParseScheduleMode(byte b)
        {
            switch (b)
            {
                case 0:
                    return "Manual setting";
                case 1:
                    return "Using schedule";
                case 3:
                    return "Vacation";
                default:
                    return "Unknown";
            }
        }

        string FormatByteArray(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b + " ");
            }
            return sb.ToString();
        }
    }
}
