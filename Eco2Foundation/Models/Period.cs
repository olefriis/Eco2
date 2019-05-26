using System;

namespace Eco2.Models
{
    public class Period
    {
        // Gotta be read-write for XML serialization to work
        public DateTime From;
        public DateTime To;

        public Period(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        private Period()
        {
            // Constructor required for XML serialization to work
        }
    }
}
