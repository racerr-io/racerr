public static class TimeExtensions
{
    public static string ToRaceTimeFormat(this float time)
    {
        int intTime = (int)time;
        int minutes = intTime / 60;
        int seconds = intTime % 60;
        int fraction = (int)(time * 10);
        fraction = fraction % 10;

        string timeText = minutes.ToString() + ":";
        timeText = timeText + seconds.ToString("00");
        timeText += "." + fraction.ToString();
        return timeText;
    }
}
