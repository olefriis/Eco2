namespace Eco2.Bluetooth
{
    public class ReadCharacteristicValueEventArgs
    {
        public readonly Characteristic Characteristic;
        public readonly byte[] Value;

        public ReadCharacteristicValueEventArgs(Characteristic characteristic, byte[] value)
        {
            Characteristic = characteristic;
            Value = value;
        }
    }
}
