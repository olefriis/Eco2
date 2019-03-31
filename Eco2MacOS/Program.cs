using AppKit;
using Eco2.Bluetooth.Mac;
using Eco2;

namespace Eco2MacOS
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            NSApplication.Init();
            var bluetooth = new MacOsBluetooth();
            EcoCli.Main(args, bluetooth);
        }
    }
}
