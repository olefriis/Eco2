namespace Eco2.Bluetooth
{
    public class Peripheral
    {
        public readonly string Name;
        public readonly string Uuid;
        public readonly Service[] Services;

        public Peripheral(string name, string uuid, Service[] services)
        {
            Name = name;
            Uuid = uuid;
            Services = services;
        }
    }
}
