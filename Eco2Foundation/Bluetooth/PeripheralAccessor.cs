using System;
using System.Threading.Tasks;

namespace Eco2.Bluetooth
{
    /*
     * Very simple async-await interface to Bluetooth, based on the IBluetooth
     * interface.
     * 
     * Error handling is extremely coarse: Print an error and exit with code 1.
     */
    public class PeripheralAccessor
    {
        readonly IBluetooth bluetooth;
        TaskCompletionSource<Peripheral> connectCompletionSource;
        TaskCompletionSource<Characteristic[]> discoverCharacteristicsCompletionSource;
        TaskCompletionSource<Characteristic> writeCharacteristicValueCompletionSource;
        TaskCompletionSource<byte[]> readCharacteristicValueCompletionSource;
        TaskCompletionSource<Peripheral> disconnectCompletionSource;

        public PeripheralAccessor(IBluetooth bluetooth)
        {
            this.bluetooth = bluetooth;

            bluetooth.FailureEventHandler += Failure;
        }

        public Task<Peripheral> ConnectToPeripheralWithName(string name, bool firstConnect)
        {
            connectCompletionSource = new TaskCompletionSource<Peripheral>();
            bluetooth.ConnectedToPeripheralEventHandler += ConnectedToPeripheral;
            bluetooth.ConnectToPeripheralWithName(name, firstConnect);
            bluetooth.StartScanning();
            return connectCompletionSource.Task;
        }

        void ConnectedToPeripheral(object sender, ConnectedToPeripheralEventArgs a)
        {
            bluetooth.ConnectedToPeripheralEventHandler -= ConnectedToPeripheral;
            connectCompletionSource.SetResult(a.Peripheral);
        }

        public Task<Characteristic[]> DiscoverCharacteristicsFor(Service service)
        {
            discoverCharacteristicsCompletionSource = new TaskCompletionSource<Characteristic[]>();
            bluetooth.DiscoveredCharacteristicsEventHandler += DiscoveredCharacteristics;
            bluetooth.DiscoverCharacteristicsFor(service);
            return discoverCharacteristicsCompletionSource.Task;
        }

        void DiscoveredCharacteristics(object sender, DiscoveredCharacteristicsEventArgs a)
        {
            bluetooth.DiscoveredCharacteristicsEventHandler -= DiscoveredCharacteristics;
            discoverCharacteristicsCompletionSource.SetResult(a.Characteristics);
        }

        public Task<Characteristic> WriteCharacteristicValue(Service service, Characteristic characteristic, byte[] value)
        {
            writeCharacteristicValueCompletionSource = new TaskCompletionSource<Characteristic>();
            bluetooth.WroteCharacteristicValueEventHandler += WroteCharacteristicValue;
            bluetooth.WriteCharacteristicsValue(service, characteristic, value);
            return writeCharacteristicValueCompletionSource.Task;
        }

        void WroteCharacteristicValue(object sender, WroteCharacteristicValueEventArgs e)
        {
            bluetooth.WroteCharacteristicValueEventHandler -= WroteCharacteristicValue;
            writeCharacteristicValueCompletionSource.SetResult(e.Characteristic);
        }

        public Task<byte[]> ReadCharacteristicValue(Service service, Characteristic characteristic)
        {
            readCharacteristicValueCompletionSource = new TaskCompletionSource<byte[]>();
            bluetooth.ReadCharacteristicValueEventHandler += ReadCharacteristicValue;
            bluetooth.ReadCharacteristicsValue(service, characteristic);
            return readCharacteristicValueCompletionSource.Task;
        }

        void ReadCharacteristicValue(object sender, ReadCharacteristicValueEventArgs e)
        {
            bluetooth.ReadCharacteristicValueEventHandler -= ReadCharacteristicValue;
            readCharacteristicValueCompletionSource.SetResult(e.Value);
        }

        public Task<Peripheral> Disconnect()
        {
            disconnectCompletionSource = new TaskCompletionSource<Peripheral>();
            bluetooth.DisconnectedFromPeripheralEventHandler += Disconnected;
            bluetooth.Disconnect();
            return disconnectCompletionSource.Task;
        }

        void Disconnected(object sender, DisconnectedFromPeripheralEventArgs e)
        {
            bluetooth.DisconnectedFromPeripheralEventHandler -= Disconnected;
            disconnectCompletionSource.SetResult(e.Peripheral);
        }

        void Failure(object sender, BluetoothFailureEventArgs e)
        {
            Console.Error.WriteLine($"Bluetooth error: {e}");
            Environment.Exit(1);
        }
    }
}
