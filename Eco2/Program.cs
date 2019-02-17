using System;
using AppKit;
using Eco2.Commands;

namespace Eco2
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            RequireAtLeastOneArgument(args);
            NSApplication.Init();
            switch (args[0])
            {
                case "scan":
                    RequireNumberOfArguments(1, args);
                    new Scan().Execute();
                    break;
                case "read":
                    RequireNumberOfArguments(2, args);
                    new Read(args[1]).Execute();
                    break;
                case "write":
                    RequireNumberOfArguments(2, args);
                    new Write(args[1]).Execute();
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
                    RequireNumberOfArguments(4, args);
                    new SetValue(args[1], args[2], args[3]).Execute();
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
