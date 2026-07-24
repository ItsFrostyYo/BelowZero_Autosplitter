using System;

namespace LiveSplit.BelowZero.RichPresence
{
    internal static class RichPresenceConfiguration
    {
        public const string ApplicationId = "1530113548875857980";

        public const string DefaultLeaderboardUrl =
            "https://www.speedrun.com/subnautica_below_zero";

        // Below Zero displays these leaderboard variables in this order.
        public static readonly string[] LeaderboardVariableOrder =
        {
            "Game Mode",
            "Run Type"
        };

        // Slightly over four seconds gives us some safety around Discord's
        // five-presence-updates-per-20-seconds limit.
        public static readonly TimeSpan RefreshInterval =
            TimeSpan.FromMilliseconds(4200);
    }
}