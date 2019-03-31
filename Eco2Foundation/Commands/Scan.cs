using System;
using System.Collections.Generic;
using Eco2.Bluetooth;
using System.Threading;

namespace Eco2.Commands
{
    public class Scan
    {
        // No idea if we are too lax or not, but the string in between the below
        // prefix and suffix is what the app displays as "MAC Address" of the
        // thermostat.
        const string ECO_2_PREFIX = "0;";
        const string ECO_2_SUFFIX = ";eTRV";

        IBluetooth bluetooth;

        // For some reason, every thermostat exposes two peripherals with the
        // same name. We only want to show each name once, hence we want to
        // store the names we've already printed.
        SortedSet<string> thermostatNames = new SortedSet<string>();

        public Scan(IBluetooth bluetooth)
        {
            this.bluetooth = bluetooth;
        }

        public void Execute()
        {
            bluetooth.DiscoveredPeripheralEventHandler += DiscoveredPeripheral;
            bluetooth.StartScanning();
            Thread.Sleep(120 * 1000);
        }

        void DiscoveredPeripheral(object sender, DiscoveredPeripheralEventArgs e)
        {
            var name = e.Name;
            if (name.StartsWith(ECO_2_PREFIX, StringComparison.Ordinal)
                && name.EndsWith(ECO_2_SUFFIX, StringComparison.Ordinal)
                && !thermostatNames.Contains(name))
            {
                Console.WriteLine(name);
                thermostatNames.Add(name);
            }
        }
    }
}
