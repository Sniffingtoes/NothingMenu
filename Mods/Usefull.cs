using Custom.Inputs;

using ExitGames.Client.Photon;

using GorillaLocomotion;

using GorillaNetworking;

using Nothing.Menu;
using Nothing.Notifications;

using Photon.Pun;
using Photon.Realtime;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace NothingMenu.Mods
{
    internal class Usefull
    {
        public static void NoFingerMovement()
        {
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
            ControllerInputPoller.instance.leftControllerPrimaryButton = false;
            ControllerInputPoller.instance.leftControllerSecondaryButton = false;
            ControllerInputPoller.instance.rightControllerPrimaryButton = false;
            ControllerInputPoller.instance.rightControllerSecondaryButton = false;
        }

        public static void AntiReportLogic()
        {
            if (!PhotonNetwork.InRoom) return;
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            if (activeRigs == null) return;

            foreach (VRRig vrrig in activeRigs)
            {
                if (vrrig == GorillaTagger.Instance.offlineVRRig) continue;

                Vector3 rHand = vrrig.rightHandTransform.position + vrrig.rightHandTransform.forward * 0.125f;
                Vector3 lHand = vrrig.leftHandTransform.position + vrrig.leftHandTransform.forward * 0.125f;
                float radius = 0.6f;

                foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (line.linePlayer == NetworkSystem.Instance.LocalPlayer)
                    {
                        Vector3 btnPos = line.reportButton.gameObject.transform.position + new Vector3(0f, 0.001f, 0.0004f);

                        if (Vector3.Distance(btnPos, lHand) < radius || Vector3.Distance(btnPos, rHand) < radius)
                        {
                            NotifiLib.SendNotification("<color=red>Anti-Report:</color> " + vrrig.playerText1.text + " Attempted to Report You");

                            if (AntiReportSettings.index == 0)
                            {
                                PhotonNetwork.Disconnect();
                            }
                        }
                    }
                }
            }
        }



        public static void TPStump()
        {
            GTPlayer.Instance.TeleportTo(new Vector3(-68.647f, 12.406f, -83.699f), GTPlayer.Instance.transform.rotation, false, false);
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
        }
        public static void TPStumpRT()
        {
            if (Get.rTrigger)
            {
                GTPlayer.Instance.TeleportTo(new Vector3(-68.647f, 12.406f, -83.699f), GTPlayer.Instance.transform.rotation, false, false);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public static void DisableNetworkTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(false);
        }

        public static void EnableNetworkTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(true);
        }

        public static void QuitGTAG()
        {
            Application.Quit();
        }

        public static void NoTagOnJoin()
        {
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("didTutorial", out object obj);
            if (obj == null || (obj is bool @bool && @bool))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                {
                    { "didTutorial", false }
                });
            }
        }


    }
}
