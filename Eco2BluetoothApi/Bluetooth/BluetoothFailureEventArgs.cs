namespace Eco2.Bluetooth
{
    public class BluetoothFailureEventArgs
    {
        public readonly string Message;

        public BluetoothFailureEventArgs(string message)
        {
            Message = message;
        }
    }
}
