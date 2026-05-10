using System;

using HarmonyLib;

using Photon.Pun;

namespace Nothing.Patches.Internal
{
    [HarmonyPatch(typeof(PhotonView), "SerializeComponent")]
    public class SerializePatch
    {
        public static Func<bool> OverrideSerialization;

        public static bool Prefix()
        {
            if (OverrideSerialization != null)
            {
                return OverrideSerialization.Invoke();
            }
            return true;
        }
    }
}
