using StardewValley;

namespace ItsStardewTime.Framework
{
    internal sealed class BuffDurationLocks
    {
        private readonly Dictionary<Buff, int> _durations = new();

        internal void Protect(Buff buff)
        {
            if (buff.millisecondsDuration < 0)
            {
                _durations.Remove(buff);
                return;
            }

            if (_durations.TryGetValue(buff, out int duration) && buff.millisecondsDuration < duration)
            {
                buff.millisecondsDuration = duration;
            }
            else if (buff.millisecondsDuration > 0)
            {
                _durations[buff] = buff.millisecondsDuration;
            }
        }

        internal void Clear() => _durations.Clear();
    }
}
