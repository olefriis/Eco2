namespace Eco2.Bluetooth
{
    public class ConnectedToPeripheralEventArgs
    {
        public readonly Peripheral Peripheral;

        public ConnectedToPeripheralEventArgs(Peripheral peripheral)
        {
            Peripheral = peripheral;
        }
    }
}
