using System;
using System.Collections.Generic;
using CoreBluetooth;
using Eco2.Models;
using Foundation;

namespace Eco2.Commands
{
    public class Read
    {
        string serial;
        Thermostats thermostats;
        CBCentralManager central;
        // Ensure that our peripheral object doesn't get garbage-collected
        List<CBPeripheral> discoveredThermostats = new List<CBPeripheral>();
        CBPeripheral connectedThermostat;
        SortedSet<string> characteristicValuesToRead;
        Dictionary<string, NSData> characteristicValues = new Dictionary<string, NSData>();

        public Read(string serial)
        {
            this.serial = serial;
            this.thermostats = Thermostats.Read();
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
                // This is weird behaviour... If we only connect to the first
                // peripheral of this name, we seem to be connected after a
                // while and can send the pin and read the properties except
                // for the secret key.
                // If we also try to connect to the subsequently discovered
                // peripheral of the same name, the connection will block until
                // you press the timer button on the thermostat. Then we can
                // send the pin code and get the secret key.
                if (thermostats.HasSecretFor(serial))
                {
                    Console.Error.WriteLine("Found thermostat. Connecting.");
                    central.DiscoveredPeripheral -= DiscoveredPeripheral;
                }
                else
                {
                    if (discoveredThermostats.Count == 0)
                    {
                        Console.Error.WriteLine("Push the timer button on the thermostat");
                    }
                }

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
            connectedThermostat.UpdatedCharacterteristicValue += UpdatedCharacterteristicValue;
            connectedThermostat.WroteCharacteristicValue += WroteCharacteristicValue;

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
                Console.Error.WriteLine("Writing pin code");
                byte[] bytes = { 0, 0, 0, 0 };
                var zeroBytes = NSData.FromArray(bytes);
                connectedThermostat.WriteValue(zeroBytes, pinCodeCharacteristic, CBCharacteristicWriteType.WithResponse);
            }
        }

        void WroteCharacteristicValue(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not write pin code value: {e.Error.Description}");
                Environment.Exit(1);
            }

            var batteryService = Array.Find(connectedThermostat.Services, service => service.UUID.Uuid == Uuids.BATTERY_SERVICE);
            if (batteryService == null)
            {
                Console.Error.WriteLine($"Did not find battery service ({Uuids.BATTERY_SERVICE})");
                Environment.Exit(1);
            }
            connectedThermostat.DiscoveredCharacteristic += DiscoveredCharacteristicsForBatteryService;
            connectedThermostat.DiscoverCharacteristics(batteryService);
        }

        void DiscoveredCharacteristicsForBatteryService(object sender, CBServiceEventArgs e)
        {
            characteristicValuesToRead = new SortedSet<string>();
            foreach (var service in connectedThermostat.Services)
            {
                Console.Error.WriteLine($"Service {service.UUID.Uuid} - {service.Description}");
                if (service.Characteristics != null)
                {
                    foreach (var characteristic in service.Characteristics)
                    {
                        Console.Error.WriteLine($" - {characteristic.UUID.Uuid} - {characteristic.Description}");
                        if (Uuids.RELEVANT_CHARACTERISTICS.Contains(characteristic.UUID.Uuid))
                        {
                            connectedThermostat.ReadValue(characteristic);
                            characteristicValuesToRead.Add(characteristic.UUID.Uuid);
                        }
                    }
                }
            }
            if (!characteristicValuesToRead.Contains(Uuids.SECRET_KEY)
                && !thermostats.HasSecretFor(serial))
            {
                Console.Error.WriteLine("You need to push the timer button on the thermostat");
                Environment.Exit(1);
            }
        }

        void UpdatedCharacterteristicValue(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not read value for characteristic: {e.Error.Description}");
                Environment.Exit(1);
            }

            var uuid = e.Characteristic.UUID.Uuid;
            var value = e.Characteristic.Value;
            Console.WriteLine($"{uuid}: {value}");
            characteristicValuesToRead.Remove(uuid);
            characteristicValues[uuid] = value;
            if (characteristicValuesToRead.Count == 0)
            {
                Console.Error.WriteLine("Read all values");
                UpdateValuesForThermostat();
                central.CancelPeripheralConnection(connectedThermostat);
            }
        }

        void UpdateValuesForThermostat()
        {
            var thermostat = thermostats.ThermostatWithSerial(serial);

            if (characteristicValues.ContainsKey(Uuids.SECRET_KEY))
            {
                thermostat.SecretKey = CharacteristicValueAsHex(Uuids.SECRET_KEY);
            }
            thermostat.BatteryLevel = CharacteristicValueAsHex(Uuids.BATTERY_LEVEL);
            thermostat.Name = CharacteristicValueAsHex(Uuids.DEVICE_NAME);
            thermostat.Temperature = CharacteristicValueAsHex(Uuids.TEMPERATURE);
            thermostat.Settings = CharacteristicValueAsHex(Uuids.SETTINGS);
            thermostat.Schedule1 = CharacteristicValueAsHex(Uuids.SCHEDULE_1);
            thermostat.Schedule2 = CharacteristicValueAsHex(Uuids.SCHEDULE_2);
            thermostat.Schedule3 = CharacteristicValueAsHex(Uuids.SCHEDULE_3);
            thermostat.Unknown = CharacteristicValueAsHex(Uuids.UNKNOWN);

            thermostats.Write();
        }

        string CharacteristicValueAsHex(string uuid)
        {
            var binaryValue = characteristicValues[uuid];
            return BitConverter.ToString(binaryValue.ToArray());
        }
    }
}
