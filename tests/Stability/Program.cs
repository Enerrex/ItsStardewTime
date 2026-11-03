using System.Reflection;
using ItsStardewTime;
using ItsStardewTime.Framework;
using ItsStardewTime.Framework.Config;
using ItsStardewTime.Framework.enums;
using StardewValley;

int passed = 0;
Run("threshold boundaries and config Update", Thresholds);
Run("zero-percent voting, disabling and notification counts", Voting);
Run("Fair intervals, repeated messages, tick zero and daily reset", Accounting);
Run("Fair selection after completed pauses and daily reset", FairSelection);
Run("host location settings and deferred polling", Locations);
Run("unchanged join and reconnect snapshots", Joining);
Run("session cache and override reset", Sessions);
Run("manual unfreeze precedence and reset", Overrides);
Run("clock progress only within the same interval", ClockSync);
Console.WriteLine($"Passed {passed} regression groups.");

void Run(string name, Action test)
{
    test();
    passed++;
    Console.WriteLine($"PASS {name}");
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new Exception($"Expected {expected}, got {actual}");
    }
}

static (PlayerStates States, ModConfig Config, Boundary Boundary) Setup()
{
    Game1.ticks = 0;
    Game1.timeOfDay = 600;
    Game1.Players.Clear();
    Game1.MasterPlayer = new() { UniqueMultiplayerID = 1 };
    Game1.Players.Add(1, Game1.MasterPlayer);
    Game1.Players.Add(2, new() { UniqueMultiplayerID = 2 });
    Boundary boundary = new();
    TimeMaster.TimeSpeed = new(boundary, boundary);
    ModConfig config = new() { RelativeTimeSpeed = false, AnyCutscenePauses = false };
    PlayerStates states = new(
        config,
        boundary,
        boundary,
        boundary
    );
    states.Add(1);
    return (states, config, boundary);
}

static bool Frozen(PlayerStates states)
{
    states.GetSharedTimeSpeedSettings(
        out _,
        out bool? manual,
        out AutoFreezeReason auto
    );
    return manual ?? auto != AutoFreezeReason.None;
}

static void Thresholds()
{
    ModConfig config = new();
    double[] thresholds =
    {
        0d,
        -1d,
        0.01,
        0.5,
        1,
        2,
        double.NaN,
        double.PositiveInfinity,
        double.NegativeInfinity,
    };
    foreach (double _threshold in thresholds)
    {
        config.VoteThresholdPercent = _threshold;
        Equal(double.PositiveInfinity, config.GetVoteThresholdCount(0));
        Equal(double.PositiveInfinity, config.GetVoteThresholdCount(-1));
        Equal(1d, config.GetVoteThresholdCount(1));
    }
    config.VoteThresholdPercent = 0;
    Equal(1d, config.GetVoteThresholdCount(4));
    config.VoteThresholdPercent = 0.51;
    Equal(3d, config.GetVoteThresholdCount(4));
    config.VoteThresholdPercent = double.NaN;
    Equal(4d, config.GetVoteThresholdCount(4));
    config.VoteThresholdPercent = -1;
    Equal(1d, config.GetVoteThresholdCount(4));
    config.VoteThresholdPercent = 2;
    Equal(4d, config.GetVoteThresholdCount(4));
    config.Update(new() { VoteThresholdPercent = 0.25, EnableVotePause = false });
    Equal(0.25, config.VoteThresholdPercent);
    Equal(false, config.EnableVotePause);
    config.Update(new()); // GMCM reset follows this path.
    Equal(1d, config.VoteThresholdPercent);
}

static void Voting()
{
    var (states, config, boundary) = Setup();
    config.PauseMode = PauseMode.Any;
    config.VoteThresholdPercent = 0;
    states.Add(2);
    Equal(false, Frozen(states));
    states.UpdateVote(1, true);
    Equal(true, Frozen(states));
    Equal("1/1", boundary.Messages.Last(m => m.Type == Messages.VoteUpdateMessage).Message);
    states.UpdateVote(1, false);
    Equal(false, Frozen(states));
    Equal("0/1", boundary.Messages.Last(m => m.Type == Messages.VoteUpdateMessage).Message);
    config.EnableVotePause = false;
    states.UpdateVote(1, true);
    Equal(false, Frozen(states));
    states.Clear();
    Game1.MasterPlayer = null!;
    Equal(false, Frozen(states));
}

static void Accounting()
{
    // Access the production private state without widening the mod's API.
    Type type = typeof(PlayerStates).GetNestedType("PlayerState", BindingFlags.NonPublic)!;
    object state = Activator.CreateInstance(type, true)!;
    void Pause(bool paused) => type.GetMethod("UpdatePauseState", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(state, new object[] { paused });
    void Reset() => type.GetMethod("DayStarted", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(state, null);
    int Total() => (int)type.GetProperty("TotalTicksPaused", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(state)!;
    void Location(AutoFreezeReason reason) => type.GetProperty("AutoFreezeReason", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(state, reason);
    Game1.ticks = 0;
    Pause(true);
    Game1.ticks = 10;
    Pause(true);
    Equal(10, Total());
    Game1.ticks = 20;
    Pause(false);
    Pause(false);
    Equal(20, Total());
    Game1.ticks = 30;
    Pause(true);
    Game1.ticks = 40;
    Pause(false);
    Equal(30, Total());
    Location(AutoFreezeReason.FrozenForLocation);
    Game1.ticks = 50;
    Pause(true);
    Location(AutoFreezeReason.None);
    Game1.ticks = 60;
    Equal(50, Total()); // Overlapping reasons form one continuous interval.
    Reset();
    Equal(0, Total());
    Game1.ticks = 70;
    Pause(false);
    Equal(10, Total());
    Reset();
    Equal(0, Total());
    Game1.ticks = 80;
    Location(AutoFreezeReason.FrozenForLocation);
    Game1.ticks = 90;
    Location(AutoFreezeReason.FrozenForLocation);
    Equal(10, Total());
    Game1.ticks = 100;
    Location(AutoFreezeReason.None);
    Equal(20, Total());
}

static void FairSelection()
{
    var (states, config, _) = Setup();
    config.EnableVotePause = false;
    states.Add(2);
    states.UpdatePauseState(1, true);
    Game1.ticks = 20;
    states.UpdatePauseState(1, false);
    states.UpdatePauseState(2, true);
    Equal(true, Frozen(states));
    Game1.ticks = 50;
    states.ForceRecompute();
    Equal(false, Frozen(states));
    states.DayStarted();
    Equal(true, Frozen(states));
}

static void Locations()
{
    var (states, config, _) = Setup();
    config.EnableVotePause = false;
    states.UpdateTimeSpeedSettings(
        1,
        tickInterval: 14000,
        autoFreeze: AutoFreezeReason.FrozenForLocation
    );
    Equal(14000, TimeMaster.TimeSpeed.TickInterval);
    Equal(true, Frozen(states));
    states.UpdateTimeSpeedSettings(
        1,
        tickInterval: 9000,
        autoFreeze: AutoFreezeReason.None,
        skipUpdate: true
    );
    states.PollForLocationUpdates(); // No remote player exists to mark modified.
    Equal(9000, TimeMaster.TimeSpeed.TickInterval);
    Equal(false, Frozen(states));
}

static void Joining()
{
    var (states, config, boundary) = Setup();
    config.PauseMode = PauseMode.Any;
    states.ForceRecompute();
    states.UpdateAllClients();
    boundary.Messages.Clear();
    states.Add(2);
    AssertSnapshot();
    states.Remove(2);
    boundary.Messages.Clear();
    states.Add(2);
    AssertSnapshot();

    void AssertSnapshot()
    {
        var snapshot = boundary.Messages.Single(m => m.Type == Messages.SetTimeSpeed && m.Players?.SequenceEqual(new long[] { 2 }) == true);
        var command = (SetTimeSpeedCommand)snapshot.Message!;
        Equal(TimeMaster.TimeSpeed.TickInterval, command.TickInterval);
        Equal(Frozen(states), command.IsTimeFrozen);
        Equal(Game1.timeOfDay, command.TimeOfDay);
    }
}

static void Sessions()
{
    var (states, _, _) = Setup();
    states.SetFreezeOverride(true);
    Equal(true, Frozen(states));
    states.Clear();
    states.Add(1);
    Equal(false, Frozen(states));
    Equal(false, TimeMaster.TimeSpeed.IsTimeFrozen);
}

static void Overrides()
{
    var (_, _, _) = Setup();
    foreach (AutoFreezeReason _reason in Enum.GetValues<AutoFreezeReason>())
    {
        foreach (bool? _manual in new bool?[] { null, false, true })
        {
            TimeMaster.TimeSpeed.UpdateTimeSpeed(
                autoFreeze: _reason,
                manualOverride: _manual,
                clearPreviousOverrides: true
            );
            SetTimeSpeedCommand command = new(
                7000,
                _manual,
                _reason
            );
            Equal(command.IsTimeFrozen, TimeMaster.TimeSpeed.IsTimeFrozen);
        }
    }
    TimeMaster.TimeSpeed.TimeHelper.SetTime(0.8);
    TimeMaster.TimeSpeed.Reset();
    Equal(false, TimeMaster.TimeSpeed.IsTimeFrozen);
    Equal(0d, TimeMaster.TimeSpeed.TimeHelper.TickProgress);
    Equal(7000, TimeMaster.TimeSpeed.TickInterval);
}

static void ClockSync()
{
    SetTimeSpeedCommand command = new() { TimeOfDay = 700, TickProgress = 0.8 };
    Equal(true, command.ShouldSyncProgress(700, 0.2));
    Equal(false, command.ShouldSyncProgress(700, 0.9));
    Equal(false, command.ShouldSyncProgress(650, 0.2));
    Equal(false, command.ShouldSyncProgress(710, 0.2));
    command.TimeOfDay = 2400;
    Equal(false, command.ShouldSyncProgress(2350, 0.2));
}
