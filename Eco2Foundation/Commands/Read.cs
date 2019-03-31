using System;
using System.Collections.Generic;
using Eco2.Bluetooth;
using Eco2.Models;
using System.Threading;

namespace Eco2.Commands
{
    public class Read
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
        Semaphore runningSemaphore = new Semaphore(0, 1);

        public Read(string serial, IBluetooth bluetooth)
        {
            this.serial = serial;
            this.bluetooth = bluetooth;
            this.thermostats = Thermostats.Read();
        }

        public void Execute()
        {
            bluetooth.FailureEventHandler += Failure;
            bluetooth.ConnectedToPeripheralEventHandler += ConnectedToPeripheral;
            bluetooth.ReadCharacteristicValueEventHandler += ReadCharacteristicValue;
            bluetooth.DiscoveredCharacteristicsEventHandler += DiscoveredMainServiceCharacteristics;
            bluetooth.DisconnectedFromPeripheralEventHandler += DisconnectedFromPeripheralEventHandler;

            var firstConnect = !thermostats.HasSecretFor(serial);
            bluetooth.ConnectToPeripheralWithName(serial, firstConnect);
            bluetooth.StartScanning();
            runningSemaphore.WaitOne();

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
            runningSemaphore.Release(1);
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
            characteristicValuesToRead = new SortedSet<string>();
            ReadRelevantCharacteristicValuesFor(mainService, mainServiceCharacteristics);
            ReadRelevantCharacteristicValuesFor(batteryService, batteryServiceCharacteristics);
            if (!characteristicValuesToRead.Contains(Uuids.SECRET_KEY)
                && !thermostats.HasSecretFor(serial))
            {
                Console.Error.WriteLine("You need to push the timer button on the thermostat");
                Environment.Exit(1);
            }
        }

        void ReadCharacteristicValue(object sender, ReadCharacteristicValueEventArgs e)
        {
            var characteristicUuid = e.Characteristic.Uuid;
            characteristicValuesToRead.Remove(characteristicUuid);
            characteristicValues[characteristicUuid] = e.Value;
            if (characteristicValuesToRead.Count == 0)
            {
                Console.Error.WriteLine("Read all values");
                UpdateValuesForThermostat();
                bluetooth.Disconnect();
            }
        }

        void ReadRelevantCharacteristicValuesFor(Service service, Characteristic[] characteristics)
        {
            foreach (var characteristic in characteristics)
            {
                Console.Error.WriteLine($" - {characteristic.Uuid}");
                if (Uuids.RELEVANT_CHARACTERISTICS.Contains(characteristic.Uuid))
                {
                    bluetooth.ReadCharacteristicsValue(service, characteristic);
                    characteristicValuesToRead.Add(characteristic.Uuid);
                }
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

            thermostats.Write();
        }

        string CharacteristicValueAsHex(string uuid)
        {
            return BitConverter.ToString(characteristicValues[uuid]);
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
