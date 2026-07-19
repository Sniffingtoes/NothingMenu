using Nothing.Mods;
using Oculus.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace Nothing.Menu
{
    [Serializable]
    public class SaveData
    {
        public int SelectedThemeIndex = 0;
        public int SelectedClickIndex = 1;
        public int SelectedFlySpeedIndex = 1;
        public int SelectedBoostIndex = 0;
        public int SelectedGravityIndex = 0;
        public int SelectedAntiReportIndex = 0;
        public bool PcArrayListDisabled = false;
        public List<string> EnabledMods = new List<string>();
        public List<string> FavoritedMods = new List<string>();
    }

    public static class FlySettings
    {
        public static string[] labels = { "really slow", "slow", "fast", "super fast" };
        public static float[] values = { 5f, 15f, 30f, 60f };
        public static int index = 1;
    }

    public static class BoostSettings
    {
        public static string[] labels = { "slow", "normal", "fast" };
        public static float[] values = { 6.3f, 7.3f, 8.0f };
        public static int index = 0;
    }

    public static class AntiReportSettings
    {
        public static string[] labels = { "Disconnect", "Notify" };
        public static int index = 0;
    }

    public static class SaveSystem
    {
        private static string FolderPath = Path.Combine(Environment.CurrentDirectory, "NothingMenu");
        private static string FilePath = Path.Combine(FolderPath, "save.json");
        private static bool hasLoaded = false;

        public static void AutoSave() => InternalSave();
        public static void Save() => InternalSave();

        private static void InternalSave()
        {
            try
            {
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

                SaveData data = new SaveData();
                data.SelectedThemeIndex = ThemeManager.currentThemeIndex;
                data.FavoritedMods = FavoriteMods.favoritedNames;
                data.SelectedClickIndex = Settings.currentClickIndex;
                data.SelectedFlySpeedIndex = FlySettings.index;
                data.SelectedBoostIndex = BoostSettings.index;
                data.SelectedAntiReportIndex = AntiReportSettings.index;
                data.PcArrayListDisabled = Settings.pcArrayList;

                foreach (var category in Buttons.buttons)
                {
                    if (category == null) continue;
                    foreach (var button in category)
                    {
                        if (button.isTogglable && button.enabled)
                        {
                            if (IsDesktopOverlaySetting(button.buttonText)) continue;
                            if (EnabledMods.IsDisabledServerCheck(button.buttonText)) continue;
                            data.EnabledMods.Add(button.buttonText);
                        }
                    }
                }

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e) { Debug.LogError("Save Failed: " + e.Message); }
        }

        public static void Load()
        {
            if (hasLoaded) return;
            try
            {
                if (!File.Exists(FilePath)) { hasLoaded = true; return; }
                string json = File.ReadAllText(FilePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return;

                ThemeManager.currentThemeIndex = data.SelectedThemeIndex;
                Settings.currentClickIndex = Settings.NormalizeClickSoundIndex(data.SelectedClickIndex);
                FlySettings.index = data.SelectedFlySpeedIndex;
                BoostSettings.index = data.SelectedBoostIndex;
                AntiReportSettings.index = data.SelectedAntiReportIndex;
                Settings.pcArrayList = data.PcArrayListDisabled;

                Movement.FlySpeed = FlySettings.values[FlySettings.index];
                Movement.SpeedBoostSpeed = BoostSettings.values[BoostSettings.index];

                if (data.FavoritedMods != null)
                {
                    FavoriteMods.favoritedNames = data.FavoritedMods;
                    FavoriteMods.RefreshFavoriteTab();
                }

                if (data.EnabledMods != null)
                {
                    foreach (var category in Buttons.buttons)
                    {
                        if (category == null) continue;
                        foreach (var button in category)
                        {
                            if (IsDesktopOverlaySetting(button.buttonText)) continue;
                            if (EnabledMods.IsDisabledServerCheck(button.buttonText))
                            {
                                button.enabled = false;
                                continue;
                            }

                            if (data.EnabledMods.Contains(button.buttonText) && !button.enabled)
                            {
                                button.enabled = true;
                            }
                        }
                    }
                }

                Buttons.RefreshValueChangerLabels();

                ThemeManager.ApplyActiveTheme();
                EnabledMods.RefreshEnabledTab();
                hasLoaded = true;
            }
            catch (Exception e) { Debug.LogError("Load Failed: " + e.Message); }
        }

        private static bool IsDesktopOverlaySetting(string buttonText) =>
            buttonText == "Disable PC Watermark" || buttonText == "Disable PC Room Joiner";

    }
}
