Danfoss Eco™ 2 Controller
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

Is this in any way an official project?
===
No. And if you brick your thermostats while using this tool, tough luck.

Limitations
===
Currently this tool only works on a Mac (as it uses the Core Bluetooth
framework), and there is only limited support for setting values on the
thermostats.

PIN codes are not supported.

In general this project is not aiming at getting full feature parity with the
official apps. We think it's OK if you need to use the app to enable or disable
adaptive learning, to switch between horizontal and vertical installation, and
other things that you will probably only do once when setting up the thermostat.

Usage
===
Just run the tool without any arguments:

```
> mono Eco2MacOS/bin/Debug/Eco2MacOS.exe
Too few arguments

scan - scan nearby devices for 120 seconds (Ctrl-C to stop)
read name - connect to and read specific thermostat
write name - connect to specific thermostat and write all values
forget name - forget about a specific thermostat
list - show all of the previously read thermostats
show name - output all previously read values from a thermostat
set name attribute value - set the given attribute to the provided value
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
command. To alter a value on the thermostat, use the `set` command, and to write
back to the thermostat, use the `write` command.

An example, in which we list the nearby thermostats, connect to and read from
one of them, and set the set-point temperature a little higher than it was
before:
```
> mono Eco2MacOS/bin/Debug/Eco2MacOS.exe scan
Scanning for thermostats for 120 seconds
0;0:04:2F:06:25:A6;eTRV
0;0:04:2F:06:25:A5;eTRV
0;0:04:2F:C0:F3:0C;eTRV
0;0:04:2F:06:24:D6;eTRV
0;0:04:2F:06:24:D1;eTRV
0;0:04:2F:C0:F2:58;eTRV
0;0:04:2F:06:24:DD;eTRV
> mono Eco2MacOS/bin/Debug/Eco2MacOS.exe read "0;0:04:2F:06:24:DD;eTRV"
[...lots of debug output. Just ignore, unless it ends with a clear sign of failure...]
> mono Eco2MacOS/bin/Debug/Eco2MacOS.exe show "0;0:04:2F:06:24:DD;eTRV"
Device name: Tilbygning
Battery level: 78%

Set-point/room temperature: 23°C / 23°C
Home/away temperature: 23°C / 19°C
Vacation/frost protection temperature: 15°C / 6°C
Schedule mode: SCHEDULED

Monday:
0:00 Home
7:00 Away
15:30 Home

Tuesday:
0:00 Home
7:00 Away
15:30 Home

Wednesday:
0:00 Home
7:00 Away
15:30 Home

Thursday:
0:00 Home
7:00 Away
15:30 Home

Friday:
0:00 Home
7:00 Away
15:30 Home

Saturday:
0:00 Home

Sunday:
0:00 Home
> mono Eco2MacOS/bin/Debug/Eco2MacOS.exe set "0;0:04:2F:06:24:DD;eTRV" set-point-temperature 23.5
> mono Eco2MacOS/bin/Debug/Eco2MacOS.exe write "0;0:04:2F:06:24:DD;eTRV"
[...a bit of debug output. Just ignore, unless it ends with a clear sign of failure...]
```

You can set and cancel vacation periods like so:
```
> mono Eco2MacOS/bin/Debug/Eco2MacOS.exe set "0;0:04:2F:06:24:DD;eTRV" vacation-period "2019-02-11 14:30" "2019-03-02 9:00"
> mono Eco2MacOS/bin/Debug/Eco2MacOS.exe set "0;0:04:2F:06:24:DD;eTRV" cancel-vacation
```

Remember to do a `write` after setting the properties, or they won't be written
back to the thermostat.

That's it!

The information fetched from the thermostats are stored in the `.eco2.xml` file
in your home directory, which means that deleting this file will reset the state
of the utility.

Building
===
This is a Mac-only project for now.

Download https://visualstudio.microsoft.com/vs/mac/ and open the outermost
solution.

Then just build the thing.

Just in case there's an issue with referencing the Mac bindings, go through the
[steps to get the Xamarin bindings working](https://docs.microsoft.com/en-us/xamarin/mac/app-fundamentals/console).
And file an issue in this project, as it shouldn't be an issue.

Code Structure
===
In order to isolate MacOS-specific code and dependencies, the console
application itself is isolated in the `Eco2MacOS` project. This implements the
Bluetooth API defined in the `Eco2BluetoothApi` project and invokes the
platform-agnostic code in the `Eco2Foundation` project (which also has a
reference to the `Eco2BluetoothApi` project as it is invoking methods on the
Bluetooth API).

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
