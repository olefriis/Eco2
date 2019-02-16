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

            return new DateTime(1970, 1, 1).AddSeconds(secondsAfter1970).ToLocalTime();
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
    }
}
