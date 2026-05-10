using Nothing.Notifications;

using Photon.Pun;

using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Custom.Inputs;

namespace NothingMenu.Mods
{
    internal class Settingss
    {
        public static void RTDisconnect()
        {
            if (Get.rTrigger)
            {
                Photon.Pun.PhotonNetwork.Disconnect();
            }
        }

        public static void CheckMaster()
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    NotifiLib.SendNotification("<color=yellow>YOU ARE MASTER</color>");
                }
                else
                {
                    NotifiLib.SendNotification("<color=orange>YOU ARE NOT MASTER</color>");
                }
            }
            else
            {
                NotifiLib.SendNotification("<color=white>NOT IN A ROOM</color>");
            }
        }
    }
}
