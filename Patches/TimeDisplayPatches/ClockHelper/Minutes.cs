using ItsStardewTime.Framework;
using StardewValley;

namespace ItsStardewTime.Patches.TimeDisplayPatches.ClockHelper;

public static class Minutes
{
    public static void Handle(int time, ref string timeString)
    {
        // minutes in time are the last two digits
        int minute = time % 100;

        // Default (vanilla-like) behavior: only show every 10 minutes.
        if (!TimeController.Config.DisplayEveryMinute)
            minute = (minute / 10) * 10; // floor to 0,10,20,30,40,50

        // Find the minute token after ':'
        int colon = timeString.IndexOf(':');
        if (colon < 0)
            return;

        int minute_start = colon + 1;

        // We expect at least two characters after ':'
        if (minute_start + 1 >= timeString.Length)
            return;

        // Replace exactly two characters after ':' with the computed minute (00-59).
        // This assumes the game string contains minutes (it should).
        string new_minutes = minute.ToString("D2");

        // If the string after ':' isn't two digits (some locales can be weird),
        // fall back to "scan digits" and replace the digit run instead.
        char c0 = timeString[minute_start];
        char c1 = timeString[minute_start + 1];
        if (char.IsDigit(c0) && char.IsDigit(c1))
        {
            timeString = timeString.Remove(minute_start, 2).Insert(minute_start, new_minutes);
            return;
        }

        // Fallback: replace a contiguous digit run after ':'
        int start = minute_start;
        while (start < timeString.Length && !char.IsDigit(timeString[start]))
            start++;

        if (start >= timeString.Length)
            return;

        int end = start;
        while (end < timeString.Length && char.IsDigit(timeString[end]))
            end++;

        int len = end - start;
        if (len <= 0)
            return;

        timeString = timeString.Remove(start, len).Insert(start, new_minutes);
    }
}