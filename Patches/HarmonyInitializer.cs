using HarmonyLib;
using ItsStardewTime.Patches.TimeDisplayPatches;

namespace ItsStardewTime.Patches;

internal static class HarmonyInitializer
{
    internal static void Initialize
    (
        in Harmony harmony
    )
    {
        // Handles the Top Right Corner display box (Day, time, money, etc)
        DayTimeMoneyBoxPatches.Initialize(harmony);
        // Handles patching the display time to account for minutes and other user settings (24 hour clock)
        TimeOfDayStringPatches.Initialize(harmony);
        // TODO: #1 Evaluate ongoing need #1, this was present in the original mod
        FixWarpToFestivalBugPatches.Initialize(harmony);
        // TODO: #2 Evaluate ongoing need #2, this was present in the original mod
        SkullCavernJumpPatches.Initialize(harmony);
    }
}