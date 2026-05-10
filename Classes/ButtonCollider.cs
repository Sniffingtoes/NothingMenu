using UnityEngine;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using static Nothing.Menu.Main;
using static Nothing.Settings;
using Nothing.Menu;

namespace Nothing.Classes
{
    public class Button : MonoBehaviour
    {
        public string relatedText;
        public static float buttonCooldown = 0f;
        private static Dictionary<int, AudioClip> clickSounds = new Dictionary<int, AudioClip>();
        private static AudioSource audioPlayer;

        private static int totalSoundCount = 5;
        private static bool isLoaded = false;
        private static bool isCurrentlyLoading = false;

        void Awake()
        {
            if (!isLoaded && !isCurrentlyLoading)
            {
                StartCoroutine(LoadAllSoundsRoutine());
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if ((collider == buttonCollider || collider == buttonCollider2) && Time.time > buttonCooldown && menu != null)
            {
                buttonCooldown = Time.time + 0.2f;

                Toggle(this.relatedText);

                StartCoroutine(PlaySoundSafe(Settings.currentClickIndex));

                bool vibrateLeft;
                if (collider == buttonCollider)
                {
                    vibrateLeft = rightHanded;
                }
                else
                {
                    vibrateLeft = !rightHanded;
                }

                GorillaTagger.Instance.StartVibration(vibrateLeft, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
            }
        }

        private IEnumerator PlaySoundSafe(int index)
        {
            while (!isLoaded && isCurrentlyLoading)
            {
                yield return null;
            }

            if (clickSounds.TryGetValue(index, out AudioClip clip))
            {
                if (audioPlayer == null) CreateAudioSource();
                audioPlayer.Stop();
                audioPlayer.PlayOneShot(clip);
            }
        }

        private void CreateAudioSource()
        {
            GameObject sfx = new GameObject("ClickSound");
            if (Camera.main != null) sfx.transform.SetParent(Camera.main.transform, false);
            audioPlayer = sfx.AddComponent<AudioSource>();
            audioPlayer.spatialBlend = 0f;
            audioPlayer.volume = 1f;
        }

        public static IEnumerator LoadAllSoundsRoutine()
        {
            isCurrentlyLoading = true;

            for (int i = 1; i <= totalSoundCount; i++)
            {
                if (clickSounds.ContainsKey(i)) continue;

                string fileName = (i == 1) ? "Click.mp3" : "Click" + i + ".mp3";
                string resPath = "NothingMenu.Resources." + fileName;

                string tempFile = Path.Combine(Application.temporaryCachePath, "temp_" + i + "_" + fileName);

                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resPath))
                {
                    if (s == null)
                    {
                        Debug.LogError("Failed to find resource: " + resPath);
                        continue;
                    }

                    byte[] b = new byte[s.Length];
                    s.Read(b, 0, b.Length);
                    File.WriteAllBytes(tempFile, b);

                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempFile, AudioType.MPEG))
                    {
                        yield return www.SendWebRequest();
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                            clip.name = fileName;
                            clickSounds[i] = clip;
                        }
                    }

                    if (File.Exists(tempFile)) File.Delete(tempFile);
                }
            }

            isCurrentlyLoading = false;
            isLoaded = true;
        }
    }
}
