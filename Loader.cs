using UnityEngine;
using Nothing;
using Nothing.Menu;
using Nothing.Notifications;

namespace Loading
{
    public class Loader
    {
        private static GameObject _load;

        public static void Load()
        {
            if (_load == null)
            {
                _load = new GameObject("NothingMenuLoaded");
                _load.AddComponent<Main>();
                _load.AddComponent<NotifiLib>();

                HarmonyPatches.OnPlayerSpawned();

                Object.DontDestroyOnLoad(_load);
            }
        }
    }
}
