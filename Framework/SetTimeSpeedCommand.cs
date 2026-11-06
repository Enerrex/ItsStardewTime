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

        internal void ApplyProgress(TimeHelper clock)
        {
            // Progress is relative to a particular ten-minute interval. Let world sync
            // reconcile differing clocks rather than applying progress to the wrong one.
            if (Game1.timeOfDay == TimeOfDay && clock.CurrentIntervalProgress < TickProgress)
            {
                clock.SetTime(TickProgress);
            }
        }

        // Required for json deserialization
        public SetTimeSpeedCommand()
        {
        }

        public SetTimeSpeedCommand(int tickInterval, bool? manualFreeze, AutoFreezeReason autoFreeze)
        {
            TickProgress = TimeMaster.TimeSpeed.TimeHelper.CurrentIntervalProgress;
            TickInterval = tickInterval;
            TimeOfDay = Game1.timeOfDay;
            ManualFreeze = manualFreeze;
            AutoFreeze = autoFreeze;
        }
    }
}
