using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.OnixClient;

namespace HiveAutoQueue {
    public partial class HiveAutoQueueConfig : OnixModuleSettingRedirector {
        [Category("Settings")]
        [Name("Requeue Key", "Press this key to manually requeue into your last known Hive gamemode.")]
        [Value(InputKey.Type.None)]
        [Air(5, afterSetting: true)]
        public partial InputKey RequeueKey { get; set; }
    }
}
