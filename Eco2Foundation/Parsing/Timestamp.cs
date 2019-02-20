using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Eco2.Parsing
{
    public static class Timestamp
    {
        public static DateTime? Parse(IEnumerable<byte> bytes)
        {
            Trace.Assert(bytes.Count() == 4, "Expected 4 bytes, got " + bytes.Count());

            if (AreAllZeroes(bytes))
            {
                return null;
            }

            var secondsAfter1970 = 0;
            foreach (var b in bytes)
            {
                secondsAfter1970 *= 256;
                secondsAfter1970 += b;
            }

            return LocalEpoch.AddSeconds(secondsAfter1970);
        }

        public static void WriteToByteArray(DateTime? dateTime, byte[] array, int offset)
        {
            if (dateTime == null)
            {
                array[offset + 0] = 0;
                array[offset + 1] = 0;
                array[offset + 2] = 0;
                array[offset + 3] = 0;
            }
            else
            {
                var secondsSinceEpoch = (int) dateTime.Value.Subtract(LocalEpoch).TotalSeconds;
                var bytes = BitConverter.GetBytes(secondsSinceEpoch);
                array[offset + 0] = bytes[3];
                array[offset + 1] = bytes[2];
                array[offset + 2] = bytes[1];
                array[offset + 3] = bytes[0];
            }
        }

        static bool AreAllZeroes(IEnumerable<byte> bytes)
        {
            foreach (var b in bytes)
            {
                if (b != 0)
                {
                    return false;
                }
            }
            return true;
        }

        static DateTime LocalEpoch => new DateTime(1970, 1, 1).ToLocalTime();
    }
}
