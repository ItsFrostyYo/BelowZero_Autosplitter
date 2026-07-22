using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace LiveSplit.BelowZero
{
    public static class Localization
    {
        private static IReadOnlyDictionary<string, string> _translations = new Dictionary<string, string>();
        private static IReadOnlyDictionary<string, string> _translationsIgnoreCase =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private const string ResourcePath = "LiveSplit.BelowZero.Resources.English.json";

        private static string StripJsonComments(string s)
        {
            s = Regex.Replace(s, @"^\s*//.*$", "", RegexOptions.Multiline);
            s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
            return s;
        }

        public static void Load()
        {
            var asm = Assembly.GetExecutingAssembly();
            using (Stream stream = asm.GetManifestResourceStream(ResourcePath))
            {
                if (stream == null) throw new FileNotFoundException("Embedded resource not found: " + ResourcePath);
                using (var sr = new StreamReader(stream, Encoding.UTF8, true))
                {
                    string json = sr.ReadToEnd();
                    json = StripJsonComments(json);
                    var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                    var dict = serializer.Deserialize<Dictionary<string, string>>(json);
                    _translations = dict ?? new Dictionary<string, string>();

                    var translationsIgnoreCase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, string> entry in _translations)
                    {
                        if (!translationsIgnoreCase.ContainsKey(entry.Key))
                            translationsIgnoreCase.Add(entry.Key, entry.Value);
                    }
                    _translationsIgnoreCase = translationsIgnoreCase;
                }
            }
        }

        public static string GetDisplayName(object key)
        {
            if (_translations == null)
                throw new InvalidOperationException("Translations not loaded.");

            var keyString = key.ToString();

            if (_translations.TryGetValue(keyString, out var value))
                return value;

            if (_translations.TryGetValue("Ency_" + keyString, out value)
                || _translations.TryGetValue("Log_" + keyString, out value)
                || _translations.TryGetValue("EncyPath_" + keyString, out value))
                return value;

            if (_translationsIgnoreCase.TryGetValue(keyString, out value))
                return value;

            if (_translationsIgnoreCase.TryGetValue("Ency_" + keyString, out value)
                || _translationsIgnoreCase.TryGetValue("Log_" + keyString, out value)
                || _translationsIgnoreCase.TryGetValue("EncyPath_" + keyString, out value))
                return value;

            return Regex.Replace(keyString.Replace('_', ' '), "(?<=[a-z0-9])([A-Z])", " $1");
        }
    }
}

