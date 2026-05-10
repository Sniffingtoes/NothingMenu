using HarmonyLib;

using JetBrains.Annotations;

using PlayFab.EventsModels;

using System.Reflection;

namespace Nothing.Patches.Internal
{
    public class TelemetryPatches
    {
        public static bool enabled = true;

        [HarmonyPatch(typeof(GorillaTelemetry), "EnqueueTelemetryEvent")]
        public class TelemetryPatch1
        {
            private static bool Prefix(string eventName, object content, [CanBeNull] string[] customTags = null) =>
                !enabled;
        }

        [HarmonyPatch]
        public class TelemetryPatch2
        {
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(GorillaTelemetry), "EnqueueTelemetryEventPlayFab");
            }

            private static bool Prefix(EventContents eventContent) =>
                !enabled;
        }
    }
}
