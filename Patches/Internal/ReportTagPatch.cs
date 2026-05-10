using System.Collections.Generic;

using Photon.Realtime;

namespace Nothing.Patches.Internal
{
    internal class ReportTagPatch
    {
        public static System.Collections.Generic.List<Photon.Realtime.Player> invinciblePlayers =
            new System.Collections.Generic.List<Photon.Realtime.Player>();
    }
}
