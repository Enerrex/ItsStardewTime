namespace ItsStardewTime.Framework.enums
{
    public enum PauseMode
    {
        Fair,
        Any,
        All,
        Host,
        Half,
        Majority,

        Auto, // Deprecated name. Renamed to Fair but keeping around so that old configs don't break
    }
}