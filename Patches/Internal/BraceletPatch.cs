using HarmonyLib;

namespace Nothing.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.UpdateFriendshipBracelet))]
    public class BraceletPatch
    {
        public static bool enabled;

        public static bool Prefix(VRRig __instance) =>
            !enabled;
    }
}
