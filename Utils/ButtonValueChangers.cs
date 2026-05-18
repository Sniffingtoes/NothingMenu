using Nothing.Classes;
using Nothing.Mods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nothing.Menu
{
    public partial class Buttons
    {
        private static readonly List<ValueChangerDefinition> valueChangers = new List<ValueChangerDefinition>();

        public static bool IsValueChanger(string buttonText) =>
            valueChangers.Any(changer => changer.Matches(buttonText));

        public static bool TryHandleValueChangerControl(string controlId)
        {
            if (string.IsNullOrEmpty(controlId) || !controlId.StartsWith("ValueChanger|"))
                return false;

            string[] parts = controlId.Split('|');
            if (parts.Length != 3)
                return false;

            int step = parts[2] == "-" ? -1 : (parts[2] == "+" ? 1 : 0);
            if (step == 0)
                return false;

            ValueChangerDefinition changer = valueChangers.FirstOrDefault(x => x.Key == parts[1]);
            if (changer == null)
                return false;

            changer.Change(step);
            return true;
        }

        public static string GetValueChangerControlId(string buttonText, bool increment)
        {
            ValueChangerDefinition changer = valueChangers.FirstOrDefault(x => x.Matches(buttonText));
            return changer?.GetControlId(increment);
        }

        public static void RefreshValueChangerLabels()
        {
            buttons.SelectMany(group => group).ToList().ForEach(button =>
            {
                ValueChangerDefinition changer = valueChangers.FirstOrDefault(x => x.Matches(button.buttonText));
                if (changer != null) button.buttonText = changer.Label;
            });
        }

        private static ButtonInfo ValueButton(string name, Func<string> getValue, Action<int> change)
        {
            ValueChangerDefinition changer = GetOrCreateValueChanger(name, getValue, change);
            return new ButtonInfo { buttonText = changer.Label, method = () => changer.Change(1), isTogglable = false };
        }

        private static void CycleTheme(int step)
        {
            ThemeManager.currentThemeIndex = WrapIndex(ThemeManager.currentThemeIndex + step, ThemeManager.themes.Length);
            ThemeManager.ApplyActiveTheme();
            SaveSystem.Save();
            RefreshValueChangerLabels();
        }

        private static void CycleClickSound(int step)
        {
            const int minClickSound = 1;
            const int maxClickSound = 5;
            const int clickSoundCount = maxClickSound - minClickSound + 1;

            int normalizedIndex = Settings.currentClickIndex - minClickSound;
            Settings.currentClickIndex = WrapIndex(normalizedIndex + step, clickSoundCount) + minClickSound;
            SaveSystem.Save();
            RefreshValueChangerLabels();
        }

        private static void CycleFlySpeed(int step)
        {
            FlySettings.index = WrapIndex(FlySettings.index + step, FlySettings.labels.Length);
            Movement.FlySpeed = FlySettings.values[FlySettings.index];
            SaveSystem.Save();
            RefreshValueChangerLabels();
        }

        private static void CycleSpeedBoost(int step)
        {
            BoostSettings.index = WrapIndex(BoostSettings.index + step, BoostSettings.labels.Length);
            Movement.SpeedBoostSpeed = BoostSettings.values[BoostSettings.index];
            SaveSystem.Save();
            RefreshValueChangerLabels();
        }

        private static ValueChangerDefinition GetOrCreateValueChanger(string name, Func<string> getValue, Action<int> change)
        {
            ValueChangerDefinition changer = valueChangers.FirstOrDefault(x => x.Name == name);
            if (changer != null)
                return changer;

            changer = new ValueChangerDefinition(name, getValue, change);
            valueChangers.Add(changer);
            return changer;
        }

        private static int WrapIndex(int index, int length) => ((index % length) + length) % length;

        private class ValueChangerDefinition
        {
            public readonly string Name;
            public readonly string Key;
            private readonly Func<string> getValue;
            private readonly Action<int> change;

            public ValueChangerDefinition(string name, Func<string> getValue, Action<int> change)
            {
                Name = name;
                Key = new string(name.Where(char.IsLetterOrDigit).ToArray());
                this.getValue = getValue;
                this.change = change;
            }

            public string Label => Name + " [" + getValue() + "]";

            public bool Matches(string buttonText) => buttonText.StartsWith(Name + " [");

            public void Change(int step) => change(step);

            public string GetControlId(bool increment) => "ValueChanger|" + Key + "|" + (increment ? "+" : "-");
        }
    }
}
