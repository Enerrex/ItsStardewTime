using ItsStardewTime.Framework;
using StardewValley;

namespace ItsStardewTime.Patches.TimeDisplayPatches.ClockHelper;

public class Hours
{
    public static void Handle(int time, ref string timeString)
    {
        if (!TimeController.Config.Use24HourFormat)
            return;
        
        if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
            return;

        int colon = timeString.IndexOf(':');
        if (colon <= 0)
            return; // can't find hour token

        // Identify hour token as the contiguous digits immediately preceding ':'
        int end = colon - 1;
        int start = end;
        while (start >= 0 && char.IsDigit(timeString[start]))
            start--;
        start++; // move back onto the first digit

        int len = (end - start) + 1;
        if (len <= 0)
            return;

        int hours24 = (time / 100) % 24;

        // Preserve padding convention already present in the string.
        // If it was "06:xx", keep 2-digit. If it was "6:xx", keep 1-digit.
        string replacement = len >= 2 ? hours24.ToString("D2") : hours24.ToString();

        timeString = timeString.Remove(start, len).Insert(start, replacement);
    }
}