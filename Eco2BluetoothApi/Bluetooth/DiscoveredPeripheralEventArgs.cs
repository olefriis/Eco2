namespace Eco2.Bluetooth
{
    public class DiscoveredPeripheralEventArgs
    {
        public readonly string Name;

        public DiscoveredPeripheralEventArgs(string name)
        {
            Name = name;
        }
    }
}
