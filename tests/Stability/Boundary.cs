// Headless boundary doubles only. All voting, accounting, aggregation, config copying,
// time scaling and synchronization decisions are compiled from production source.
using ItsStardewTime.Framework;
using ItsStardewTime.Framework.Config;
using StardewModdingAPI;

namespace StardewValley
{
    public enum Season { Spring }
    public class GameLocation
    {
        public int ExtraMillisecondsPerInGameMinute;
        public bool Freeze;
        public int Interval = 7000;
    }
    public class Farmer
    {
        public long UniqueMultiplayerID;
        public string Name = "Player";
        public GameLocation currentLocation = new();
    }
    public static class Utility
    {
        public static int getSeasonNumber(string season) => 0;
        public static bool isFestivalDay(int day, Season season) => false;
    }
    public static class Game1
    {
        public static int ticks;
        public static int timeOfDay = 600;
        public static int realMilliSecondsPerGameTenMinutes = 7000;
        public static int gameTimeInterval;
        public static Farmer MasterPlayer = new() { UniqueMultiplayerID = 1 };
        public static Dictionary<long, Farmer> Players = new();
        public static GameClock currentGameTime = new();
        public static Farmer getFarmer(long id) => GetPlayer(id) ?? MasterPlayer;
        public static Farmer? GetPlayer(long id) => Players.GetValueOrDefault(id);
    }
    public class GameClock
    {
        public TimeSpan ElapsedGameTime = TimeSpan.FromMilliseconds(16);
    }
}
namespace StardewModdingAPI.Events
{
    public class UpdateTickedEventArgs : EventArgs { }
}
namespace StardewModdingAPI
{
    public enum LogLevel { Debug, Info, Warn, Error, Alert }
    public interface IMonitor
    {
        void Log(string message, LogLevel level = LogLevel.Debug);
        void LogOnce(string message, LogLevel level);
    }
    public interface IManifest { string UniqueID { get; } }
    public interface IMultiplayerHelper
    {
        void SendMessage<T>(
            T message,
            string type,
            string[] modIDs,
            long[]? playerIDs = null
        );
    }
    public interface IGameLoopEvents
    {
        event EventHandler<Events.UpdateTickedEventArgs>? UpdateTicked;
    }
    public interface IModEvents { IGameLoopEvents GameLoop { get; } }
    public interface IModHelper
    {
        IMultiplayerHelper Multiplayer { get; }
        IModEvents Events { get; }
    }
    public static class Context
    {
        public static bool IsMultiplayer = true;
        public static bool IsMainPlayer = true;
        public static bool IsWorldReady = true;
        public static int ScreenId;
    }
}
namespace ItsStardewTime.Framework.Config
{
    internal class ModSecondsPerMinuteConfig
    {
        public double GetSecondsPerMinute(StardewValley.GameLocation location) => location.Interval / 10000d;
    }
    internal class ModFreezeTimeConfig
    {
        public int? AnywhereAtTime = null;
        public bool ShouldFreeze(StardewValley.GameLocation location) => location.Freeze;
    }
    internal class ModControlsConfig { }
}
namespace ItsStardewTime
{
    internal static class TimeMaster
    {
        internal static TimeSpeed TimeSpeed = null!;
    }
    internal static class I18n
    {
        internal static string Message_PlayerJoinedAPausedGame(string name) => name;
        internal static string Message_PlayerVotedToPause(
            string name,
            int yes,
            double total
        ) => $"{yes}/{total}";
        internal static string Message_PlayerVotedToUnpause(
            string name,
            int yes,
            double total
        ) => $"{yes}/{total}";
        internal static string Message_OnTimeChange_TimeStopped() => "stopped";
        internal static string Message_OnLocationChange_TimeStoppedGlobally() => "global";
        internal static string Message_OnLocationChange_TimeStoppedHere() => "here";
        internal static string Message_OnLocationChange_TimeSpeedHere(int seconds) => "speed";
    }
}
namespace ItsStardewTime.Framework
{
    internal static class Notifier
    {
        internal static void NotifyInChatBox(string message) { }
        internal static void ShortNotify(string message) { }
    }
}
internal sealed class Boundary : IMonitor, IManifest, IModHelper, IMultiplayerHelper, IModEvents, IGameLoopEvents
{
    public string UniqueID => "test";
    public IMultiplayerHelper Multiplayer => this;
    public IModEvents Events => this;
    public IGameLoopEvents GameLoop => this;
    public event EventHandler<StardewModdingAPI.Events.UpdateTickedEventArgs>? UpdateTicked;
    internal readonly List<(object? Message, string Type, long[]? Players)> Messages = new();
    public void SendMessage<T>(
        T message,
        string type,
        string[] modIDs,
        long[]? playerIDs = null
    ) => Messages.Add((message, type, playerIDs));
    public void Log(string message, LogLevel level = LogLevel.Debug) { }
    public void LogOnce(string message, LogLevel level) { }
    internal void Tick() => UpdateTicked?.Invoke(this, new());
}
