namespace hiveWinstreak {
    public enum HiveGameMode {
        Bedwars,
        DeathRun,
        HideAndSeek,
        MurderMystery,
        SurvivalGames,
        Skywars,
        BuildBattle,
        GroundWars,
        BlockDrop,
        CaptureTheFlag,
        TheBridge,
        BlockParty,
        Gravity,
    }

    public static class HiveGameModeExtensions {
        private static readonly Dictionary<string, HiveGameMode> QueueCodeToMode = new(StringComparer.OrdinalIgnoreCase) {
            ["Bed"] = HiveGameMode.Bedwars,
            ["Wars"] = HiveGameMode.Bedwars, // legacy "Treasure Wars" queue code
            ["DR"] = HiveGameMode.DeathRun,
            ["Hide"] = HiveGameMode.HideAndSeek,
            ["Murder"] = HiveGameMode.MurderMystery,
            ["SG"] = HiveGameMode.SurvivalGames,
            ["Sky"] = HiveGameMode.Skywars,
            ["Build"] = HiveGameMode.BuildBattle,
            ["Ground"] = HiveGameMode.GroundWars,
            ["Drop"] = HiveGameMode.BlockDrop,
            ["CTF"] = HiveGameMode.CaptureTheFlag,
            ["Bridge"] = HiveGameMode.TheBridge,
            ["Party"] = HiveGameMode.BlockParty,
            ["Grav"] = HiveGameMode.Gravity,
        };

        private static readonly HashSet<HiveGameMode> TeamModes = new() {
            HiveGameMode.Bedwars, HiveGameMode.GroundWars, HiveGameMode.CaptureTheFlag, HiveGameMode.TheBridge,
        };

        private static readonly Dictionary<string, HiveGameMode> DisplayNameToMode = Enum.GetValues<HiveGameMode>()
            .ToDictionary(m => m.ToDisplayName(), m => m, StringComparer.OrdinalIgnoreCase);

        public static bool TryParseQueueCode(string code, out HiveGameMode mode) => QueueCodeToMode.TryGetValue(code, out mode);

        public static bool TryParseGamemodeText(string text, out HiveGameMode mode) {
            text = text.Trim();
            return DisplayNameToMode.TryGetValue(text, out mode) || QueueCodeToMode.TryGetValue(text, out mode);
        }

        public static bool IsTeamMode(this HiveGameMode mode) => TeamModes.Contains(mode);

        public static string ToDisplayName(this HiveGameMode mode) => mode switch {
            HiveGameMode.Bedwars => "Bedwars",
            HiveGameMode.DeathRun => "DeathRun",
            HiveGameMode.HideAndSeek => "Hide and Seek",
            HiveGameMode.MurderMystery => "Murder Mystery",
            HiveGameMode.SurvivalGames => "Survival Games",
            HiveGameMode.Skywars => "Skywars",
            HiveGameMode.BuildBattle => "Build Battle",
            HiveGameMode.GroundWars => "Ground Wars",
            HiveGameMode.BlockDrop => "Block Drop",
            HiveGameMode.CaptureTheFlag => "Capture The Flag",
            HiveGameMode.TheBridge => "The Bridge",
            HiveGameMode.BlockParty => "Block Party",
            HiveGameMode.Gravity => "Gravity",
            _ => mode.ToString(),
        };
    }
}
