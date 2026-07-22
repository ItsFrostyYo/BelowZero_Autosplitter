using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LiveSplit.DropPodLocation
{
    internal sealed class DropPodLogReader : IDisposable
    {
        private const string UnknownLocation = "Unknown";
        private const double CoordinateTolerance = 0.5;

        private static readonly Regex DropLocationPattern = new Regex(
            @"SupplyDropForBaseBootstrap.*?\bto dest\s*\(\s*(?<x>-?\d+(?:\.\d+)?)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private readonly string playerLogPath;
        private int processId;
        private long logPosition;
        private string currentLocation = UnknownLocation;

        public DropPodLogReader()
        {
            string localLow = Path.GetFullPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "..",
                "LocalLow"));

            playerLogPath = Path.Combine(
                localLow,
                "Unknown Worlds",
                "Subnautica Below Zero",
                "Player.log");
        }

        public string Update()
        {
            Process process = FindGameProcess();
            if (process == null)
            {
                Reset();
                return currentLocation;
            }

            try
            {
                if (process.Id != processId)
                {
                    processId = process.Id;
                    logPosition = 0;
                    currentLocation = UnknownLocation;
                }

                ReadNewLogLines();
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            finally
            {
                process.Dispose();
            }

            return currentLocation;
        }

        public void Dispose()
        {
        }

        private static Process FindGameProcess()
        {
            Process[] processes = Process.GetProcessesByName("SubnauticaZero");
            Process selected = null;

            foreach (Process process in processes)
            {
                if (selected == null && !process.HasExited)
                {
                    selected = process;
                    continue;
                }

                process.Dispose();
            }

            return selected;
        }

        private void ReadNewLogLines()
        {
            if (!File.Exists(playerLogPath))
                return;

            using (var stream = new FileStream(
                playerLogPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete))
            {
                if (stream.Length < logPosition)
                {
                    logPosition = 0;
                    currentLocation = UnknownLocation;
                }

                stream.Position = logPosition;
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        ProcessLogLine(line);

                    logPosition = stream.Position;
                }
            }
        }

        private void ProcessLogLine(string line)
        {
            if (line.IndexOf("loading scene: StartScreen in mode Single", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                currentLocation = UnknownLocation;
                return;
            }

            Match match = DropLocationPattern.Match(line);
            if (!match.Success
                || !double.TryParse(
                    match.Groups["x"].Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double x))
            {
                return;
            }

            if (IsNear(x, -136.0))
                currentLocation = "A";
            else if (IsNear(x, -275.7))
                currentLocation = "B";
            else if (IsNear(x, -327.0))
                currentLocation = "C";
        }

        private static bool IsNear(double value, double expected) =>
            Math.Abs(value - expected) <= CoordinateTolerance;

        private void Reset()
        {
            processId = 0;
            logPosition = 0;
            currentLocation = UnknownLocation;
        }
    }
}
