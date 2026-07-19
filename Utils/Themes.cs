using BepInEx;
using Nothing.Classes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nothing.Menu
{
    public class ThemeManager
    {
        public static int currentThemeIndex = 0;

        public static string GetCurrentThemeName() => themes[currentThemeIndex].Name;

        public struct Theme
        {
            public string Name;
            public Color Background;
            public Color Button;
            public Color Text;
            public Color Outline;
        }

        public static Theme[] themes = new Theme[]
        {
            new Theme { Name = "Default",    Background = new Color(0.08f, 0.14f, 0.20f), Button = new Color(0.12f, 0.19f, 0.27f), Text = Color.white },
            new Theme { Name = "Dark",       Background = new Color(0.10f, 0.10f, 0.10f), Button = new Color(0.14f, 0.14f, 0.14f), Text = Color.white },
            new Theme { Name = "Purple",     Background = new Color(0.14f, 0.08f, 0.18f), Button = new Color(0.18f, 0.12f, 0.24f), Text = Color.white },
            new Theme { Name = "Red",        Background = new Color(0.18f, 0.08f, 0.08f), Button = new Color(0.24f, 0.11f, 0.11f), Text = Color.white },
            new Theme { Name = "Forest",     Background = new Color(0.08f, 0.14f, 0.08f), Button = new Color(0.12f, 0.18f, 0.12f), Text = Color.white },
            new Theme { Name = "Cyberpunk",  Background = new Color(0.08f, 0.05f, 0.12f), Button = new Color(0.13f, 0.09f, 0.18f), Text = Color.cyan },
            new Theme { Name = "Midnight",   Background = new Color(0.07f, 0.07f, 0.09f), Button = new Color(0.11f, 0.11f, 0.14f), Text = new Color(0.75f, 0.78f, 0.85f) },
            new Theme { Name = "Sandstone",  Background = new Color(0.86f, 0.83f, 0.76f), Button = new Color(0.91f, 0.88f, 0.81f), Text = new Color(0.25f, 0.20f, 0.15f) },
            new Theme { Name = "Slate",      Background = new Color(0.16f, 0.19f, 0.24f), Button = new Color(0.20f, 0.24f, 0.30f), Text = Color.white },
            new Theme { Name = "GameBoy",    Background = new Color(0.63f, 0.74f, 0.18f), Button = new Color(0.69f, 0.80f, 0.24f), Text = new Color(0.06f, 0.22f, 0.06f) },
            new Theme { Name = "Nordic",     Background = new Color(0.22f, 0.25f, 0.30f), Button = new Color(0.27f, 0.30f, 0.36f), Text = new Color(0.90f, 0.93f, 0.96f) },
            new Theme { Name = "Military",   Background = new Color(0.19f, 0.20f, 0.16f), Button = new Color(0.24f, 0.25f, 0.20f), Text = new Color(0.90f, 0.90f, 0.82f) },
            new Theme { Name = "Rose",       Background = new Color(0.24f, 0.16f, 0.19f), Button = new Color(0.30f, 0.20f, 0.24f), Text = new Color(1f, 0.87f, 0.92f) },
            new Theme { Name = "Volcano",    Background = new Color(0.15f, 0.09f, 0.07f), Button = new Color(0.21f, 0.12f, 0.09f), Text = new Color(1f, 0.50f, 0.15f) },
            new Theme { Name = "Deep Sea",   Background = new Color(0.06f, 0.11f, 0.17f), Button = new Color(0.10f, 0.16f, 0.23f), Text = new Color(0.55f, 0.92f, 1f) },
            new Theme { Name = "Gold",       Background = new Color(0.18f, 0.15f, 0.08f), Button = new Color(0.24f, 0.20f, 0.11f), Text = new Color(1f, 0.88f, 0.45f) },
            new Theme { Name = "Ghost",      Background = new Color(0.94f, 0.94f, 0.94f), Button = new Color(0.98f, 0.98f, 0.98f), Text = Color.black }
        };

        public static Color[] textPalette = new Color[]
        {
            Color.white,
            Color.red,
            Color.green,
            Color.cyan,
            Color.magenta,
            Color.yellow,
            new Color(0.5f, 0f, 1f),
            new Color(1f, 0.5f, 0f)
        };

        public static Theme GetColors()
        {
            return themes[currentThemeIndex];
        }

        public static void CycleTheme()
        {
            currentThemeIndex++;
            if (currentThemeIndex >= themes.Length)
                currentThemeIndex = 0;

            ApplyActiveTheme();
            SaveSystem.Save();
        }

        public static void ApplyActiveTheme()
        {
            Main.RecreateMenu();
        }

        public static ButtonInfo GetButton(string name)
        {
            for (int i = 0; i < Buttons.buttons.Length; i++)
            {
                for (int j = 0; j < Buttons.buttons[i].Length; j++)
                {
                    if (Buttons.buttons[i][j].buttonText == name)
                        return Buttons.buttons[i][j];
                }
            }

            return null;
        }

        public static string GetColorName(Color c)
        {
            if (c == Color.white) return "White";
            if (c == Color.red) return "Red";
            if (c == Color.green) return "Green";
            if (c == Color.cyan) return "Cyan";
            if (c == Color.magenta) return "Purple";
            if (c == Color.yellow) return "Yellow";
            if (c.r == 0.5f && c.b == 1f) return "D-Purple";
            if (c.r == 1f && c.g == 0.5f) return "Orange";
            return "Custom";
        }
    }
}