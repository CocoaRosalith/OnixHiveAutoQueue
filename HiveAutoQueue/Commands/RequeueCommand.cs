using HiveAutoQueue;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.OnixClient.Commands;

namespace HiveAutoQueue.Commands {
    public class RequeueCommand : OnixCommandBase {
        public RequeueCommand() : base("rq", "Requeues into your last known Hive gamemode.", CommandExecutionTarget.Client, CommandPermissionLevel.Any) { }

        [Overload]
        OnixCommandOutput Run() {
            var lastGamemode = HiveAutoQueue.Instance.LastGamemodeRaw;
            if (string.IsNullOrEmpty(lastGamemode)) {
                return Error("No known gamemode to requeue into yet.");
            }

            HiveAutoQueue.Instance.Requeue(lastGamemode);
            return Success($"§c§l» §r§cQueuing into a new game of {HiveQueueCode.FormatDisplay(lastGamemode)}§r§c.");
        }
    }
}
