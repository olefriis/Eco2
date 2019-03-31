namespace Eco2.Bluetooth
{
    public class Peripheral
    {
        public readonly string Name;
        public readonly Service[] Services;

        public Peripheral(string name, Service[] services)
        {
            Name = name;
            Services = services;
        }
    }
}
