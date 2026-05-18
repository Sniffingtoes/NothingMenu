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

        private const string SoundLibApiUrl = "https://api.github.com/repos/Sniffingtoes/NothingMenu/contents/soundlib";
        private const string SoundLibRawBase = "https://raw.githubusercontent.com/Sniffingtoes/NothingMenu/main/soundlib/";

        private static bool libraryLoaded = false;
        private static List<ButtonInfo> cachedLibraryButtons = null;

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
                Directory.CreateDirectory(SoundPath);

            List<ButtonInfo> soundButtons = new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Return to Main",     method = () => Main.currentCategory = 0,   isTogglable = false },
                new ButtonInfo { buttonText = "Open Sounds Folder", method = () => OpenFolder(),                isTogglable = false },
                new ButtonInfo { buttonText = "Sound Library",      method = () => OpenSoundLibrary(),          isTogglable = false },
                new ButtonInfo { buttonText = "Loop Sounds",        method = () => { isLooping = !isLooping; }, isTogglable = true  },
                new ButtonInfo { buttonText = "Stop All Sounds",    method = () => StopAllSounds(),             isTogglable = false }
            };

            string[] files = Directory.GetFiles(SoundPath, "*.mp3");
            foreach (string file in files)
            {
                string capturedFile = file;
                string fileName = Path.GetFileName(capturedFile);
                soundButtons.Add(new ButtonInfo
                {
                    buttonText = fileName.Replace(".mp3", ""),
                    method = () => PlayLocal(capturedFile),
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

        public static void OpenSoundLibrary()
        {
            if (libraryLoaded && cachedLibraryButtons != null)
            {
                Buttons.buttons[11] = cachedLibraryButtons.ToArray();
                Main.currentCategory = 11;
                return;
            }

            Buttons.buttons[11] = new ButtonInfo[]
            {
                new ButtonInfo
                {
                    buttonText = "Return to Soundboard",
                    method = () => { Main.currentCategory = 8; RefreshSoundboardButtons(); },
                    isTogglable = false
                },
                new ButtonInfo { buttonText = "Click to load", method = () => { }, isTogglable = false }
            };
            Main.currentCategory = 11;

            Instance.StartCoroutine(FetchSoundLibrary());
        }

        private static IEnumerator FetchSoundLibrary()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(SoundLibApiUrl))
            {
                www.SetRequestHeader("User-Agent", "NothingMenu");
                yield return www.SendWebRequest();

                List<ButtonInfo> libButtons = new List<ButtonInfo>
                {
                    new ButtonInfo
                    {
                        buttonText = "Return to Soundboard",
                        method = () => { Main.currentCategory = 8; RefreshSoundboardButtons(); },
                        isTogglable = false
                    }
                };

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    List<string> mp3Names = ParseGitHubFileNames(json, ".mp3");

                    if (mp3Names.Count == 0)
                    {
                        libButtons.Add(new ButtonInfo { buttonText = "No sounds found", method = () => { }, isTogglable = false });
                    }
                    else
                    {
                        foreach (string name in mp3Names)
                        {
                            string capturedName = name;
                            string url = SoundLibRawBase + UnityWebRequest.EscapeURL(capturedName);
                            libButtons.Add(new ButtonInfo
                            {
                                buttonText = capturedName.Replace(".mp3", ""),
                                method = () => Instance.StartCoroutine(DownloadOnly(url, capturedName)),
                                isTogglable = false
                            });
                        }
                    }

                    libraryLoaded = true;
                    cachedLibraryButtons = libButtons;
                }
                else
                {
                    libButtons.Add(new ButtonInfo { buttonText = "Failed to load library", method = () => { }, isTogglable = false });
                    libButtons.Add(new ButtonInfo { buttonText = "Retry", method = () => OpenSoundLibrary(), isTogglable = false });
                }

                Buttons.buttons[11] = libButtons.ToArray();
                Main.currentCategory = 11;
            }
        }

        private static IEnumerator DownloadOnly(string url, string fileName)
        {
            if (!Directory.Exists(SoundPath))
                Directory.CreateDirectory(SoundPath);

            string savePath = Path.Combine(SoundPath, fileName);

            if (File.Exists(savePath))
            {
                PlayLocal(savePath);
                yield break;
            }

            using (UnityWebRequest dl = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                dl.downloadHandler = new DownloadHandlerBuffer();
                dl.SetRequestHeader("User-Agent", "NothingMenu");
                dl.timeout = 0;

                yield return dl.SendWebRequest();

                if (dl.result != UnityWebRequest.Result.Success)
                    yield break;

                File.WriteAllBytes(savePath, dl.downloadHandler.data);
            }

            RefreshSoundboardButtons();
        }

        private static List<string> ParseGitHubFileNames(string json, string extension)
        {
            List<string> names = new List<string>();
            string search = "\"name\":\"";
            int idx = 0;
            while ((idx = json.IndexOf(search, idx)) != -1)
            {
                idx += search.Length;
                int end = json.IndexOf("\"", idx);
                if (end == -1) break;
                string name = json.Substring(idx, end - idx);
                if (name.EndsWith(extension, System.StringComparison.OrdinalIgnoreCase))
                    names.Add(name);
                idx = end;
            }
            return names;
        }

        public static void PlayLocal(string path)
        {
            StopAllSounds();
            activeRoutine = Instance.StartCoroutine(PlayLocalCoroutine(path));
        }

        private static IEnumerator PlayLocalCoroutine(string fullPath)
        {
            Recorder voiceRecorder = Object.FindFirstObjectByType<Recorder>();
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
                        activeRoutine = Instance.StartCoroutine(PlayLocalCoroutine(fullPath));
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

            GameObject[] ghosts = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject g in ghosts)
            {
                if (g.name == "NothingSound_Local") Destroy(g);
            }

            Recorder voiceRecorder = Object.FindFirstObjectByType<Recorder>();
            if (voiceRecorder != null)
            {
                voiceRecorder.SourceType = Recorder.InputSourceType.Microphone;
                voiceRecorder.VoiceDetection = true;
                voiceRecorder.RestartRecording();
            }
        }
    }
}
