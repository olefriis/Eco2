using System;
using System.Linq;
using Eco2.Bluetooth;
using Eco2.Commands;

namespace Eco2
{
    public class EcoCli
    {
        public static void Main(string[] args, IBluetooth bluetooth)
        {
            RequireAtLeastOneArgument(args);
            switch (args[0])
            {
                case "scan":
                    RequireNumberOfArguments(1, args);
                    new Scan(bluetooth).Execute();
                    break;
                case "read":
                    RequireNumberOfArguments(2, args);
                    new Read(args[1], bluetooth).Execute();
                    break;
                case "write":
                    RequireNumberOfArguments(2, args);
                    new Write(args[1], bluetooth).Execute();
                    break;
                case "forget":
                    RequireNumberOfArguments(2, args);
                    new Forget(args[1]).Execute();
                    break;
                case "list":
                    RequireNumberOfArguments(1, args);
                    new ListThermostats().Execute();
                    break;
                case "show":
                    RequireNumberOfArguments(2, args);
                    new Show(args[1]).Execute();
                    break;
                case "set":
                    new SetValue(args[1], args[2], args.Skip(3).ToArray()).Execute();
                    break;
                default:
                    QuitWithUsage($"Unknown command: {args[0]}");
                    break;
            }
        }

        static void RequireAtLeastOneArgument(string[] args)
        {
            if (args.Length < 1)
            {
                QuitWithUsage("Too few arguments");
            }
        }

        static void RequireNumberOfArguments(int expected, string[] arguments)
        {
            if (arguments.Length != expected)
            {
                QuitWithUsage("Wrong number of arguments");
            }
        }

        static void QuitWithUsage(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine();
            Console.WriteLine("scan - scan nearby devices for 120 seconds (Ctrl-C to stop)");
            Console.WriteLine("read name - connect to and read specific thermostat");
            Console.WriteLine("write name - connect to specific thermostat and write all values");
            Console.WriteLine("forget name - forget about a specific thermostat");
            Console.WriteLine("list - show all of the previously read thermostats");
            Console.WriteLine("show name - output all previously read values from a thermostat");
            Console.WriteLine("set name attribute value - set the given attribute to the provided value");

            Environment.Exit(1);
        }
    }
}
