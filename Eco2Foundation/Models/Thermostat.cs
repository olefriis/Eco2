using System;

namespace Eco2.Models
{
    public class Thermostat
    {
        public string Serial;
        public string Uuid;

        // Hex-encoded, and sometimes encrypted, values, as they have been read from the peripheral
        public string SecretKey;
        public string BatteryLevel;
        public string Temperature;
        public string Name;
        public string Settings;
        public string Schedule1;
        public string Schedule2;
        public string Schedule3;

        // Values that we want to push back to the thermostat
        public Temperature UpdatedSetPointTemperature;
        public bool HasUpdatedVacationPeriod;
        public Period UpdatedVacationPeriod;

        public bool HasUpdatedAttributes
        {
            get { return UpdatedSetPointTemperature != null || HasUpdatedVacationPeriod; }
        }

        public void UpdateVacation(DateTime from, DateTime to)
        {
            UpdatedVacationPeriod = new Period(from, to);
            HasUpdatedVacationPeriod = true;
        }

        public void CancelVacation()
        {
            UpdatedVacationPeriod = null;
            HasUpdatedVacationPeriod = true;
        }
    }
}
