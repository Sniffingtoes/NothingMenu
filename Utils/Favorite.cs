using Nothing.Classes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nothing.Menu
{
    public static class FavoriteMods
    {
        public static List<string> favoritedNames = new List<string>();

        public static void RefreshFavoriteTab()
        {
            List<ButtonInfo> favoriteList = new List<ButtonInfo>();

            favoriteList.Add(new ButtonInfo
            {
                buttonText = "Return to Main",
                method = () => Main.currentCategory = 0,
                isTogglable = false
            });

            for (int i = 0; i < Buttons.buttons.Length; i++)
            {
                if (i == 3) continue;

                foreach (ButtonInfo btn in Buttons.buttons[i])
                {
                    if (favoritedNames.Contains(btn.buttonText))
                    {
                        if (!favoriteList.Any(b => b.buttonText == btn.buttonText))
                        {
                            favoriteList.Add(btn);
                        }
                    }
                }
            }

            Buttons.buttons[3] = favoriteList.ToArray();
        }

        public static void ToggleFavorite(string buttonName)
        {
            if (favoritedNames.Contains(buttonName))
            {
                favoritedNames.Remove(buttonName);
            }
            else
            {
                favoritedNames.Add(buttonName);
            }
            RefreshFavoriteTab();
            SaveSystem.Save();
        }
    }
}
