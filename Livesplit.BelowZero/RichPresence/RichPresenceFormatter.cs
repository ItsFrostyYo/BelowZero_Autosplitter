using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit.BelowZero.RichPresence
{
    internal static class RichPresenceFormatter
    {
        public static string FormatRunTime(TimeSpan? value)
        {
            if (!value.HasValue)
                return "0:00";

            long totalSeconds =
                (long)Math.Floor(Math.Max(0, value.Value.TotalSeconds));

            return FormatUnsignedSeconds(totalSeconds);
        }

        public static string FormatDifference(TimeSpan? value)
        {
            if (!value.HasValue)
                return "0";

            long totalSeconds =
                (long)Math.Truncate(value.Value.TotalSeconds);

            if (totalSeconds == 0)
                return "0";

            string sign = totalSeconds < 0 ? "-" : "+";
            long absoluteSeconds = Math.Abs(totalSeconds);

            return sign + FormatUnsignedSeconds(absoluteSeconds);
        }

        public static string GetCategoryName(IRun run)
        {
            if (run == null)
                return "Unknown Category";

            string categoryName =
                GetCategoryName(run.CategoryName);

            List<string> subcategories =
                GetOrderedVariableValues(run).ToList();

            if (subcategories.Count == 0)
                return categoryName;

            return categoryName +
                " (" +
                string.Join(", ", subcategories) +
                ")";
        }

        public static string GetCategoryName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return "Unknown Category";

            return categoryName.Trim();
        }

        public static string GetLeaderboardUrl(IRun run)
        {
            if (run == null ||
                string.IsNullOrWhiteSpace(run.CategoryName))
            {
                return RichPresenceConfiguration.DefaultLeaderboardUrl;
            }

            var parts = new List<string>();
            string categorySlug =
                ToSpeedrunLeaderboardSlug(run.CategoryName);

            if (!string.IsNullOrEmpty(categorySlug))
                parts.Add(categorySlug);

            foreach (string value in GetOrderedVariableValues(run))
            {
                string valueSlug =
                    ToSpeedrunLeaderboardSlug(value);

                if (!string.IsNullOrEmpty(valueSlug))
                    parts.Add(valueSlug);
            }

            if (parts.Count == 0)
                return RichPresenceConfiguration.DefaultLeaderboardUrl;

            string leaderboardSelection =
                string.Join("-", parts);

            return RichPresenceConfiguration.DefaultLeaderboardUrl +
                "?h=" +
                Uri.EscapeDataString(leaderboardSelection);
        }

        public static string ClampDiscordText(string value)
        {
            const int maximumLength = 128;

            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            value = value.Trim();

            if (value.Length <= maximumLength)
                return value;

            return value.Substring(0, maximumLength);
        }

        private static IEnumerable<string> GetOrderedVariableValues(
            IRun run)
        {
            if (run?.Metadata?.VariableValueNames == null ||
                run.Metadata.VariableValueNames.Count == 0)
            {
                yield break;
            }

            var usedNames =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (
                string variableName
                in RichPresenceConfiguration.LeaderboardVariableOrder)
            {
                if (run.Metadata.VariableValueNames.TryGetValue(
                    variableName,
                    out string value) &&
                    !string.IsNullOrWhiteSpace(value))
                {
                    usedNames.Add(variableName);
                    yield return value.Trim();
                }
            }

            foreach (
                KeyValuePair<string, string> variable
                in run.Metadata.VariableValueNames)
            {
                if (usedNames.Contains(variable.Key) ||
                    string.IsNullOrWhiteSpace(variable.Value))
                {
                    continue;
                }

                yield return variable.Value.Trim();
            }
        }

        private static string ToSpeedrunLeaderboardSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder();
            bool pendingSeparator = false;

            foreach (char character in value.Trim())
            {
                // Speedrun.com's readable selector drops the percentage
                // sign: "Any%" becomes "Any".
                if (character == '%')
                    continue;

                if (char.IsLetterOrDigit(character))
                {
                    if (pendingSeparator && builder.Length > 0)
                        builder.Append('-');

                    builder.Append(character);
                    pendingSeparator = false;
                }
                else
                {
                    pendingSeparator = true;
                }
            }

            return builder.ToString().Trim('-');
        }

        private static string FormatUnsignedSeconds(long totalSeconds)
        {
            long hours = totalSeconds / 3600;
            long minutes = (totalSeconds % 3600) / 60;
            long seconds = totalSeconds % 60;

            if (hours > 0)
            {
                return string.Format(
                    "{0}:{1:00}:{2:00}",
                    hours,
                    minutes,
                    seconds);
            }

            return string.Format(
                "{0}:{1:00}",
                minutes,
                seconds);
        }
    }
}
