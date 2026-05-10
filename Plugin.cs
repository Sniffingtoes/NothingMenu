using BepInEx;

using UnityEngine;

using Nothing.Menu;
using Nothing.Notifications;

using NothingMenu.Menu;

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

            Nothing.Menu.Buttons.Init();
            SaveSystem.Load();
            Patches.PatchHandler.PatchAll();

            NotifiLib.SendNotification("Test notification");

            uiObject = new GameObject("NothingPCUI");
            uiObject.AddComponent<PCUI>();
            uiObject.AddComponent<RoomGUI>();
            uiObject.AddComponent<Thingy>();
            uiObject.AddComponent<Owner>();
            uiObject.AddComponent<ModsPP>();

            DontDestroyOnLoad(uiObject);

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized && GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
            {
                OnPlayerSpawned();
            }
        }
    }
}
