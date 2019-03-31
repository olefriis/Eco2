namespace Eco2.Bluetooth
{
    public class WroteCharacteristicValueEventArgs
    {
        public readonly Characteristic Characteristic;

        public WroteCharacteristicValueEventArgs(Characteristic characteristic)
        {
            Characteristic = characteristic;
        }
    }
}
