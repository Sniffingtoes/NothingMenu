using HarmonyLib;

using Photon.Pun;

namespace Nothing.Patches.Internal
{
    public class IncrementRPCPatches
    {
        [HarmonyPatch(typeof(VRRig), "IncrementRPC", typeof(PhotonMessageInfoWrapped), typeof(string))]
        public class NoIncrementRPC
        {
            private static bool Prefix(PhotonMessageInfoWrapped info, string sourceCall) =>
                false;
        }

    }
}
