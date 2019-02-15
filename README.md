Danfoss Eco™ 2 Reader
===
Simple command-line tool to do some of the tasks that you can do with your
Danfoss Eco™ 2 app for iOS or Android.

Why?
===
There are many more or less smart thermostats on the market at various price
points. Most of these contain some kind of central hub which connects to an
online service and lets you control your home while you are away, lets you group
your thermostats and easily schedule whole rooms at a time.

And then there's the Danfoss Eco™ 2.

The Eco 2 is a very cheap thermostat based on Bluetooth LE. It does not support
any kind of central hub, but instead relies on you using the associated app to
connect to all of your thermostats in turn and set up individual schedules. This
is very time-consuming.

This project is intended as an open-source "reference implementation" of how to
interface with the Eco 2 peripherals. Out of the box, it will hopefully turn
into something that will help you quickly set up your thermostats, set vacation
mode on and off, read battery levels, etc. It can also work as the foundation
for creating the missing piece in the Eco 2 ecosystem - a hub.

Building
===
This is a Mac-only project.

Download https://visualstudio.microsoft.com/vs/mac/ and open the outermost
solution.

Go through the [steps to get the Xamarin bindings working](https://docs.microsoft.com/en-us/xamarin/mac/app-fundamentals/console),
as there doesn't seem to be a properly supported way of doing this.

Then just build the thing.

Caveats
===
Currently this tool only supports reading the thermostat values, and it only
works on a Mac (as it uses the Core Bluetooth framework).

Also, it's very much a work-in-progress. A lot of stuff is not implemented yet:
- PIN codes can be set in the app, which will block this tool for now.
- Altering and writing back schedules and settings are not supported yet.
- ...and some settings are not parsed...

Usage
===
Just run the tool without any arguments:

```
> mono Eco2/bin/Debug/Eco2.exe
Too few arguments

scan - scan nearby devices for 120 seconds (Ctrl-C to stop)
read name - connect to and read specific thermostat
forget name - forget about a specific thermostat
list - show all of the previously read thermostats
show name - output all previously read values from a thermostat
```

In order to read a certain value from a thermostat, first of all you need to do
an initial connect. If you don't know the complete ID of your thermostat, use
the `scan` command to list the nearby thermostats.

Then do a `read` for your thermostat. When you first do this, the utility will
connect to your thermostat, and then it will wait for you to click on the
"timer" button on the thermostat. For subsequent reads, you don't need to click
the button.

You can see the list of thermostats you're connected to, by running the `list`
command. If you have connected to a certain device by accident, you can make the
utility forget about that by calling the `forget` command.

To read the current values from a thermostat, issue the `read` command again. To
see the last read values without connecting to the thermostat, use the `show`
command.

That's it!

The information fetched from the thermostats are stored in the `.eco2.xml` file
in your home directory, which means that deleting this file will reset the state
of the utility.

Code Structure
===
It may seem a little overkill with 3 projects in the solution instead of just
2: the code and some tests. However, I don't know how to add unit tests to a
".NET Framework" project, so I had to pull out some classes to a ".NET Standard"
project and then test those.

Otherwise it's a pretty small repository, so you'll probably get the gist of it.
At least I've tried hard to make readable code.

So... Did you hack the Eco 2 security?
===
If you've read the official specification for the Eco 2, you may have noticed
that it mentions that data on the device is secure, and that the security has
been audited by external parties. So did we break the security to make this
tool?

Short answer: No.

Without physical access to the thermostat, all you can do is connect to it and
read the battery level, the model number, firmware version, and other harmless
data. If you set a PIN code on the thermostat (in the app), you even need to
know that PIN code in order to retrieve this data.

To enable the tool to read the settings on the device, upon first connection you
need to physically push the "timer" button on the thermostat. This will, for a
short while, reveal an encryption key that can be used to read the remaining
data on the thermostat.

In other words, the Eco 2 security is sensible and seems well-implemented, and
in order to access the data, this tool (presumably!) does exactly the same as
the iOS and Android apps do.

Thank you!
===
This project is based largely on the information from the
[Danfoss-BLE](https://github.com/dsltip/Danfoss-BLE) project.

The XXTEA implementation is from the [xxtea-dotnet](https://github.com/xxtea/xxtea-dotnet)
project, modified such that the length of the input byte array is not included
in the encoded data. Accompanying license file is included in this repository.

Also thanks to my colleague [Simon](https://github.com/john7doe) who pushed me
in the first place and pointed me to the relevant parts of the Danfoss-BLE
repo mentioned above.
