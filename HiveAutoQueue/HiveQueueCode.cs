using hiveWinstreak;

namespace HiveAutoQueue {
    public static class HiveQueueCode {
        public const string Hub = "HUB";
        public const string Arcade = "ARCADE";

        public static bool IsLobby(string rawCode) =>
            rawCode.Equals(Hub, StringComparison.OrdinalIgnoreCase) || rawCode.Equals(Arcade, StringComparison.OrdinalIgnoreCase);

        public static (HiveGameMode? Mode, string? Variant) Parse(string rawCode) {
            var dashIndex = rawCode.IndexOf('-');
            var baseCode = dashIndex < 0 ? rawCode : rawCode[..dashIndex];
            var variant = dashIndex < 0 ? null : rawCode[(dashIndex + 1)..];
            return HiveGameModeExtensions.TryParseQueueCode(baseCode, out var mode) ? (mode, variant) : (null, variant);
        }

        public static string FormatDisplay(string rawCode) {
            if (string.IsNullOrEmpty(rawCode)) return "§7Unknown";
            if (IsLobby(rawCode)) return rawCode.Equals(Hub, StringComparison.OrdinalIgnoreCase) ? "§eHub" : "§eArcade Hub";

            var (mode, variant) = Parse(rawCode);
            var baseName = mode?.ToDisplayName() ?? rawCode;
            if (string.IsNullOrEmpty(variant)) return $"§b{baseName}";

            var variantName = string.Join(' ', variant.Split('-').Select(FormatWord));
            return $"§b{baseName}§8: §7{variantName}";
        }

        private static string FormatWord(string word) =>
            word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
    }
}
