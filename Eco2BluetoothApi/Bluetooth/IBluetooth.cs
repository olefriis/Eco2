using System;
namespace Eco2.Bluetooth
{
    /*
     * Very simple abstraction of Bluetooth interaction, with very rough error handling.
     * Hopefully this same abstraction will be relatively easy to do on top of BlueZ on
     * Linux (and maybe something similar on Windows?).
     */
    public interface IBluetooth
    {
        event EventHandler<BluetoothFailureEventArgs> FailureEventHandler;
        event EventHandler<DiscoveredPeripheralEventArgs> DiscoveredPeripheralEventHandler;
        event EventHandler<ConnectedToPeripheralEventArgs> ConnectedToPeripheralEventHandler;
        event EventHandler<DisconnectedFromPeripheralEventArgs> DisconnectedFromPeripheralEventHandler;
        event EventHandler<DiscoveredCharacteristicsEventArgs> DiscoveredCharacteristicsEventHandler;
        event EventHandler<ReadCharacteristicValueEventArgs> ReadCharacteristicValueEventHandler;
        event EventHandler<WroteCharacteristicValueEventArgs> WroteCharacteristicValueEventHandler;

        void Start();
        void Stop();
        void StartScanning();
        void ConnectToPeripheralWithName(string name, bool firstConnect);
        void DiscoverCharacteristicsFor(Service service);
        void ReadCharacteristicsValue(Service service, Characteristic characteristic);
        void WriteCharacteristicsValue(Service service, Characteristic characteristic, byte[] value);
        void Disconnect();
    }
}
