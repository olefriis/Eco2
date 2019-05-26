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

        public static void Write(DateTime dateTime, byte[] array, int offset)
        {
            var secondsSinceEpoch = (int) dateTime.Subtract(LocalEpoch).TotalSeconds;
            var bytes = BitConverter.GetBytes(secondsSinceEpoch).Reverse().ToArray();
            WriteToByteArray(bytes, array, offset);
        }

        public static void Clear(byte[] array, int offset)
        {
            WriteToByteArray(new byte[] { 0, 0, 0, 0 }, array, offset);
        }

        static void WriteToByteArray(byte[] src, byte[] dest, int offset)
        {
            for (int i=0; i<src.Length; i++)
            {
                dest[offset + i] = src[i];
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
