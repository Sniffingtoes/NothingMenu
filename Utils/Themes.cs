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
            new Theme { Name = "Dark", Background = new Color(0.05f, 0.05f, 0.05f), Button = new Color(0.12f, 0.12f, 0.12f), Text = Color.white },
            new Theme { Name = "Purple", Background = new Color(0.08f, 0.02f, 0.12f), Button = new Color(0.25f, 0.05f, 0.4f), Text = Color.white },
            new Theme { Name = "Red", Background = new Color(0.12f, 0.02f, 0.02f), Button = new Color(0.35f, 0.05f, 0.05f), Text = Color.white },
            new Theme { Name = "Forest", Background = new Color(0.03f, 0.08f, 0.03f), Button = new Color(0.1f, 0.25f, 0.1f), Text = Color.white },
            new Theme { Name = "Ocean", Background = new Color(0.02f, 0.08f, 0.15f), Button = new Color(0.05f, 0.25f, 0.45f), Text = Color.white },
            new Theme { Name = "Cyberpunk", Background = new Color(0.02f, 0.01f, 0.05f), Button = new Color(0.15f, 0.0f, 0.25f), Text = Color.cyan },
            new Theme { Name = "Midnight", Background = Color.black, Button = new Color(0.08f, 0.08f, 0.1f), Text = new Color(0.7f, 0.7f, 0.8f) },
            new Theme { Name = "Sandstone", Background = new Color(0.93f, 0.89f, 0.81f), Button = new Color(0.85f, 0.8f, 0.7f), Text = new Color(0.25f, 0.2f, 0.15f) },
            new Theme { Name = "Slate", Background = new Color(0.12f, 0.15f, 0.2f), Button = new Color(0.25f, 0.32f, 0.4f), Text = Color.white },
            new Theme { Name = "GameBoy", Background = new Color(0.61f, 0.73f, 0.06f), Button = new Color(0.55f, 0.67f, 0.05f), Text = new Color(0.06f, 0.22f, 0.06f) },
            new Theme { Name = "Nordic", Background = new Color(0.18f, 0.21f, 0.25f), Button = new Color(0.23f, 0.26f, 0.32f), Text = new Color(0.88f, 0.91f, 0.94f) },
            new Theme { Name = "Military", Background = new Color(0.15f, 0.16f, 0.12f), Button = new Color(0.25f, 0.27f, 0.2f), Text = new Color(0.9f, 0.9f, 0.8f) },
            new Theme { Name = "Rose", Background = new Color(0.22f, 0.12f, 0.15f), Button = new Color(0.45f, 0.25f, 0.32f), Text = new Color(1f, 0.85f, 0.9f) },
            new Theme { Name = "Volcano", Background = new Color(0.08f, 0.03f, 0.02f), Button = new Color(0.3f, 0.05f, 0.02f), Text = new Color(1f, 0.45f, 0.1f) },
            new Theme { Name = "Deep Sea", Background = new Color(0.0f, 0.05f, 0.1f), Button = new Color(0.02f, 0.15f, 0.3f), Text = new Color(0.5f, 0.9f, 1f) },
            new Theme { Name = "Gold", Background = new Color(0.1f, 0.08f, 0.03f), Button = new Color(0.3f, 0.25f, 0.1f), Text = new Color(1f, 0.85f, 0.4f) },
            new Theme { Name = "Ghost", Background = Color.white, Button = new Color(0.92f, 0.92f, 0.92f), Text = Color.black }
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
            if (currentThemeIndex >= themes.Length) currentThemeIndex = 0;
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
                    {
                        return Buttons.buttons[i][j];
                    }
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
