using System.Text.RegularExpressions;
using hiveWinstreak;
using HiveAutoQueue.Commands;
using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.UI;
using OnixRuntime.Plugin;

namespace HiveAutoQueue {
    public class HiveAutoQueue : OnixPluginBase {
        private const string GameSelectorItemName = "§r§bGame Selector§7 [Use]";
        private static readonly Regex ServerNameCodePattern = new("^[A-Za-z-]*", RegexOptions.Compiled);

        public static HiveAutoQueue Instance { get; private set; } = null!;
        public static HiveAutoQueueConfig Config { get; private set; } = null!;

        public string LastGamemodeRaw { get; private set; } = "";

        private string _team = "Unknown";
        private bool _warnedLanguage;

        public HiveAutoQueue(OnixPluginInitInfo initInfo) : base(initInfo) {
            Instance = this;
            // If you can clean up what the plugin leaves behind manually, please do not unload the plugin when disabling.
            base.DisablingShouldUnloadPlugin = false;
#if DEBUG
           // base.WaitForDebuggerToBeAttached();
#endif
        }

        protected override void OnLoaded() {
            Console.WriteLine($"Plugin {CurrentPluginManifest.Name} loaded!");
            Config = new HiveAutoQueueConfig(PluginDisplayModule, true);
            Onix.Client.CommandRegistry.RegisterCommand(new RequeueCommand());
        }

        protected override void OnEnabled() {
            Onix.Events.Session.Tick += OnTick;
            Onix.Events.Session.Chat.Receive += OnChatReceive;
            Onix.Events.Input.Input += OnInput;
        }

        protected override void OnDisabled() {
            Onix.Events.Session.Tick -= OnTick;
            Onix.Events.Session.Chat.Receive -= OnChatReceive;
            Onix.Events.Input.Input -= OnInput;
        }

        protected override void OnUnloaded() {
            // Ensure every task or thread is stopped when this function returns.
            // You can give them base.PluginEjectionCancellationToken which will be cancelled when this function returns.
            Console.WriteLine($"Plugin {CurrentPluginManifest.Name} unloaded!");
        }

        private void OnTick() {
            if (!Onix.Game.LanguageCode.Contains("en", StringComparison.OrdinalIgnoreCase)) {
                if (!_warnedLanguage) {
                    Onix.Client.Notify("Hive Autoqueue", "This plugin only supports English.");
                    _warnedLanguage = true;
                }
                return;
            }

            var localPlayer = Onix.LocalPlayer;
            if (localPlayer is null) return;

            var heldItem = localPlayer.Inventory.GetItem(1);
            if (!heldItem.IsEmpty && heldItem.CustomName == GameSelectorItemName) {
                LastGamemodeRaw = HiveQueueCode.Hub;
            }
        }

        private bool OnChatReceive(string message, string username, string xuid, ChatMessageType type) {
            var localPlayer = Onix.LocalPlayer;

            if (message.Contains("§b§l» §r§a§lVoting has ended!")) {
                Onix.Game.ExecuteCommand("/connection");
            }

            if (message.Contains(" joined. §8") && localPlayer is not null && message.Contains(localPlayer.Username)) {
                Onix.Game.ExecuteCommand("/connection");
            }

            const string serverNamePrefix = "You are connected to server name ";
            var serverNameIndex = message.IndexOf(serverNamePrefix, StringComparison.Ordinal);
            if (serverNameIndex >= 0) {
                var rest = message[(serverNameIndex + serverNamePrefix.Length)..];
                LastGamemodeRaw = ServerNameCodePattern.Match(rest).Value;
            }

            if (message.Contains("You are connected to proxy ") ||
                message.Contains("You are connected to server ") ||
                message.Contains("You are connected to public IP ") ||
                message.Contains("You are connected to internal IP ") ||
                message.Contains("§cYou're issuing commands too quickly, try again later.") ||
                message.Contains("§cUnknown command. Sorry!")) {
                return true;
            }

            if (message.Contains("§rYou are on the ") && message.Length >= 30) {
                _team = message[29] switch {
                    'e' => "§eYellow",
                    'a' => "§aLime",
                    'c' => "§cRed",
                    '9' => "§9Blue",
                    '6' => "§6Gold",
                    'd' => "§dMagenta",
                    'b' => "§bAqua",
                    '7' => "§7Gray",
                    '5' => "§5Purple",
                    '2' => "§2Green",
                    '8' => "§8Dark Gray",
                    '3' => "§3Cyan",
                    _ => "Unknown",
                };
            }

            HandleGameEndMessages(message, localPlayer);
            return false;
        }

        private void HandleGameEndMessages(string message, LocalPlayer? localPlayer) {
            if (localPlayer is null) return;

            var (mode, _) = HiveQueueCode.Parse(LastGamemodeRaw);
            var localUsername = localPlayer.Username;

            if (mode?.IsTeamMode() ?? false) {
                if (message.Contains($"{_team} Team §7has been §cELIMINATED§7!")) {
                    if (message.Contains(localUsername) && message.Contains(" did an oopsie!") && mode == HiveGameMode.Skywars) {
                        Requeue(LastGamemodeRaw, "You did an oopsie??", true);
                    } else {
                        Requeue(LastGamemodeRaw, "Unfortulately you lost.", true);
                    }
                    return;
                }
                if (message.Contains($"{_team} was ELIMINATED!")) {
                    Requeue(LastGamemodeRaw, "Unfortulately you lost.", true);
                    return;
                }
                if (message.Contains($"{_team} Team are the WINNERS!") || message.Contains($"{_team} Team is the WINNER!")) {
                    Requeue(LastGamemodeRaw, "Congratulations on winning! <3", true);
                    return;
                }
            }

            if (message.Contains("§a§l» §r§eYou finished in §f")) {
                Requeue(LastGamemodeRaw, "Wow, you did something.", true);
                return;
            }

            // murder mystery
            if (message.Contains("§c§l» §r§cYou died! §7§oYou will be taken to the Graveyard shortly...")) {
                Requeue(LastGamemodeRaw, "Dying is so bald!", true);
                return;
            }
            if (message.Contains("§b§l» §r§aYou survived!")) {
                Requeue(LastGamemodeRaw, "Congratulations on surviving!", true);
                return;
            }

            // block party
            if (message.Contains("§crock 'n' rolled into the void") && message.Contains(localUsername)) {
                Requeue(LastGamemodeRaw, $"{localUsername} gave you up. {localUsername} let you down.", true);
                return;
            }
            if (message.Contains("§ctook the L!§8") && message.Contains(localUsername)) {
                Requeue(LastGamemodeRaw, "The 12th letter of the alphabet is yours.", true);
                return;
            }
            if (message.Contains("§cain't stayin' alive") && message.Contains(localUsername)) {
                Requeue(LastGamemodeRaw, "Ah, ha, ha, ha not staying alive. Not staying alive!", true);
                return;
            }
            if (message.Contains("§chas two left feet") && message.Contains(localUsername)) {
                Requeue(LastGamemodeRaw, "How can you dance?", true);
                return;
            }
            if (message.Contains("§cfell off the map") && message.Contains(localUsername)) {
                Requeue(LastGamemodeRaw, "L ratio.", true);
                return;
            }

            // gravity
            if (message.Contains("§a§l» §r§eYou finished all maps and came in")) {
                Requeue(LastGamemodeRaw, "Congratulations on finishing all maps!", true);
                return;
            }

            // all
            if (message.Contains("§c§l» §r§c§lGame OVER!")) {
                if (mode == HiveGameMode.TheBridge) {
                    var variant = HiveQueueCode.Parse(LastGamemodeRaw).Variant ?? "";
                    if (variant.Contains("DUOS", StringComparison.OrdinalIgnoreCase)) {
                        Requeue(LastGamemodeRaw, "Your game has ended.", true);
                    } else {
                        Onix.Game.ExecuteCommand("/hub");
                        Requeue(LastGamemodeRaw, "Your game has ended.", true);
                    }
                } else {
                    Requeue(LastGamemodeRaw);
                }
                return;
            }

            // block drop
            if (message.Contains("§c§l» §r§cYou died! §7Stick around or play another round.")) {
                Requeue(LastGamemodeRaw, "F.", true);
            }
        }

        private bool OnInput(InputKey key, bool isDown) {
            var requeueKey = Config.RequeueKey;
            if (isDown && requeueKey is not null && key == requeueKey && Onix.Gui.MouseGrabbed) {
                Requeue(LastGamemodeRaw);
            }
            return false;
        }

        internal void Requeue(string game, string? message = null, bool sendRequeue = false) {
            if (Onix.ConnectionInfo.ConnectedIp.Contains("zeqa", StringComparison.OrdinalIgnoreCase)) return;

            if (message is not null) Console.WriteLine(message);
            if (sendRequeue) Console.WriteLine("§r§8Queueing into a new game.");

            Onix.Game.ExecuteCommand($"/q {game}");
        }
    }
}
