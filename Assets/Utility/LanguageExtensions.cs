namespace Racerr.Utility
{
    /// <summary>
    /// Extension functions for features in the C# language.
    /// </summary>
    public static class LanguageExtensions
    {
        /// <summary>
        /// Convert seconds to M:SS.FFF format.
        /// </summary>
        /// <remarks>
        /// Delegating function to ToRaceTimeFormat to convert float into the required double parameter type.
        /// </remarks>
        /// <param name="time">Time in seconds</param>
        /// <returns>String in M:SS.FFF format.</returns>
        public static string ToRaceTimeFormat(this float time)
        {
            return ToRaceTimeFormat((double)time);
        }

        /// <summary>
        /// Convert seconds to M:SS.FFF format.
        /// </summary>
        /// <param name="time">Time in seconds</param>
        /// <returns>String in M:SS.FFF format.</returns>
        public static string ToRaceTimeFormat(this double time)
        {
            int intTime = (int)time;
            int minutes = intTime / 60;
            int seconds = intTime % 60;
            int fraction = (int)(time * 1000);
            fraction %= 1000;

            string timeText = minutes.ToString() + ":";
            timeText += seconds.ToString("00");
            timeText += "." + fraction.ToString("000");
            return timeText;
        }
    }
}