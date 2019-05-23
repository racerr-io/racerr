public static class TimeExtensions
{
    /// <summary>
    /// Convert seconds to M:SS.F format.
    /// </summary>
    /// <param name="time">Time in seconds</param>
    /// <returns>String in M:SS.F format.</returns>
    public static string ToRaceTimeFormat(this float time)
    {
        return ToRaceTimeFormat(time);
    }

    /// <summary>
    /// Convert seconds to M:SS.F format.
    /// </summary>
    /// <param name="time">Time in seconds</param>
    /// <returns>String in M:SS.F format.</returns>
    public static string ToRaceTimeFormat(this double time)
    {
        int intTime = (int)time;
        int minutes = intTime / 60;
        int seconds = intTime % 60;
        int fraction = (int)(time * 10);
        fraction %= 10;

        string timeText = minutes.ToString() + ":";
        timeText += seconds.ToString("00");
        timeText += "." + fraction.ToString();
        return timeText;
    }
}
