using BepInEx;
using UnityEngine;
using Nothing.Menu;
using Nothing.Notifications;
using NothingMenu.Utils;
using System;
using System.Collections;

namespace Nothing
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class HarmonyPatches : BaseUnityPlugin
    {
        public static bool isInitialized = false;
        private static GameObject uiObject;

        public static void OnPlayerSpawned()
        {
            if (isInitialized) return;
            isInitialized = true;

            try
            {
                Nothing.Menu.Buttons.Init();
                SaveSystem.Load();
                Patches.PatchHandler.PatchAll();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Nothing Menu] Non-fatal patch execution failure bypassed: {ex.Message}");
            }

            NotifiLib.SendNotification("Test notification");

            uiObject = new GameObject("NothingPCUI");
            uiObject.AddComponent<PCUI>();
            uiObject.AddComponent<RoomGUI>();
            uiObject.AddComponent<Thingy>();
            uiObject.AddComponent<Owner>();
            uiObject.AddComponent<ModsPP>();

            DontDestroyOnLoad(uiObject);
        }

        private void Update()
        {
            if (!isInitialized)
            {
                if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
                {
                    StartCoroutine(InitializationDelayRoutine());
                }
            }
        }

        private IEnumerator InitializationDelayRoutine()
        {
            yield return new WaitForEndOfFrame();
            if (!isInitialized)
            {
                OnPlayerSpawned();
            }
        }
    }
}