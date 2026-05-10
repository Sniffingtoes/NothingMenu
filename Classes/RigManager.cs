using System;
using System.Collections.Generic;

using HarmonyLib;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Nothing.Classes
{
    public class RigManager
    {
        public static VRRig GetVRRigFromPlayer(Player p)
        {
            return GorillaGameManager.instance.FindPlayerVRRig(p);
        }

        public static VRRig GetRandomVRRig(bool includeSelf)
        {
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null || activeRigs.Count == 0;
            VRRig result;
            if (flag)
            {
                result = null;
            }
            else if (includeSelf)
            {
                result = activeRigs[Random.Range(0, activeRigs.Count)];
            }
            else
            {
                for (int i = 0; i < activeRigs.Count; i++)
                {
                    VRRig vrrig = activeRigs[Random.Range(0, activeRigs.Count)];
                    bool flag2 = vrrig != VRRig.LocalRig;
                    if (flag2)
                    {
                        return vrrig;
                    }
                }
                result = null;
            }
            return result;
        }
        public static VRRig GetClosestVRRig()
        {
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            bool flag = activeRigs == null || activeRigs.Count == 0;
            VRRig result;
            if (flag)
            {
                result = null;
            }
            else
            {
                float num = float.MaxValue;
                VRRig vrrig = null;
                foreach (VRRig vrrig2 in activeRigs)
                {
                    bool flag2 = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig2.transform.position) < num;
                    if (flag2)
                    {
                        num = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig2.transform.position);
                        vrrig = vrrig2;
                    }
                }
                result = vrrig;
            }
            return result;
        }

        public static PhotonView GetPhotonViewFromVRRig(VRRig p)
        {
            return (PhotonView)Traverse.Create(p).Field("photonView").GetValue();
        }

        public static Player GetRandomPlayer(bool includeSelf)
        {
            Player result;
            if (includeSelf)
            {
                result = PhotonNetwork.PlayerList[Random.Range(0, PhotonNetwork.PlayerList.Length - 1)];
            }
            else
            {
                result = PhotonNetwork.PlayerListOthers[Random.Range(0, PhotonNetwork.PlayerListOthers.Length - 1)];
            }
            return result;
        }

        public static Player GetPlayerFromVRRig(VRRig p)
        {
            return RigManager.GetPhotonViewFromVRRig(p).Owner;
        }

        public static Player GetPlayerFromID(string id)
        {
            Player result = null;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                bool flag = player.UserId == id;
                if (flag)
                {
                    result = player;
                    break;
                }
            }
            return result;
        }
    }
}
