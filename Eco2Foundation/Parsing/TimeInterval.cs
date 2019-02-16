namespace Eco2.Parsing
{
    public class TimeInterval
    {
        private bool isAway;
        private int startingAt; // Half ours from midnight

        public TimeInterval(int startingAt, bool isAway)
        {
            this.startingAt = startingAt;
            this.isAway = isAway;
        }

        public override string ToString() => formatStartTime() + " " + FormatHomeAway();

        string FormatHomeAway() => isAway ? "Away" : "Home";

        string formatStartTime() => (startingAt / 2) + ":" + (startingAt % 2 == 1 ? "30" : "00");
    }
}
