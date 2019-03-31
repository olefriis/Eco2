using System;
using System.Collections.Generic;

namespace Eco2
{
    public class Uuids
    {
        public const string MAIN_SERVICE = "10020000-2749-0001-0000-00805F9B042F";
        public const string PIN_CODE_CHARACTERISTIC = "10020001-2749-0001-0000-00805F9B042F";

        public const string BATTERY_SERVICE = "180F";
        public const string BATTERY_LEVEL = "2A19";

        public const string SECRET_KEY = "1002000B-2749-0001-0000-00805F9B042F";
        public const string TEMPERATURE = "10020005-2749-0001-0000-00805F9B042F";
        public const string DEVICE_NAME = "10020006-2749-0001-0000-00805F9B042F";
        public const string SETTINGS = "10020003-2749-0001-0000-00805F9B042F";
        // Home temperature, Out temperature, Schedule Monday + Tuesday + Wednesday
        public const string SCHEDULE_1 = "1002000D-2749-0001-0000-00805F9B042F";
        // Schedule Thursday + Friday
        public const string SCHEDULE_2 = "1002000E-2749-0001-0000-00805F9B042F";
        // Schedule Saturday + Sunday
        public const string SCHEDULE_3 = "1002000F-2749-0001-0000-00805F9B042F";
        public static readonly SortedSet<string> RELEVANT_CHARACTERISTICS = new SortedSet<string>(new String[]{
            BATTERY_LEVEL,
            SECRET_KEY,
            TEMPERATURE,
            DEVICE_NAME,
            SETTINGS,
            SCHEDULE_1,
            SCHEDULE_2,
            SCHEDULE_3
        });
    }
}
