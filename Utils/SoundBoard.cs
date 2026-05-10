using Photon.Voice.Unity;

using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

using Nothing.Classes;

namespace Nothing.Menu
{
    public class SoundboardHandler : MonoBehaviour
    {
        private static SoundboardHandler loader;
        private static Coroutine activeRoutine;
        private static GameObject localSoundObj;
        public static bool isLooping = false;
        public static string SoundPath = Path.Combine(Directory.GetCurrentDirectory(), "NothingMenu", "Sounds");

        private static SoundboardHandler Instance
        {
            get
            {
                if (loader == null)
                {
                    GameObject go = new GameObject("Nothing_SoundRunner");
                    loader = go.AddComponent<SoundboardHandler>();
                    Object.DontDestroyOnLoad(go);
                }
                return loader;
            }
        }

        public static void RefreshSoundboardButtons()
        {
            if (!Directory.Exists(SoundPath))
            {
                Directory.CreateDirectory(SoundPath);
            }

            List<ButtonInfo> soundButtons = new List<ButtonInfo>
            {
            new ButtonInfo { buttonText = "Return to Main", method = () => Main.currentCategory = 0, isTogglable = false },
            new ButtonInfo { buttonText = "Open Sounds Folder", method = () => OpenFolder(), isTogglable = false },
            new ButtonInfo { buttonText = "Loop Sounds", method = () => { isLooping = !isLooping; }, isTogglable = true },
            new ButtonInfo { buttonText = "Stop All Sounds", method = () => StopAllSounds(), isTogglable = false }
            };

            string[] files = Directory.GetFiles(SoundPath, "*.mp3");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                soundButtons.Add(new ButtonInfo
                {
                    buttonText = fileName.Replace(".mp3", ""),
                    method = () => { Play(file); },
                    isTogglable = false
                });
            }

            Buttons.buttons[8] = soundButtons.ToArray();
        }

        public static void OpenFolder()
        {
            if (!Directory.Exists(SoundPath)) Directory.CreateDirectory(SoundPath);
            Process.Start(new ProcessStartInfo()
            {
                FileName = SoundPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public static void Play(string path)
        {
            StopAllSounds();
            activeRoutine = Instance.StartCoroutine(PlayMP3(path));
        }

        private static IEnumerator PlayMP3(string fullPath)
        {
            Recorder voiceRecorder = Object.FindObjectOfType<Recorder>();
            if (voiceRecorder == null) yield break;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + fullPath, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                    var originalSource = voiceRecorder.SourceType;
                    var originalClip = voiceRecorder.AudioClip;

                    voiceRecorder.SourceType = Recorder.InputSourceType.AudioClip;
                    voiceRecorder.AudioClip = clip;
                    voiceRecorder.VoiceDetection = false;
                    voiceRecorder.TransmitEnabled = true;
                    voiceRecorder.RestartRecording();

                    localSoundObj = new GameObject("NothingSound_Local");
                    AudioSource localSource = localSoundObj.AddComponent<AudioSource>();
                    localSource.clip = clip;
                    localSource.spatialBlend = 0f;
                    localSource.volume = 0.5f;
                    localSource.Play();

                    yield return new WaitForSeconds(clip.length);

                    if (isLooping)
                    {
                        if (localSoundObj != null) Destroy(localSoundObj);
                        activeRoutine = Instance.StartCoroutine(PlayMP3(fullPath));
                        yield break;
                    }

                    voiceRecorder.SourceType = originalSource;
                    voiceRecorder.AudioClip = originalClip;
                    voiceRecorder.VoiceDetection = true;
                    voiceRecorder.RestartRecording();

                    if (localSoundObj != null) Destroy(localSoundObj);
                    activeRoutine = null;
                }
            }
        }

        public static void StopAllSounds()
        {
            if (activeRoutine != null)
            {
                Instance.StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            if (localSoundObj != null) Destroy(localSoundObj);

            GameObject[] ghosts = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject g in ghosts)
            {
                if (g.name == "NothingSound_Local") Destroy(g);
            }

            Recorder voiceRecorder = Object.FindObjectOfType<Recorder>();
            if (voiceRecorder != null)
            {
                voiceRecorder.SourceType = Recorder.InputSourceType.Microphone;
                voiceRecorder.VoiceDetection = true;
                voiceRecorder.RestartRecording();
            }
        }
    }
}
