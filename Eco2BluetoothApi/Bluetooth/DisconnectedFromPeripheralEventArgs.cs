namespace Eco2.Bluetooth
{
    public class DisconnectedFromPeripheralEventArgs
    {
        public readonly Peripheral Peripheral;

        public DisconnectedFromPeripheralEventArgs(Peripheral peripheral)
        {
            Peripheral = peripheral;
        }
    }
}
