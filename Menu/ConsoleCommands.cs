using System;
using System.Collections.Generic;
using System.Text;

using Nothing.Notifications;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

namespace Nothing.Menu
{
    internal static class ConsoleCommands
    {
        public static void AddWelcomeLines(List<string> lines)
        {
            if (lines.Count > 0) return;

            lines.Add("Nothing console.");
            lines.Add("Run \"help\" for a list of all commands.");
            lines.Add("If you are a normal user you dont have anything to do here.");
        }

        public static void Execute(string command, List<string> lines)
        {
            string trimmed = command.Trim();
            if (trimmed.Length == 0) return;

            lines.Add("> " + trimmed);

            switch (trimmed.ToLowerInvariant())
            {
                case "playfabid":
                case "id":
                    lines.Add(GetLocalPlayFabId());
                    break;
                case "copyid":
                case "copy playfabid":
                case "copy playfab id":
                    CopyPlayFabId(lines);
                    break;
                case "status":
                case "info":
                    AddStatus(lines);
                    break;
                case "room":
                case "roominfo":
                    AddRoomInfo(lines);
                    break;
                case "copyroom":
                case "copy room":
                    CopyRoomCode(lines);
                    break;
                case "players":
                case "playerlist":
                    AddPlayers(lines);
                    break;
                case "rigs":
                case "rigcount":
                    AddRigInfo(lines);
                    break;
                case "pos":
                case "position":
                    AddPosition(lines);
                    break;
                case "velocity":
                case "vel":
                    AddVelocity(lines);
                    break;
                case "fps":
                    AddFps(lines);
                    break;
                case "ping":
                    AddPing(lines);
                    break;
                case "time":
                    lines.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                case "notifytest":
                case "notiftest":
                    NotifiLib.SendNotification("Console notification test");
                    lines.Add("Sent notification test.");
                    break;
                case "clear":
                case "cls":
                    lines.Clear();
                    break;
                case "help":
                    AddHelp(lines);
                    break;
                default:
                    lines.Add("Unknown command: " + trimmed);
                    break;
            }

            Trim(lines);
        }

        public static void CopyPlayFabId(List<string> lines)
        {
            string playFabId = GetLocalPlayFabId();
            if (string.IsNullOrEmpty(playFabId) || playFabId == "Unavailable")
            {
                lines.Add("PlayFab ID unavailable.");
                Trim(lines);
                return;
            }

            GUIUtility.systemCopyBuffer = playFabId;
            lines.Add("Copied PlayFab ID: " + playFabId);
            NotifiLib.SendNotification("Copied PlayFab ID");
            Trim(lines);
        }

        private static void AddHelp(List<string> lines)
        {
            lines.Add("status - menu, network, fps, ping summary");
            lines.Add("playfabid/id - prints your PlayFab ID");
            lines.Add("copyid - copies your PlayFab ID");
            lines.Add("room - prints current room info");
            lines.Add("copyroom - copies current room code");
            lines.Add("players - lists Photon players");
            lines.Add("rigs - prints active rig count");
            lines.Add("pos - prints your current position");
            lines.Add("velocity/vel - prints current rigidbody velocity");
            lines.Add("fps - prints current FPS estimate");
            lines.Add("ping - prints Photon ping");
            lines.Add("time - prints local time");
            lines.Add("notifytest - sends a test notification");
            lines.Add("clear/cls - clears this console");
        }

        private static void AddStatus(List<string> lines)
        {
            lines.Add("Plugin: " + PluginInfo.Name + " " + PluginInfo.Version);
            lines.Add("PlayFab ID: " + GetLocalPlayFabId());
            lines.Add("Network: " + (PhotonNetwork.IsConnected ? "connected" : "disconnected") + ", room: " + GetRoomName());
            lines.Add("Players: " + GetPlayerCountText() + ", rigs: " + GetRigCount());
            lines.Add("FPS: " + GetFpsText() + ", ping: " + GetPingText());
            lines.Add("Position: " + FormatVector(GetPosition()));
        }

        private static void AddRoomInfo(List<string> lines)
        {
            if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
            {
                lines.Add("Not in a room.");
                return;
            }

            Room room = PhotonNetwork.CurrentRoom;
            lines.Add("Room: " + room.Name);
            lines.Add("Players: " + room.PlayerCount + "/" + room.MaxPlayers);
            lines.Add("Visible: " + room.IsVisible + ", open: " + room.IsOpen);
            lines.Add("Master: " + GetMasterClientName());
        }

        private static void CopyRoomCode(List<string> lines)
        {
            string roomName = GetRoomName();
            if (roomName == "None")
            {
                lines.Add("No room code to copy.");
                return;
            }

            GUIUtility.systemCopyBuffer = roomName;
            lines.Add("Copied room code: " + roomName);
            NotifiLib.SendNotification("Copied room code");
        }

        private static void AddPlayers(List<string> lines)
        {
            if (!PhotonNetwork.InRoom || PhotonNetwork.PlayerList == null || PhotonNetwork.PlayerList.Length == 0)
            {
                lines.Add("No Photon players found.");
                return;
            }

            lines.Add("Players:");
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                string master = player.IsMasterClient ? " master" : "";
                string local = player.IsLocal ? " local" : "";
                lines.Add("#" + player.ActorNumber + " " + player.NickName + " | " + player.UserId + master + local);
            }
        }

        private static void AddRigInfo(List<string> lines)
        {
            int active = GetRigCount();
            int remote = 0;
            IReadOnlyList<VRRig> rigs = VRRigCache.ActiveRigs;
            if (rigs != null)
            {
                foreach (VRRig rig in rigs)
                {
                    if (rig != null && rig != VRRig.LocalRig && rig != GorillaTagger.Instance.offlineVRRig)
                        remote++;
                }
            }

            lines.Add("Active rigs: " + active + ", remote rigs: " + remote);
        }

        private static void AddPosition(List<string> lines)
        {
            lines.Add("Position: " + FormatVector(GetPosition()));
            try
            {
                if (GorillaTagger.Instance != null && GorillaTagger.Instance.headCollider != null)
                    lines.Add("Head: " + FormatVector(GorillaTagger.Instance.headCollider.transform.position));
            }
            catch
            {
            }
        }

        private static void AddVelocity(List<string> lines)
        {
            try
            {
                if (GorillaTagger.Instance != null && GorillaTagger.Instance.rigidbody != null)
                {
                    Vector3 velocity = GorillaTagger.Instance.rigidbody.linearVelocity;
                    lines.Add("Velocity: " + FormatVector(velocity) + " | speed " + velocity.magnitude.ToString("0.00"));
                    return;
                }
            }
            catch
            {
            }

            lines.Add("Velocity unavailable.");
        }

        private static void AddFps(List<string> lines) => lines.Add("FPS: " + GetFpsText());

        private static void AddPing(List<string> lines) => lines.Add("Ping: " + GetPingText());

        private static string GetLocalPlayFabId()
        {
            try
            {
                if (PhotonNetwork.LocalPlayer != null && !string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.UserId))
                    return PhotonNetwork.LocalPlayer.UserId;
            }
            catch
            {
            }

            return "Unavailable";
        }

        private static string GetRoomName()
        {
            try
            {
                if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null && !string.IsNullOrEmpty(PhotonNetwork.CurrentRoom.Name))
                    return PhotonNetwork.CurrentRoom.Name;
            }
            catch
            {
            }

            return "None";
        }

        private static string GetMasterClientName()
        {
            try
            {
                return PhotonNetwork.MasterClient != null ? PhotonNetwork.MasterClient.NickName : "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetPlayerCountText()
        {
            try
            {
                if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
                    return PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
                return PhotonNetwork.PlayerList != null ? PhotonNetwork.PlayerList.Length.ToString() : "0";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static int GetRigCount()
        {
            try
            {
                return VRRigCache.ActiveRigs != null ? VRRigCache.ActiveRigs.Count : 0;
            }
            catch
            {
                return 0;
            }
        }

        private static Vector3 GetPosition()
        {
            try
            {
                if (GorillaTagger.Instance != null)
                    return GorillaTagger.Instance.transform.position;
            }
            catch
            {
            }

            return Vector3.zero;
        }

        private static string GetFpsText()
        {
            try
            {
                return Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString("0");
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetPingText()
        {
            try
            {
                return PhotonNetwork.GetPing() + " ms";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string FormatVector(Vector3 value) =>
            value.x.ToString("0.00") + ", " + value.y.ToString("0.00") + ", " + value.z.ToString("0.00");

        private static void Trim(List<string> lines)
        {
            while (lines.Count > 80)
                lines.RemoveAt(0);
        }
    }
}
