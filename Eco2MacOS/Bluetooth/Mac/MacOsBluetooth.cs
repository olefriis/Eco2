using System;
using System.Collections.Generic;
using CoreBluetooth;
using Foundation;

namespace Eco2.Bluetooth.Mac
{
    /*
     * Very simple wrapper on top of CoreBluetooth. Probably all we need for now.
     */
    public class MacOsBluetooth : IBluetooth
    {
        CBCentralManager central;
        string nameOfPeripheralToConnectTo;
        bool firstConnect;
        // Ensure that our peripheral object doesn't get garbage-collected
        List<CBPeripheral> discoveredPeripherals = new List<CBPeripheral>();
        CBPeripheral connectedCbPeripheral;
        Peripheral connectedPeripheral;

        public event EventHandler<BluetoothFailureEventArgs> FailureEventHandler;
        public event EventHandler<DiscoveredPeripheralEventArgs> DiscoveredPeripheralEventHandler;
        public event EventHandler<ConnectedToPeripheralEventArgs> ConnectedToPeripheralEventHandler;
        public event EventHandler<DisconnectedFromPeripheralEventArgs> DisconnectedFromPeripheralEventHandler;
        public event EventHandler<DiscoveredCharacteristicsEventArgs> DiscoveredCharacteristicsEventHandler;
        public event EventHandler<ReadCharacteristicValueEventArgs> ReadCharacteristicValueEventHandler;
        public event EventHandler<WroteCharacteristicValueEventArgs> WroteCharacteristicValueEventHandler;

        public void StartScanning()
        {
            central = new CBCentralManager();
            central.UpdatedState += UpdatedState;
        }

        public void ConnectToPeripheralWithName(string name, bool firstConnect)
        {
            Console.Error.WriteLine($"ConnectToPeripheralWithName(name={name}, firstConnect={firstConnect})");
            this.nameOfPeripheralToConnectTo = name;
            this.firstConnect = firstConnect;
        }

        public void DiscoverCharacteristicsFor(Service service)
        {
            // The event should probably have been called "DiscoveredCharacteristics"
            // instead of "DiscoveredCharacteristic" - at least it seems like it
            // will only be called once per service.
            connectedCbPeripheral.DiscoveredCharacteristic += DiscoveredCharacteristicsForService;
            var cbService = FindService(service);
            connectedCbPeripheral.DiscoverCharacteristics(cbService);
        }

        public void ReadCharacteristicsValue(Service service, Characteristic characteristic)
        {
            var cbCharacteristic = FindCharacteristic(service, characteristic);
            connectedCbPeripheral.ReadValue(cbCharacteristic);
        }

        public void WriteCharacteristicsValue(Service service, Characteristic characteristic, byte[] value)
        {
            var data = NSData.FromArray(value);
            var cbCharacteristic = FindCharacteristic(service, characteristic);
            connectedCbPeripheral.WriteValue(data, cbCharacteristic, CBCharacteristicWriteType.WithResponse);
            connectedCbPeripheral.WroteCharacteristicValue += WroteCharacteristicValue;
        }

        public void Disconnect()
        {
            central.DisconnectedPeripheral += DisconnectedPeripheral;
            central.CancelPeripheralConnection(connectedCbPeripheral);
        }

        void UpdatedState(object sender, EventArgs eventArgs)
        {
            Console.Error.WriteLine($"Updated state: {central.State}");
            if (central.State == CBCentralManagerState.PoweredOn)
            {
                Console.Error.WriteLine("Scanning for peripherals");
                central.DiscoveredPeripheral += DiscoveredPeripheral;
                central.ConnectedPeripheral += ConnectedPeripheral;
                central.FailedToConnectPeripheral += FailedToConnectPeripheral;
                central.ScanForPeripherals(new CBUUID[0]);
            }
        }

        void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs discoveredPeripheralEventArgs)
        {
            var peripheral = discoveredPeripheralEventArgs.Peripheral;
            if (peripheral == null || peripheral.Name == null)
            {
                return;
            }

            var name = peripheral.Name;
            DiscoveredPeripheralEventHandler?.Invoke(this, new DiscoveredPeripheralEventArgs(name));

            if (name == nameOfPeripheralToConnectTo)
            {
                // This is weird behaviour... If we only connect to the first
                // peripheral of this name, we seem to be connected after a
                // while and can send the pin and read the properties except
                // for the secret key.
                // If we also try to connect to the subsequently discovered
                // peripheral of the same name, the connection will block until
                // you press the timer button on the thermostat. Then we can
                // send the pin code and get the secret key.
                if (firstConnect && discoveredPeripherals.Count == 0)
                {
                    Console.Error.WriteLine("Push the timer button on the thermostat");
                }
                else
                {
                    Console.Error.WriteLine("Found thermostat. Connecting.");
                    central.DiscoveredPeripheral -= DiscoveredPeripheral;
                }

                discoveredPeripherals.Add(peripheral);
                central.ConnectPeripheral(peripheral);
            }
        }

        void ConnectedPeripheral(object sender, CBPeripheralEventArgs e)
        {
            Console.Error.WriteLine("Connected. Discovering services.");
            central.ConnectedPeripheral -= ConnectedPeripheral;
            central.StopScan();
            connectedCbPeripheral = e.Peripheral;

            connectedCbPeripheral.DiscoveredService += DiscoveredService;
            connectedCbPeripheral.UpdatedCharacterteristicValue += UpdatedCharacterteristicValue;
            connectedCbPeripheral.WroteCharacteristicValue += WroteCharacteristicValue;
            connectedCbPeripheral.DiscoverServices();
        }

        void FailedToConnectPeripheral(object sender, CBPeripheralErrorEventArgs e)
        {
            FailureEventHandler?.Invoke(this, new BluetoothFailureEventArgs(e.Error.Description));
        }

        void DiscoveredService(object sender, NSErrorEventArgs e)
        {
            // In Swift, we get a single "peripheral(_ peripheral: CBPeripheral, didDiscoverServices error: Error?)"
            // callback, but for some reason the Mono folks decided to send an
            // event for each discovered service - without telling us which
            // actual service the individual event is for. So we'll need to
            // remove the event handler.
            connectedCbPeripheral.DiscoveredService -= DiscoveredService;

            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not discover services: {e.Error.Description}");
                Environment.Exit(1);
            }

            Console.Error.WriteLine("Discovered services");
            var services = Array.ConvertAll(connectedCbPeripheral.Services, service => new Service(service.UUID.Uuid));
            connectedPeripheral = new Peripheral(connectedCbPeripheral.Name, services);
            ConnectedToPeripheralEventHandler?.Invoke(this, new ConnectedToPeripheralEventArgs(connectedPeripheral));
        }

        void DiscoveredCharacteristicsForService(object sender, CBServiceEventArgs e)
        {
            if (e.Error != null)
            {
                Console.Error.WriteLine($"Could not discover characteristics: {e.Error.Description}");
                Environment.Exit(1);
            }

            connectedCbPeripheral.DiscoveredCharacteristic -= DiscoveredCharacteristicsForService;
            var service = e.Service;
            var characteristics = Array.ConvertAll(service.Characteristics, c => new Characteristic(c.UUID.Uuid));
            DiscoveredCharacteristicsEventHandler?.Invoke(this, new DiscoveredCharacteristicsEventArgs(new Service(service.UUID.Uuid), characteristics));
        }

        void WroteCharacteristicValue(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Error != null)
            {
                FailureEventHandler?.Invoke(this, new BluetoothFailureEventArgs(e.Error.Description));
                return;
            }

            var characteristic = new Characteristic(e.Characteristic.UUID.Uuid);
            WroteCharacteristicValueEventHandler?.Invoke(this, new WroteCharacteristicValueEventArgs(characteristic));
        }

        void UpdatedCharacterteristicValue(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Error != null)
            {
                FailureEventHandler?.Invoke(this, new BluetoothFailureEventArgs(e.Error.Description));
                return;
            }

            Console.Error.WriteLine($"Got characteristic value for {e.Characteristic.UUID.Uuid}");
            var uuid = e.Characteristic.UUID.Uuid;
            var nsDataValue = e.Characteristic.Value;
            var characteristic = new Characteristic(uuid);
            var value = nsDataValue.ToArray();
            ReadCharacteristicValueEventHandler?.Invoke(this, new ReadCharacteristicValueEventArgs(characteristic, value));
        }

        void DisconnectedPeripheral(object sender, CBPeripheralErrorEventArgs e)
        {
            central.DisconnectedPeripheral -= DisconnectedPeripheral;
            DisconnectedFromPeripheralEventHandler?.Invoke(this, new DisconnectedFromPeripheralEventArgs(connectedPeripheral));
        }

        CBService FindService(Service service)
        {
            return Array.Find(connectedCbPeripheral.Services, s => s.UUID.Uuid == service.Uuid);
        }

        CBCharacteristic FindCharacteristic(Service service, Characteristic characteristic)
        {
            return Array.Find(FindService(service).Characteristics, c => c.UUID.Uuid == characteristic.Uuid);
        }
    }
}
