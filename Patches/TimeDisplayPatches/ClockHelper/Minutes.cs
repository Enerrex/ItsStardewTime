namespace ItsStardewTime.Patches.TimeDisplayPatches.ClockHelper;

public static class Minutes
{
    public static void Handle(int time, ref string timeString)
    {
        var minute = time % 100;
        var new_minutes = minute.ToString("D2");

        // Find the separator between minutes and hours.
        var separator_index = FindTimeSeparator(timeString);

        // Starting point of the minutes portion of the string, SHOULD be at most 2 digits.
        var start = separator_index + 1;

        // Skip any non-digits immediately after the colon.
        while (start < timeString.Length && !char.IsDigit(timeString[start]))
            start++;

        // In moving past non-digits, we went past the length of the string, maybe malformed?
        // Returning here will leave the string as-is, which may be okay.
        if (start >= timeString.Length)
            return;

        // Should realistically run twice, to find the at most 2 digit run.
        // Safeguards prior to this "should" protect against 1 digit runs
        var end = start;
        while (end < timeString.Length && char.IsDigit(timeString[end]))
            end++;

        // Replace the current minutes with new minutes.
        timeString =
            timeString.Remove
            (
                start, end - start
            ).Insert
            (
                start, new_minutes
            );
    }

    // Very naive and brittle approach but should nonethless cover the cases provided in Stardew.
    private static int FindTimeSeparator(string timeString)
    {
        for (int char_index = 0; char_index < timeString.Length; char_index++)
        {
            char current_char = timeString[char_index];
            if (current_char is ':' or 'h' or 'H')
                return char_index;
        }

        return -1;
    }
}