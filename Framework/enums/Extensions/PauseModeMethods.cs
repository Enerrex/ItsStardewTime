namespace ItsStardewTime.Framework.enums.Extensions;

static class PauseModeMethods
{
    public static bool IsDeprecated(this PauseMode value)
    {
        return value == PauseMode.Auto;
    }
}