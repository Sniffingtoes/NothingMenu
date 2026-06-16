using HarmonyLib;
using JetBrains.Annotations;
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

        public class TelemetryPatch2
        {
            public static bool Prefix()
            {
                return !enabled;
            }
        }
    }
}