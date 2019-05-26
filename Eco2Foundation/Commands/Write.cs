using System;
using System.Threading.Tasks;
using Eco2.Bluetooth;
using Eco2.Models;
using Eco2.Parsing;

namespace Eco2.Commands
{
    public class Write
    {
        readonly string serial;
        readonly Thermostats thermostats;
        readonly PeripheralAccessor accessor;

        public Write(string serial, IBluetooth bluetooth)
        {
            this.serial = serial;
            thermostats = Thermostats.Read();
            accessor = new PeripheralAccessor(bluetooth);

            if (!thermostats.HasSecretAndUuidFor(serial))
            {
                Console.Error.WriteLine($"Has not previously connected to {serial}. Do a read first.");
                Environment.Exit(1);
            }
        }

        public void Execute()
        {
            var thermostat = thermostats.ThermostatWithSerial(serial);
            Connect(thermostat).Wait();
            thermostats.Write();

            Console.Error.WriteLine("Done");
            Environment.Exit(0);
        }

        async Task Connect(Thermostat thermostat)
        {
            var name = thermostat.Serial;
            var uuid = thermostat.Uuid;
            var connectedThermostat = await accessor.ConnectToPeripheralWithNameAndUuid(name, uuid);

            var mainService = FindService(connectedThermostat, Uuids.MAIN_SERVICE);
            var batteryService = FindService(connectedThermostat, Uuids.BATTERY_SERVICE);

            var mainServiceCharacteristics = await accessor.DiscoverCharacteristicsFor(mainService);

            var secretValueCharacteristic = Array.Find(mainServiceCharacteristics, c => c.Uuid == Uuids.SECRET_KEY);
            if (secretValueCharacteristic == null && !thermostats.HasSecretAndUuidFor(serial))
            {
                Console.Error.WriteLine("You need to push the timer button on the thermostat");
                Environment.Exit(1);
            }

            var pinCodeCharacteristic = CharacteristicWithUuid(mainServiceCharacteristics, Uuids.PIN_CODE_CHARACTERISTIC);
            if (pinCodeCharacteristic == null)
            {
                Console.Error.WriteLine($"Did not find pin code characteristic ({Uuids.PIN_CODE_CHARACTERISTIC})");
                Environment.Exit(1);
            }

            Console.Error.WriteLine("Writing pin code");
            byte[] zeroBytes = { 0, 0, 0, 0 };
            await accessor.WriteCharacteristicValue(mainService, pinCodeCharacteristic, zeroBytes);
            Console.Error.WriteLine("Wrote pin code");

            var batteryServiceCharacteristics = await accessor.DiscoverCharacteristicsFor(batteryService);
            Console.Error.WriteLine("Discovered battery service characteristics");

            thermostat.BatteryLevel = await ReadCharacteristicWithUuid(batteryService, batteryServiceCharacteristics, Uuids.BATTERY_LEVEL);
            thermostat.Name = await ReadCharacteristicWithUuid(mainService, mainServiceCharacteristics, Uuids.DEVICE_NAME);
            thermostat.Temperature = await ReadCharacteristicWithUuid(mainService, mainServiceCharacteristics, Uuids.TEMPERATURE);
            thermostat.Settings = await ReadCharacteristicWithUuid(mainService, mainServiceCharacteristics, Uuids.SETTINGS);
            thermostat.Schedule1 = await ReadCharacteristicWithUuid(mainService, mainServiceCharacteristics, Uuids.SCHEDULE_1);
            thermostat.Schedule2 = await ReadCharacteristicWithUuid(mainService, mainServiceCharacteristics, Uuids.SCHEDULE_2);
            thermostat.Schedule3 = await ReadCharacteristicWithUuid(mainService, mainServiceCharacteristics, Uuids.SCHEDULE_3);

            var parsedThermostat = new ParsedThermostat(thermostat);
            parsedThermostat.ApplyUpdates();

            await WriteCharacteristicWithUuid(mainService, mainServiceCharacteristics, Uuids.TEMPERATURE, thermostat.Temperature);
            await WriteCharacteristicWithUuid(mainService, mainServiceCharacteristics, Uuids.SETTINGS, thermostat.Settings);

            await accessor.Disconnect();
        }

        async Task<string> ReadCharacteristicWithUuid(Service service, Characteristic[] characteristics, string uuid)
        {
            var characteristic = CharacteristicWithUuid(characteristics, uuid);
            return await ReadCharacteristic(service, characteristic);
        }

        async Task<string> ReadCharacteristic(Service service, Characteristic characteristic)
        {
            Console.Error.WriteLine($"Reading characteristic {characteristic.Uuid}");

            return BitConverter.ToString(await accessor.ReadCharacteristicValue(service, characteristic));
        }

        async Task<Characteristic> WriteCharacteristicWithUuid(Service service, Characteristic[] characteristics, string uuid, string value)
        {
            var characteristic = CharacteristicWithUuid(characteristics, uuid);
            return await WriteCharacteristic(service, characteristic, value);
        }

        async Task<Characteristic> WriteCharacteristic(Service service, Characteristic characteristic, string value)
        {
            Console.Error.WriteLine($"Writing characteristic {characteristic.Uuid}");

            return await accessor.WriteCharacteristicValue(service, characteristic, Conversion.HexStringToByteArray(value));
        }

        Characteristic CharacteristicWithUuid(Characteristic[] characteristics, string uuid)
        {
            return Array.Find(characteristics, c => c.Uuid == uuid);
        }

        Service FindService(Peripheral thermostat, string uuid)
        {
            var service = Array.Find(thermostat.Services, s => s.Uuid == uuid);
            if (service == null)
            {
                Console.Error.WriteLine($"Did not find service with UUID {uuid}");
                Environment.Exit(1);
            }
            return service;
        }
    }
}
