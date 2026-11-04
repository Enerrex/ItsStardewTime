# Stability verification

Automated checks passed; the live-game checks below have **not** been performed.
Run the same build on the host and farmhands and collect SMAPI debug logs for any
failure. Record pause mode, vote threshold, RelativeTimeSpeed, location freeze
settings, player count, and the final logged pause reason.

## Automated checks

```powershell
dotnet build ItsStardewTime.csproj -p:EnableModDeploy=false -p:EnableModZip=false
dotnet run --project tests/Stability/Stability.csproj
```

The console runner exits unsuccessfully on an assertion failure. Its eleven groups
compile the production PlayerStates, ModConfig, TimeSpeed, TimeHelper, buff duration locks and settings
message code. Boundary doubles supply game state, configuration location lookup,
logging, notifications and message transport; they do not reimplement the tested
algorithms. The private player counter is exercised through reflection to avoid
exposing it in the mod API. Tests do not simulate graphics, game updates, actual
SMAPI dispatch, network delivery or protection effects.

Coverage includes zero votes at 0%, one-vote success, vote withdrawal, disabled
voting, notification denominators, empty populations, invalid thresholds, config
copy/reset, repeated pause messages, tick-zero starts, retained completed intervals,
multiple and overlapping activity/location intervals, day reset and Fair selection,
host settings updates and pending polling without remote modifications, unchanged
join/reconnect snapshots, session cache/override cleanup, manual-unfreeze precedence,
and progress synchronization within matching clock intervals.

Finite thresholds outside [0, 1] are clamped to that range. Nonfinite values use
unanimity. Nonpositive populations cannot pass a vote. The stored property and its
fractional representation remain unchanged for config compatibility. A threshold
of zero requires one affirmative vote; EnableVotePause remains the disabling control.

## Evidence and scope

Local game assembly inspected: Stardew Valley 1.6.15.24356; SMAPI 4.5.1. Inspection
used Mono.Cecil to read method instructions without running the game.

- `Game1.shouldTimePass(true)` already reads `Game1.paused` (IL_0044), then
  freezeControls, overlayMenu, isTimePaused, eventUp, activeClickableMenu and
  Farmer.CanMove/UsingTool/forceTimePass. The unfinished extra paused condition was
  redundant and was removed. This mod doesn't write paused or IsTimePaused, so no
  feedback path through its own freeze flag was found.
- `SteamHelper.onGameOverlayActivated` sets paused for a single-player overlay
  and clears it when the overlay closes. `Game1.ResetGameStateOnTitleScreen` clears
  it too. `Game1.Update` computes the network IsTimePaused flag from native player
  pause requests in local multiplayer; the existing mod check is retained.
- `Horse.update` clears mounting/dismounting and restores rider.canMove (IL_0296
  and IL_05e1); `Horse.dismount` also restores movement. This establishes a normal
  transition completion path, not proof that every game/mod combination completes it.
- Mod pause reasons are derived anew each update. Event and request transitions
  send both activation and deactivation. No independent sticky menu/horse/event
  reason has been established. A report with a nonzero threshold needs its own log
  and reproduction; the confirmed 0% defect must not be assumed to explain it.
- SMAPI's installed XML documentation says PeerContextReceived is the earliest
  message-capable event and PeerConnected occurs after approval. Initialization
  remains at PeerConnected and now sends a targeted current settings snapshot even
  without an aggregate change. Real connection/load timing remains a live check.
- TimeSpeed previously ignored explicit false while SetTimeSpeedCommand honored
  it. Explicit unfreeze now has the same precedence in both. Existing override
  expiry rules are unchanged; whether a move between two different frozen locations
  should expire it is a separate behavior decision (the hotkey tooltip suggests
  expiry on changed location settings).
- Fair accounting now counts the same combined pause state already used to choose
  Fair players and compute RelativeTimeSpeed: activity requests and automatic
  location/time freezes. Overlapping reasons count once. Votes are separate.
- Clear previously retained cached settings and the host override, and there was
  no return-to-title cleanup for protection snapshots or TimeHelper progress.
  Cleanup now resets these values for all active screens without replacing
  TimeSpeed instances or duplicating their event subscriptions.
- Progress belongs to its TimeOfDay interval. A settings message for another
  interval now leaves progress alone until native world synchronization catches up.
  Snapshots taken before the helper observes a native boundary report zero progress
  for the new interval. A farmhand accepting newer progress records the interval
  immediately. The regression constructs a packet at midnight and advances the
  receiving helper through its next update.
- On a normal secondary-screen exit, SMAPI's `SGame.CleanupReturningToTitle`
  enters the ReturningToTitle stage. The base game's `Game1.CleanupReturningToTitle`
  queues only that instance for removal (IL_004e–0054). `GameRunner.Update` removes
  it after updating the instances in that frame (IL_01f7–023e). SMAPI raises
  `ReturnedToTitle` on a subsequent update observing the unloaded stage, which the
  removed instance doesn't receive. The host's peer-disconnect handler removes
  the departing player. A regression exercises that removal with an active host
  override and checks that the host can still change its location settings.
  In split-screen, also quit just the farmhand while the host stays in the save,
  then reconnect and repeat. This sequence still needs live verification.
- Voting tests cover the float supplied by GMCM for 60% with five players and
  a hand-edited double just above the same threshold. Buff tests acquire a second
  timed buff during a freeze and check its duration on the next update.

Pause/event detection, immediate unfreeze cleanup, health protection and buff
restoration run every update. NPC/animal/monster movement maintenance runs on a
freeze transition and then every seven ticks, preserving its original cadence and
250 ms lock timers. Fair reevaluation retains its 70-tick cadence.

## Live verification matrix

Use single-player, a remote host/farmhand pair, and two local split-screen players.
In multiplayer repeat with each player initiating the transition. Keep existing
policy defaults (Fair, AnyCutscenePauses and RelativeTimeSpeed) for the baseline,
then isolate other policies as indicated. Check actual clock progression as well
as the pause X and debug messages.

| Scenario | Checks and expected result |
| --- | --- |
| Voting | At 0%, no votes never freezes an otherwise active game; one vote pauses and withdrawal resumes. Check 1%, 50%, 100%, disabled voting and changing thresholds through GMCM. Denominators match the required count. Test each existing pause policy separately to distinguish policy pauses from voting. |
| GMCM reset | Change threshold, save, reset, save again and reopen. Threshold returns to 100%; other config fields still reset. Reload a hand-edited config too. |
| Locations | Warp repeatedly indoors/outdoors, into mines/Skull Cavern and custom freeze locations. Host-only sessions apply speed/freeze changes immediately. In multiplayer test Host/Average/Min/Max time modes and remote polling; leaving the last freezing location clears that reason. |
| Joining/reconnecting | Join an active game with unchanged default settings, changed speed, a manual freeze/unfreeze, a location freeze and a successful vote. Reconnect in each state. Farmhand receives current settings, vote state and monster-lock policy without requiring a subsequent host change. Repeat for a second split-screen player. |
| Fair accounting | Alternate long and short menus between players, repeat messages/transitions, overlap menu and location freezes, and leave both. The least accumulated requester controls Fair mode; completed intervals still count. Sleep and confirm the new day starts a new balance, including an active request across day start. |
| Menus and horses | Open/close inventory, dialogue, shops, map and fishing UI; mount/dismount repeatedly, including near a warp and during another player's pause. The activation log must be followed by an unpause log when all native conditions clear. Repeat with 0% and nonzero thresholds. |
| Cutscenes | Enter, finish and skip events, festivals and minigames. Test AnyCutscenePauses on/off without changing normal pause policies. Ensure event flags clear after exit; ordinary menus retain their current policy behavior. |
| Session resets | Return to title while voted, manually frozen, location frozen and protected. Load the same save and a different save, reconnect as farmhand, and recreate split-screen. No old vote, override, tick progress, monster position, health or buff snapshot carries over. |
| Clock boundaries | Compare host/farmhand around :50 to the next hour, 23:50 to 24:00, overnight and day start; include differing network clock intervals and paused boundaries. Settings packets must not apply prior/next-interval progress to the current interval or move the display backwards within an interval. Check 12/24-hour displays and minute display. |
| Objects | With ObjectsPassTimeWhenTimeIsFrozen off, paused machines don't advance. With it on, machines advance once per virtual ten-minute interval, including building interiors; unpausing does not create duplicate processing. Check speed changes while frozen. |
| Protection | Freeze near monsters and moving NPCs/animals. Check movement locks and health with LockMonsters on/off, healing during pause, food/drink/other buffs and invincibility cleanup after resume. Repeat on host, remote farmhand and both local screens. Buff time and protected health must not decay during a freeze, nor restore an old session's values. |

The cutscene-only multiplayer request is deferred: it needs an explicit decision
about ordinary activity pause requests and RelativeTimeSpeed. No policy defaults
were changed in this stability work.
