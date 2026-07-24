using DiscordRPC;
using LiveSplit.Model;
using System;
using Voxif.IO;

namespace LiveSplit.BelowZero.RichPresence
{
    internal sealed class BelowZeroRichPresence : IDisposable
    {
        private readonly LiveSplitState state;
        private readonly BelowZeroMemory memory;
        private readonly BelowZeroSettings settings;
        private readonly Func<bool> isSurvivalStartArmed;
        private readonly Logger logger;

        private DiscordRpcClient client;

        private DateTime nextAllowedUpdateUtc = DateTime.MinValue;
        private string lastPayloadKey = string.Empty;
        private string lastLoggedStatusKey = string.Empty;
        private bool presenceVisible;
        private bool disposed;

        public BelowZeroRichPresence(
            LiveSplitState state,
            BelowZeroMemory memory,
            BelowZeroSettings settings,
            Func<bool> isSurvivalStartArmed,
            Logger logger)
        {
            this.state = state;
            this.memory = memory;
            this.settings = settings;
            this.isSurvivalStartArmed =
                isSurvivalStartArmed ?? (() => false);
            this.logger = logger;
        }

        public void Update()
        {
            if (disposed)
                return;

            if (!settings.DiscordStatusEnabled)
            {
                Stop("disabled in component settings");
                return;
            }

           DateTime nowUtc = DateTime.UtcNow;

            // Avoid calculating category text, leaderboard URLs, comparison
            // deltas, and memory statuses between Discord refreshes.
            if (nowUtc < nextAllowedUpdateUtc)
              return;

            DateTime? processStartTimeUtc =
              memory.ProcessStartTimeUtc;

            bool localGameProcessFound =
                processStartTimeUtc.HasValue;

            // Rich Presence can operate from LiveSplit alone when Below Zero
            // is running on a console or another computer. Game-memory-only
            // statuses are simply unavailable in that fallback mode.
            PresenceData data =
                CreatePresenceData(localGameProcessFound);

            // With a local process, Discord's separate elapsed timer is the
            // total time Below Zero has been open. In manual/remote mode there
            // is no trustworthy game launch time, so the timestamp is omitted.
            DateTime? activityStartTimeUtc =
                localGameProcessFound
                    ? processStartTimeUtc
                    : null;

            string payloadKey =
                data.Details + "\n" +
                data.State + "\n" +
                data.LeaderboardUrl + "\n" +
                (activityStartTimeUtc?.Ticks ?? 0L);

            // Static statuses do not need repeated identical updates.
            // Discord advances a supplied elapsed timestamp itself.
            if (presenceVisible &&
                payloadKey == lastPayloadKey)
            {
                nextAllowedUpdateUtc =
                    nowUtc.Add(
                        RichPresenceConfiguration.RefreshInterval);

                return;
            }

            try
            {
                EnsureClient();

                Button[] buttons = null;

                if (!string.IsNullOrWhiteSpace(
                    data.LeaderboardUrl))
                {
                    buttons = new[]
                    {
                        new Button
                        {
                            Label = "View Leaderboard",
                            Url = data.LeaderboardUrl
                        }
                    };
                }

                client.SetPresence(
                    new DiscordRPC.RichPresence
                    {
                        Details =
                            RichPresenceFormatter.ClampDiscordText(
                                data.Details),

                        State =
                            RichPresenceFormatter.ClampDiscordText(
                                data.State),

                        Timestamps =
                            activityStartTimeUtc.HasValue
                                ? new Timestamps(
                                    activityStartTimeUtc.Value)
                                : null,

                        Buttons = buttons
                    });

                presenceVisible = true;
                lastPayloadKey = payloadKey;

                nextAllowedUpdateUtc =
                    nowUtc.Add(
                        RichPresenceConfiguration.RefreshInterval);

                LogStatusTransition(data);
            }
            catch (Exception ex)
            {
                logger?.Log(
                    "Discord Rich Presence update failed: " + ex);

                DisposeClient();
                ResetTracking();

                nextAllowedUpdateUtc =
                    nowUtc.Add(
                        RichPresenceConfiguration.RefreshInterval);
            }   
        }

        public void Stop()
        {
            Stop("stopped");
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            Stop("component disposed");
        }

        private void Stop(string reason)
        {
            if (client != null)
            {
                try
                {
                    if (presenceVisible)
                        client.ClearPresence();
                }
                catch (Exception ex)
                {
                    logger?.Log(
                        "Discord Rich Presence clear failed: " +
                        ex.Message);
                }

                DisposeClient();
            }

            ResetTracking();
            LogClearTransition(reason);
        }

        private void EnsureClient()
        {
            if (client != null)
                return;

            client = new DiscordRpcClient(
                RichPresenceConfiguration.ApplicationId);

            client.Initialize();

            logger?.Log(
                "Discord Rich Presence client initialized.");
        }

        private PresenceData CreatePresenceData(
            bool localGameProcessFound)
        {
            // Main-menu detection only exists when the local game process and
            // Below Zero pointers are available. It keeps priority over the
            // LiveSplit timer to preserve Reset on Main Menu behavior.
            if (localGameProcessFound &&
                memory.pointersInitialized &&
                memory.isInMainMenu)
            {
                return new PresenceData(
                    mode: "Main Menu",
                    details: "On the Main Menu",
                    stateText: "Preparing for a run",
                    leaderboardUrl: null);
            }

            string categoryName =
                RichPresenceFormatter.GetCategoryName(
                    state.Run);

            string leaderboardUrl =
                RichPresenceFormatter.GetLeaderboardUrl(
                    state.Run);

            if (state.CurrentPhase == TimerPhase.Running ||
                state.CurrentPhase == TimerPhase.Paused)
            {
                string runTime =
                    RichPresenceFormatter.FormatRunTime(
                        state.CurrentTime.RealTime);

                TimeSpan? comparisonDifference =
                    GetComparisonDifference();

                string stateText = "Time: " + runTime;

                // Do not display "| 0" merely because no comparison delta
                // exists yet. A real zero-valued delta is still displayed.
                if (comparisonDifference.HasValue)
                {
                    stateText +=
                        " | " +
                        RichPresenceFormatter.FormatDifference(
                            comparisonDifference);
                }

                return new PresenceData(
                    mode:
                        localGameProcessFound
                            ? "Running"
                            : "Running (LiveSplit only)",
                    details: "Running - " + categoryName,
                    stateText: stateText,
                    leaderboardUrl: leaderboardUrl);
            }

            if (state.CurrentPhase == TimerPhase.Ended)
            {
                string finalTime =
                    RichPresenceFormatter.FormatRunTime(
                        state.CurrentTime.RealTime);

                TimeSpan? comparisonDifference =
                    GetComparisonDifference();

                string stateText =
                    "Finished - " +
                    finalTime;

                if (comparisonDifference.HasValue)
                {
                    stateText +=
                        " | " +
                        RichPresenceFormatter.FormatDifference(
                            comparisonDifference);
                }

                return new PresenceData(
                    mode:
                        localGameProcessFound
                            ? "Finished"
                            : "Finished (LiveSplit only)",
                    details: categoryName,
                    stateText: stateText,
                    leaderboardUrl: leaderboardUrl);
            }

            // Without a local game process, LiveSplit is the only available
            // signal. Main Menu and Loading cannot be detected, so the idle
            // fallback is explicitly unknown.
            if (!localGameProcessFound)
            {
                return new PresenceData(
                    mode: "Unknown (LiveSplit only)",
                    details: "Unknown / Timer not Running",
                    stateText: "No local game process detected",
                    leaderboardUrl: null);
            }

            // Running and finished states have already been handled above,
            // so loading can only replace the timer-not-running state.
            // Loading or cinematic signals during an active run are ignored.
            bool loadingScreenActive =
                memory.pointersInitialized &&
                (memory.IsLoadingScreenShowing?.New ?? false);

            bool introCinematicActive =
                memory.pointersInitialized &&
                (memory.IsIntroCinematicActive?.New ?? false);

            // Survival Start remains armed after the intro cinematic ends
            // while it waits for real player movement or the intro-complete
            // fallback. This prevents a gap before the timer actually starts.
            bool waitingForSurvivalStart =
                settings.IntroStart &&
                isSurvivalStartArmed();

            if (loadingScreenActive ||
                introCinematicActive ||
                waitingForSurvivalStart)
            {
                return new PresenceData(
                    mode: "Loading",
                    details: "Loading",
                    stateText: "On a Loading Screen",
                    leaderboardUrl: null);
            }

            // On the first launch main menu, the Player-dependent
            // ItemGroup.items field is not available yet. The memory
            // initializer waits at that exact lookup until a save is loaded.
            // Since all timer/loading states above are false here, treat this
            // specific in-progress lookup as the otherwise-undetectable first
            // main menu. A failed or completed lookup clears the flag.
            if (memory.IsWaitingForItemGroupItemsField)
            {
                return new PresenceData(
                    mode: "Main Menu (Initial Pointer Wait)",
                    details: "On the Main Menu",
                    stateText: "Preparing for a run",
                    leaderboardUrl: null);
            }

            // IntroRespawnLocation and Weather_IntroReboot are emitted in the
            // exact gap between the initial load and the opening cinematic.
            // Keep Loading through that transition even when player input is
            // temporarily false. IntroComplete ends this special window.
            if (memory.IsIntroSetupTransitionActive)
            {
                return new PresenceData(
                    mode: "Loading (Intro Setup Goals)",
                    details: "Loading",
                    stateText: "On a Loading Screen",
                    leaderboardUrl: null);
            }

            // At this point the timer is not running, the player is not on
            // the detected main menu, no loading screen or intro cinematic is
            // active, and Survival Start is not armed.
            bool playerInputEnabled =
                memory.pointersInitialized &&
                (memory.PlayerControllerInputEnabled?.New ?? false);

            // Input alone is too broad because it remains enabled during
            // ordinary gameplay. The early loading-to-intro transition is
            // identified more narrowly by having at most one relevant story
            // goal. IntroRespawnLocation and Weather_IntroReboot are excluded
            // because they are automatic setup goals for this transition.
            // Before story-goal tracking initializes, the filtered count is
            // zero, which intentionally keeps the startup edge case Loading.
            bool earlyStoryProgress =
                memory.RichPresenceCompletedStoryGoalCount <= 1;

            if (playerInputEnabled &&
                earlyStoryProgress)
            {
                return new PresenceData(
                    mode: "Loading (Early Story Input)",
                    details: "Loading",
                    stateText: "On a Loading Screen",
                    leaderboardUrl: null);
            }

            // With more than one completed story goal—or with input disabled—
            // this is normal idle play or another state we cannot currently
            // identify more precisely.
            return new PresenceData(
                mode: "Lingering",
                details: "Lingering / Timer not Running",
                stateText: "Inside a Save File or Unknown",
                leaderboardUrl: null);
        }

        private TimeSpan? GetComparisonDifference()
        {
            if (state.Run == null ||
                state.Run.Count == 0 ||
                string.IsNullOrWhiteSpace(
                    state.CurrentComparison))
            {
                return null;
            }

            try
            {
                if (state.CurrentPhase == TimerPhase.Ended)
                {
                    return LiveSplitStateHelper.GetLastDelta(
                        state,
                        state.Run.Count - 1,
                        state.CurrentComparison,
                        TimingMethod.RealTime);
                }

                int splitIndex = state.CurrentSplitIndex;

                if (splitIndex < 0 ||
                    splitIndex >= state.Run.Count)
                {
                    return null;
                }

                // When the runner has fallen behind the comparison's next
                // split time, this supplies a live growing delta.
                TimeSpan? liveDifference =
                    LiveSplitStateHelper.CheckLiveDelta(
                        state,
                        true,
                        state.CurrentComparison,
                        TimingMethod.RealTime);

                if (liveDifference.HasValue)
                    return liveDifference;

                // Otherwise display the overall delta from the latest
                // completed split.
                return LiveSplitStateHelper.GetLastDelta(
                    state,
                    splitIndex - 1,
                    state.CurrentComparison,
                    TimingMethod.RealTime);
            }
            catch (Exception ex)
            {
                logger?.Log(
                    "Could not calculate Discord comparison delta: " +
                    ex.Message);

                return null;
            }
        }

        private void LogStatusTransition(
            PresenceData data)
        {
            string statusKey =
                "status|" +
                data.Mode + "|" +
                data.Details + "|" +
                data.LeaderboardUrl;

            if (statusKey == lastLoggedStatusKey)
                return;

            string message =
                "Discord Rich Presence status: " +
                data.Mode +
                " | " +
                data.Details;

            if (!string.IsNullOrWhiteSpace(
                data.LeaderboardUrl))
            {
                message +=
                    " | Leaderboard: " +
                    data.LeaderboardUrl;
            }

            logger?.Log(message);
            lastLoggedStatusKey = statusKey;
        }

        private void LogClearTransition(string reason)
        {
            string statusKey =
                "cleared|" + reason;

            if (statusKey == lastLoggedStatusKey)
                return;

            logger?.Log(
                "Discord Rich Presence cleared: " +
                reason +
                ".");

            lastLoggedStatusKey = statusKey;
        }

        private void DisposeClient()
        {
            try
            {
                client?.Dispose();
            }
            catch (Exception ex)
            {
                logger?.Log(
                    "Discord Rich Presence disposal failed: " +
                    ex.Message);
            }

            client = null;
        }

        private void ResetTracking()
        {
            presenceVisible = false;
            lastPayloadKey = string.Empty;
            nextAllowedUpdateUtc = DateTime.MinValue;
        }

        private sealed class PresenceData
        {
            public string Mode { get; }
            public string Details { get; }
            public string State { get; }
            public string LeaderboardUrl { get; }

            public PresenceData(
                string mode,
                string details,
                string stateText,
                string leaderboardUrl)
            {
                Mode = mode;
                Details = details;
                State = stateText;
                LeaderboardUrl = leaderboardUrl;
            }
        }
    }
}