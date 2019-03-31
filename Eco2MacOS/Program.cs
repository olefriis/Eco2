using AppKit;
using Eco2.Bluetooth.Mac;
using System.Threading;
using Foundation;

namespace Eco2
{
    class MainClass
    {
        static bool running;
        static Thread cliThread;

        public static void Main(string[] args)
        {
            NSApplication.Init();

            cliThread = new Thread(() => RunCli(args));
            cliThread.Start();

            RunRunLoop();
        }

        static void RunCli(string[] args)
        {
            var bluetooth = new MacOsBluetooth();
            EcoCli.Main(args, bluetooth);
            running = false;
        }

        static void RunRunLoop()
        {
            running = true;
            var runLoop = NSRunLoop.Current;
            while (running)
            {
                runLoop.RunUntil(NSDate.FromTimeIntervalSinceNow(1));
            }
        }
    }
}
