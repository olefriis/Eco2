using System;
using System.Collections.Generic;
using CoreBluetooth;
using Eco2.Models;
using Eco2.Parsing;
using Foundation;

namespace Eco2.Commands
{
    public class Write
    {
        readonly string serial;
        Thermostats thermostats;
        CBCentralManager central;
        // Ensure that our peripheral object doesn't get garbage-collected
        List<CBPeripheral> discoveredThermostats = new List<CBPeripheral>();
        CBPeripheral connectedThermostat;

        public Write(string serial)
        {
            this.serial = serial;
            this.thermostats = Thermostats.Read();
            if (!thermostats.HasSecretFor(serial))
            {
                Console.Error.WriteLine($"Has not previously connected to {serial}. Do a read first.");
                Environment.Exit(1);
            }
        }

        public void Execute()
        {
            central = new CBCentralManager();
            central.UpdatedState += UpdatedState;
            central.DiscoveredPeripheral += DiscoveredPeripheral;
            central.ConnectedPeripheral += ConnectedPeripheral;
            central.DisconnectedPeripheral += DisconnectedPeripheral;

            // Wait for 120 seconds
            var runLoop = NSRunLoop.Current;
            runLoop.RunUntil(NSDate.FromTimeIntervalSinceNow(120));
        }


        void DisconnectedPeripheral(object sender, CBPeripheralErrorEventArgs e)
        {
            Console.Error.WriteLine("Disconnected");
            Environment.Exit(0);
        }

        void UpdatedState(object sender, EventArgs eventArgs)
        {
            if (central.State == CBCentralManagerState.PoweredOn)
            {
                Console.Error.WriteLine("Scanning for peripherals");
                central.ScanForPeripherals(new CBUUID[0]);
            }
        }

        void DiscoveredPeripheral(object discoveredPeripheralSender, CBDiscoveredPeripheralEventArgs discoveredPeripheralEventArgs)
        {
            var peripheral = discoveredPeripheralEventArgs.Peripheral;
            if (peripheral != null && peripheral.Name == serial)
            {
                Console.Error.WriteLine("Found thermostat. Connecting.");
                central.DiscoveredPeripheral -= DiscoveredPeripheral;

                discoveredThermostats.Add(peripheral);
                central.ConnectPeripheral(peripheral);
            }
        }

        void ConnectedPeripheral(object sender, CBPeripheralEventArgs e)
        {
            Console.Error.WriteLine("Connected. Discovering services.");
            central.StopScan();
            connectedThermostat = e.Peripheral;

            connectedThermostat.DiscoveredService += DiscoveredService;
            connectedThermostat.DiscoverServices();
        }

        void DiscoveredService(object sender, NSErrorEventArgs e)
        {
            // In Swift, we get a single "peripheral(_ peripheral: CBPeripheral, didDiscoverServices error: Error?)"
            // callback, but for some reason the Mono folks decided to send an
            // event for each discovered service - without telling us which
            // actual service the individual event is for. So we'll need to
            // remove the event handler.
            connectedThermostat.DiscoveredService -= DiscoveredService;

            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not discover services: {e.Error.Description}");
                Environment.Exit(1);
            }

            Console.Error.WriteLine("Discovered services");
            var pinCodeService = Array.Find(connectedThermostat.Services, service => service.UUID.Uuid == Uuids.PIN_CODE_SERVICE);
            if (pinCodeService == null)
            {
                Console.Error.WriteLine($"Did not find pin code service ({Uuids.PIN_CODE_SERVICE})");
                Environment.Exit(1);
            }

            // The event should probably have been called "DiscoveredCharacteristics"
            // instead of "DiscoveredCharacteristic" - at least it seems like it
            // will only be called once per service.
            connectedThermostat.DiscoveredCharacteristic += DiscoveredCharacteristicsForPinCodeService;
            connectedThermostat.DiscoverCharacteristics(pinCodeService);
        }

        void DiscoveredCharacteristicsForPinCodeService(object sender, CBServiceEventArgs e)
        {
            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not discover characteristics: {e.Error.Description}");
                Environment.Exit(1);
            }

            connectedThermostat.DiscoveredCharacteristic -= DiscoveredCharacteristicsForPinCodeService;
            var service = e.Service;
            if (service.UUID.Uuid == Uuids.PIN_CODE_SERVICE)
            {
                var pinCodeCharacteristic = Array.Find(service.Characteristics, characteristic => characteristic.UUID.Uuid == Uuids.PIN_CODE_CHARACTERISTIC);
                if (pinCodeCharacteristic == null)
                {
                    Console.Error.WriteLine($"Did not find pin code characteristic ({Uuids.PIN_CODE_CHARACTERISTIC})");
                    Environment.Exit(1);
                }
                connectedThermostat.WroteCharacteristicValue += WrotePinValue;
                Console.Error.WriteLine("Writing pin code");
                byte[] bytes = { 0, 0, 0, 0 };
                var zeroBytes = NSData.FromArray(bytes);
                connectedThermostat.WriteValue(zeroBytes, pinCodeCharacteristic, CBCharacteristicWriteType.WithResponse);
            }
        }

        void WrotePinValue(object sender, CBCharacteristicEventArgs e)
        {
            connectedThermostat.WroteCharacteristicValue -= WrotePinValue;

            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not write pin code value: {e.Error.Description}");
                Environment.Exit(1);
            }

            var pinCodeService = Array.Find(connectedThermostat.Services, s => s.UUID.Uuid == Uuids.PIN_CODE_SERVICE);
            if (pinCodeService == null)
            {
                Console.Error.WriteLine($"Did not find pin code service ({Uuids.PIN_CODE_SERVICE})");
                Environment.Exit(1);
            }
            var temperatureCharacteristic = Array.Find(pinCodeService.Characteristics, c => c.UUID.Uuid == Uuids.TEMPERATURE);
            if (temperatureCharacteristic == null)
            {
                Console.Error.WriteLine($"Did not find temperature characteristic ({Uuids.TEMPERATURE})");
                Environment.Exit(1);
            }

            connectedThermostat.WroteCharacteristicValue += WroteTemperatureValue;
            NSData data = NSData.FromArray(Conversion.HexStringToByteArray(thermostats.ThermostatWithSerial(serial).Temperature));
            connectedThermostat.WriteValue(data, temperatureCharacteristic, CBCharacteristicWriteType.WithResponse);
        }

        void WroteTemperatureValue(object sender, CBCharacteristicEventArgs e)
        {
            connectedThermostat.WroteCharacteristicValue -= WroteTemperatureValue;

            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not write temperature value: {e.Error.Description}");
                Environment.Exit(1);
            }

            Console.Error.WriteLine("Wrote temperature value");

            var pinCodeService = Array.Find(connectedThermostat.Services, s => s.UUID.Uuid == Uuids.PIN_CODE_SERVICE);
            if (pinCodeService == null)
            {
                Console.Error.WriteLine($"Did not find pin code service ({Uuids.PIN_CODE_SERVICE})");
                Environment.Exit(1);
            }
            var settingsCharacteristic = Array.Find(pinCodeService.Characteristics, c => c.UUID.Uuid == Uuids.SETTINGS);
            if (settingsCharacteristic == null)
            {
                Console.Error.WriteLine($"Did not find settings characteristic ({Uuids.SETTINGS})");
                Environment.Exit(1);
            }

            connectedThermostat.WroteCharacteristicValue += WroteSettingsValue;
            NSData data = NSData.FromArray(Conversion.HexStringToByteArray(thermostats.ThermostatWithSerial(serial).Settings));
            connectedThermostat.WriteValue(data, settingsCharacteristic, CBCharacteristicWriteType.WithResponse);
        }

        void WroteSettingsValue(object sender, CBCharacteristicEventArgs e)
        {
            connectedThermostat.WroteCharacteristicValue -= WroteSettingsValue;

            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not write settings value: {e.Error.Description}");
                Environment.Exit(1);
            }

            Console.Error.WriteLine("Wrote settings value");
            central.CancelPeripheralConnection(connectedThermostat);
        }
    }
}
