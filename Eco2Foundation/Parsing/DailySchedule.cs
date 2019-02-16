using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Eco2.Parsing
{
    public class DailySchedule
    {
        private List<TimeInterval> timeIntervals;

        public static DailySchedule Parse(IEnumerable<byte> bytes)
        {
            Trace.Assert(bytes.Count() == 6, "Expected 6 bytes, got " + bytes.Count());
            bool isAway = false;
            List<TimeInterval> intervals = new List<TimeInterval>();
            foreach (var b in bytes)
            {
                if (b == 48) // Seems to be the end-marker
                {
                    break;
                }
                intervals.Add(new TimeInterval(b, isAway));
                isAway = !isAway;
            }
            return new DailySchedule(intervals);
        }

        DailySchedule(List<TimeInterval> timeIntervals)
        {
            this.timeIntervals = timeIntervals;
        }

        public override string ToString() => String.Join("\n", timeIntervals.ConvertAll(interval => interval.ToString()).ToArray());
    }
}
