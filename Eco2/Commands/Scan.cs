using System;
using System.Collections.Generic;
using CoreBluetooth;
using Foundation;

namespace Eco2.Commands
{
    public class Scan
    {
        // No idea if we are too lax or not, but the string in between the below
        // prefix and suffix is what the app displays as "MAC Address" of the
        // thermostat.
        const string ECO_2_PREFIX = "0;";
        const string ECO_2_SUFFIX = ";eTRV";

        CBCentralManager central;
        // For some reason, every thermostat exposes two peripherals with the
        // same name. We only want to show each name once, hence we want to
        // store the names we've already printed.
        SortedSet<string> thermostatNames = new SortedSet<string>();

        public void Execute()
        {
            Console.Error.WriteLine("Scanning for thermostats for 120 seconds");
            central = new CBCentralManager();

            central.UpdatedState += UpdatedState;
            central.DiscoveredPeripheral += DiscoveredPeripheral;

            // Wait for 120 seconds
            var runLoop = NSRunLoop.Current;
            runLoop.RunUntil(NSDate.FromTimeIntervalSinceNow(120));
        }

        void UpdatedState(object sender, EventArgs eventArgs)
        {
            if (central.State == CBCentralManagerState.PoweredOn)
            {
                central.ScanForPeripherals(new CBUUID[0]);
            }
        }

        void DiscoveredPeripheral(object discoveredPeripheralSender, CBDiscoveredPeripheralEventArgs discoveredPeripheralEventArgs)
        {
            var peripheral = discoveredPeripheralEventArgs.Peripheral;
            if (peripheral != null
                && peripheral.Name != null
                && peripheral.Name.StartsWith(ECO_2_PREFIX, StringComparison.Ordinal)
                && peripheral.Name.EndsWith(ECO_2_SUFFIX, StringComparison.Ordinal)
                && !thermostatNames.Contains(peripheral.Name))
            {
                Console.WriteLine(peripheral.Name);
                thermostatNames.Add(peripheral.Name);
            }
        }
    }
}
