using System;
using Eco2.Models;
using Eco2.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eco2Tests.Parsing
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
    }
}
