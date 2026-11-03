using ItsStardewTime.Framework.enums;
using StardewValley;

namespace ItsStardewTime.Framework
{
    internal class SetTimeSpeedCommand
    {
        public double TickProgress;
        public int TickInterval;
        public int TimeOfDay;
        public bool? ManualFreeze;
        public AutoFreezeReason AutoFreeze;
        internal bool IsTimeFrozen =>
            ManualFreeze == true
            || AutoFreeze != AutoFreezeReason.None && ManualFreeze != false;

        internal bool ShouldSyncProgress(int localTimeOfDay, double localProgress)
        {
            // Progress is relative to a particular ten-minute interval. Let world sync
            // reconcile differing clocks rather than applying progress to the wrong one.
            return localTimeOfDay == TimeOfDay && localProgress < TickProgress;
        }

        // Required for json deserialization
        public SetTimeSpeedCommand()
        {
        }

        public SetTimeSpeedCommand(int tickInterval, bool? manualFreeze, AutoFreezeReason autoFreeze)
        {
            TickProgress = TimeMaster.TimeSpeed.TimeHelper.TickProgress;
            TickInterval = tickInterval;
            TimeOfDay = Game1.timeOfDay;
            ManualFreeze = manualFreeze;
            AutoFreeze = autoFreeze;
        }
    }
}
