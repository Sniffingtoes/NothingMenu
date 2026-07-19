using Nothing.Classes;
using UnityEngine;
using Nothing.Notifications;
using System.Collections.Generic;

namespace Nothing
{
    public class Settings
    {
        public static ExtGradient backgroundColor = new ExtGradient { rainbow = false };
        public static ExtGradient[] buttonColors = new ExtGradient[]
        {
            new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) },
            new ExtGradient { rainbow = false }
        };

        public static Color[] textColors = new Color[]
        {
            Color.white,
            Color.green
        };

        public static int currentClickIndex = 1;

        public static int NormalizeClickSoundIndex(int index)
        {
            const int min = 1;
            const int max = 5;
            if (index < min) return min;
            if (index > max) return max;
            return index;
        }

        public static string GetClickSound()
        {
            currentClickIndex = NormalizeClickSoundIndex(currentClickIndex);
            return currentClickIndex.ToString();
        }

        public static void RightHand()
        {
            Settings.rightHanded = true;
        }

        public static bool fpsCounter = true;
        public static bool disconnectButton = true;
        public static bool rightHanded;
        public static bool disableNotifications;
        public static bool pcArrayList = true;
        public static bool pcWatermark
        {
            get => PlayerPrefs.GetInt("PcWatermark", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("PcWatermark", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool pcRoomJoiner
        {
            get => PlayerPrefs.GetInt("PcRoomJoiner", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("PcRoomJoiner", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
        public static KeyCode keyboardButton = KeyCode.Q;
        public static Vector3 menuSize = new Vector3(0.06f, 0.63f, 0.8f);
        public static int buttonsPerPage = 4;
        public static float gradientSpeed = 0.5f;
    }
}
