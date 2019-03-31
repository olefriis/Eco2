using System;
using Eco2.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eco2.Parsing
{
    [TestClass]
    public class ParsedThermostatTests
    {
        Thermostat thermostat;

        [TestInitialize]
        public void Initialize()
        {
            thermostat = new Thermostat
            {
                Serial = "0;0:04:2F:06:24:DD;eTRV",
                SecretKey = "DF-5B-7D-6A-16-32-CC-A4-79-30-6E-B3-78-B6-E9-59",
                BatteryLevel = "4E",
                Temperature = "86-03-60-1E-AB-A0-F7-8E",
                Name = "8F-EA-76-86-55-22-A2-82-F1-AB-F1-E4-17-1C-B0-02",
                Settings = "BE-41-EC-A1-33-D9-17-60-CF-59-AE-39-A5-47-8E-A2",
                Schedule1 = "79-3C-26-0B-9C-15-DE-55-B7-CA-2D-F8-43-AF-38-91-8E-4A-8D-7F",
                Schedule2 = "C8-4E-6B-6B-68-D7-2A-1C-9C-2B-A9-16",
                Schedule3 = "B3-45-EF-23-3B-1B-11-F0-3C-9B-1B-0A"
            };
        }

        [TestMethod]
        public void ParsesBatteryLevel()
        {
            Assert.AreEqual(78, new ParsedThermostat(thermostat).BatteryLevelPercent);
        }

        [TestMethod]
        public void ParsesDeviceName()
        {
            Assert.AreEqual("Tilbygning", new ParsedThermostat(thermostat).DeviceName);
        }

        [TestMethod]
        public void ParsesFrostProtectionTemperature()
        {
            Assert.AreEqual(6, new ParsedThermostat(thermostat).FrostProtectionTemperature.InDegreesCelcius);
        }

        [TestMethod]
        public void ParsesVacationTemperature()
        {
            Assert.AreEqual(15, new ParsedThermostat(thermostat).VacationTemperature.InDegreesCelcius);
        }

        [TestMethod]
        public void KnowsWhenScheduleModeIsScheduled()
        {
            Assert.AreEqual(ParsedThermostat.ScheduleModes.SCHEDULED, new ParsedThermostat(thermostat).ScheduleMode);
        }

        [TestMethod]
        public void ParsesSetPointTemperature()
        {
            Assert.AreEqual(23, new ParsedThermostat(thermostat).SetPointTemperature.InDegreesCelcius);
        }

        [TestMethod]
        public void CanSetSetPointTemperature()
        {
            var parsedThermostat = new ParsedThermostat(thermostat);
            parsedThermostat.SetPointTemperature = Temperature.FromDegreesCelcius(21.5F);

            parsedThermostat = new ParsedThermostat(thermostat);
            Assert.AreEqual(21.5, new ParsedThermostat(thermostat).SetPointTemperature.InDegreesCelcius);
        }

        [TestMethod]
        public void KnowsWhenVacationDatesAreNotSet()
        {
            Assert.IsNull(new ParsedThermostat(thermostat).VacationFrom);
            Assert.IsNull(new ParsedThermostat(thermostat).VacationTo);
        }

        [TestMethod]
        public void CanParseVacationDates()
        {
            var decryptedSettings = Decrypt(thermostat.Settings);

            // 2019-02-18 20:30:00 +0100 is 1550518200 in Unix timestamp
            var fromBytes = BitConverter.GetBytes(1550518200);
            // 2019-02-25 09:00:00 +0100 is 1551081600 in Unix timestamp
            var toBytes = BitConverter.GetBytes(1551081600);

            decryptedSettings[6] = fromBytes[3];
            decryptedSettings[7] = fromBytes[2];
            decryptedSettings[8] = fromBytes[1];
            decryptedSettings[9] = fromBytes[0];

            decryptedSettings[10] = toBytes[3];
            decryptedSettings[11] = toBytes[2];
            decryptedSettings[12] = toBytes[1];
            decryptedSettings[13] = toBytes[0];

            thermostat.Settings = Encrypt(decryptedSettings);

            Assert.AreEqual(DateTime.Parse("2019-02-18 20:30:00 +0100"), new ParsedThermostat(thermostat).VacationFrom);
            Assert.AreEqual(DateTime.Parse("2019-02-25 09:00:00 +0100"), new ParsedThermostat(thermostat).VacationTo);
        }

        [TestMethod]
        public void CanSetVacationDates()
        {
            var from = DateTime.Parse("2019-02-13 13:30:00 +0100");
            var to = DateTime.Parse("2019-02-20 18:15:00 +0100");

            var parsedThermostat = new ParsedThermostat(thermostat);
            parsedThermostat.VacationFrom = from;
            parsedThermostat.VacationTo = to;

            var anotherParsedThermostat = new ParsedThermostat(thermostat);
            Assert.AreEqual(from, anotherParsedThermostat.VacationFrom);
            Assert.AreEqual(to, anotherParsedThermostat.VacationTo);
        }

        [TestMethod]
        public void CanRemoveVacationDates()
        {
            var parsedThermostat = new ParsedThermostat(thermostat);
            parsedThermostat.VacationFrom = DateTime.Parse("2019-02-13 13:30:00 +0100");
            parsedThermostat.VacationTo = DateTime.Parse("2019-02-20 18:15:00 +0100");

            parsedThermostat = new ParsedThermostat(thermostat);
            parsedThermostat.VacationFrom = null;
            parsedThermostat.VacationTo = null;

            parsedThermostat = new ParsedThermostat(thermostat);
            Assert.IsNull(parsedThermostat.VacationFrom);
            Assert.IsNull(parsedThermostat.VacationTo);
        }

        byte[] Decrypt(string value) => new Encryption.Encryption(thermostat.SecretKey).Decrypt(value);
        string Encrypt(byte[] value) => new Encryption.Encryption(thermostat.SecretKey).Encrypt(value);
    }
}
