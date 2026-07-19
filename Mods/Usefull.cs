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
        private static bool disconnectingForReport;

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
            if (!PhotonNetwork.InRoom)
            {
                disconnectingForReport = false;
                return;
            }

            if (disconnectingForReport) return;

            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            if (activeRigs == null || NetworkSystem.Instance == null) return;

            GorillaPlayerScoreboardLine localPlayerLine = null;
            IReadOnlyList<GorillaPlayerScoreboardLine> scoreboardLines = GorillaScoreboardTotalUpdater.allScoreboardLines;
            if (scoreboardLines == null) return;

            foreach (GorillaPlayerScoreboardLine line in scoreboardLines)
            {
                if (line != null && line.linePlayer == NetworkSystem.Instance.LocalPlayer && line.reportButton != null)
                {
                    localPlayerLine = line;
                    break;
                }
            }

            if (localPlayerLine == null) return;

            Vector3 reportButtonPosition = localPlayerLine.reportButton.transform.position + new Vector3(0f, 0.001f, 0.0004f);
            const float reportRadius = 0.6f;

            foreach (VRRig vrrig in activeRigs)
            {
                if (vrrig == null || vrrig == GorillaTagger.Instance.offlineVRRig || vrrig.leftHandTransform == null || vrrig.rightHandTransform == null) continue;

                Vector3 rHand = vrrig.rightHandTransform.position + vrrig.rightHandTransform.forward * 0.125f;
                Vector3 lHand = vrrig.leftHandTransform.position + vrrig.leftHandTransform.forward * 0.125f;

                if (Vector3.Distance(reportButtonPosition, lHand) >= reportRadius && Vector3.Distance(reportButtonPosition, rHand) >= reportRadius) continue;

                string playerName = vrrig.playerText1 != null ? vrrig.playerText1.text : "A player";
                NotifiLib.SendNotification("<color=red>Anti-Report:</color> " + playerName + " Attempted to Report You");

                if (AntiReportSettings.index == 0)
                {
                    disconnectingForReport = true;
                    PhotonNetwork.Disconnect();
                }

                return;
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
