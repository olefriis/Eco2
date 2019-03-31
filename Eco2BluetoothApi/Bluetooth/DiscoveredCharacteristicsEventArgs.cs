namespace Eco2.Bluetooth
{
    public class DiscoveredCharacteristicsEventArgs
    {
        public readonly Service Service;
        public readonly Characteristic[] Characteristics;

        public DiscoveredCharacteristicsEventArgs(Service service, Characteristic[] characteristics)
        {
            Service = service;
            Characteristics = characteristics;
        }
    }
}
