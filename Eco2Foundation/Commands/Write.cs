using System;
using System.Collections.Generic;
using Eco2.Bluetooth;
using Eco2.Models;
using Eco2.Parsing;

namespace Eco2.Commands
{
    public class Write
    {
        readonly string serial;
        readonly IBluetooth bluetooth;
        readonly Thermostats thermostats;
        Peripheral connectedThermostat;
        Service batteryService;
        Service mainService;
        Characteristic[] batteryServiceCharacteristics;
        Characteristic[] mainServiceCharacteristics;
        SortedSet<string> characteristicValuesToRead;
        Dictionary<string, byte[]> characteristicValues = new Dictionary<string, byte[]>();

        public Write(string serial, IBluetooth bluetooth)
        {
            this.serial = serial;
            this.bluetooth = bluetooth;
            this.thermostats = Thermostats.Read();
            this.serial = serial;

            if (!thermostats.HasSecretFor(serial))
            {
                Console.Error.WriteLine($"Has not previously connected to {serial}. Do a read first.");
                Environment.Exit(1);
            }
        }

        public void Execute()
        {
            bluetooth.FailureEventHandler += Failure;
            bluetooth.ConnectedToPeripheralEventHandler += ConnectedToPeripheral;
            bluetooth.DiscoveredCharacteristicsEventHandler += DiscoveredMainServiceCharacteristics;
            bluetooth.DisconnectedFromPeripheralEventHandler += DisconnectedFromPeripheralEventHandler;

            bluetooth.ConnectToPeripheralWithName(serial, false);
            bluetooth.StartScanning();
            bluetooth.Start();

            Console.Error.WriteLine("Done");
            Environment.Exit(0);
        }

        void Failure(object sender, BluetoothFailureEventArgs e)
        {
            Console.Error.WriteLine($"Bluetooth error: {e}");
            Environment.Exit(1);
        }

        void ConnectedToPeripheral(object sender, ConnectedToPeripheralEventArgs a)
        {
            connectedThermostat = a.Peripheral;

            mainService = FindService(Uuids.MAIN_SERVICE);
            batteryService = FindService(Uuids.BATTERY_SERVICE);

            bluetooth.DiscoverCharacteristicsFor(mainService);
        }

        void DisconnectedFromPeripheralEventHandler(object sender, DisconnectedFromPeripheralEventArgs e)
        {
            Console.Error.WriteLine("Disconnected");
            bluetooth.Stop();
        }

        void DiscoveredMainServiceCharacteristics(object sender, DiscoveredCharacteristicsEventArgs a)
        {
            bluetooth.DiscoveredCharacteristicsEventHandler -= DiscoveredMainServiceCharacteristics;

            mainServiceCharacteristics = a.Characteristics;
            var pinCodeCharacteristic = Array.Find(mainServiceCharacteristics, characteristic => characteristic.Uuid == Uuids.PIN_CODE_CHARACTERISTIC);
            if (pinCodeCharacteristic == null)
            {
                Console.Error.WriteLine($"Did not find pin code characteristic ({Uuids.PIN_CODE_CHARACTERISTIC})");
                Environment.Exit(1);
            }

            bluetooth.WroteCharacteristicValueEventHandler += WrotePinCodeCharacteristicValue;
            Console.Error.WriteLine("Writing pin code");
            byte[] zeroBytes = { 0, 0, 0, 0 };
            bluetooth.WriteCharacteristicsValue(a.Service, pinCodeCharacteristic, zeroBytes);
        }

        void WrotePinCodeCharacteristicValue(object sender, WroteCharacteristicValueEventArgs e)
        {
            bluetooth.WroteCharacteristicValueEventHandler -= WrotePinCodeCharacteristicValue;

            bluetooth.DiscoveredCharacteristicsEventHandler += DiscoveredCharacteristicsForBatteryService;
            bluetooth.DiscoverCharacteristicsFor(batteryService);
        }

        void DiscoveredCharacteristicsForBatteryService(object sender, DiscoveredCharacteristicsEventArgs e)
        {
            bluetooth.DiscoveredCharacteristicsEventHandler -= DiscoveredCharacteristicsForBatteryService;

            batteryServiceCharacteristics = e.Characteristics;

            var temperatureCharacteristic = Array.Find(mainServiceCharacteristics, c => c.Uuid == Uuids.TEMPERATURE);
            if (temperatureCharacteristic == null)
            {
                Console.Error.WriteLine($"Did not find temperature characteristic ({Uuids.TEMPERATURE})");
                Environment.Exit(1);
            }

            bluetooth.WroteCharacteristicValueEventHandler += WroteTemperatureValue;
            var data = Conversion.HexStringToByteArray(thermostats.ThermostatWithSerial(serial).Temperature);
            bluetooth.WriteCharacteristicsValue(mainService, temperatureCharacteristic, data);
        }
        /*
            characteristicValuesToRead = new SortedSet<string>();
            ReadRelevantCharacteristicValuesFor(mainService, mainServiceCharacteristics);
            ReadRelevantCharacteristicValuesFor(batteryService, batteryServiceCharacteristics);
        }*/

        void WroteTemperatureValue(object sender, WroteCharacteristicValueEventArgs e)
        {
            bluetooth.WroteCharacteristicValueEventHandler -= WroteTemperatureValue;

            Console.Error.WriteLine("Wrote temperature value");

            var settingsCharacteristic = Array.Find(mainServiceCharacteristics, c => c.Uuid == Uuids.SETTINGS);
            if (settingsCharacteristic == null)
            {
                Console.Error.WriteLine($"Did not find settings characteristic ({Uuids.SETTINGS})");
                Environment.Exit(1);
            }

            bluetooth.WroteCharacteristicValueEventHandler += WroteSettingsValue;
            var data = Conversion.HexStringToByteArray(thermostats.ThermostatWithSerial(serial).Settings);
            bluetooth.WriteCharacteristicsValue(mainService, settingsCharacteristic, data);
        }

        void WroteSettingsValue(object sender, WroteCharacteristicValueEventArgs e)
        {
            bluetooth.WroteCharacteristicValueEventHandler -= WroteSettingsValue;

            Console.Error.WriteLine("Wrote settings value");
            bluetooth.Stop();
        }

        Service FindService(string uuid)
        {
            var service = Array.Find(connectedThermostat.Services, s => s.Uuid == uuid);
            if (service == null)
            {
                Console.Error.WriteLine($"Did not find service with UUID {uuid}");
                Environment.Exit(1);
            }
            return service;
        }
    }
}
