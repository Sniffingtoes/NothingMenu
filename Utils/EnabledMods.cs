using Nothing.Classes;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Nothing.Menu
{
    public static class EnabledMods
    {
        private static readonly HashSet<string> DisabledServerChecks = new HashSet<string>
        {
            "Anti Report",
            "No Tag On Join"
        };

        public static readonly List<ButtonInfo> RuntimeEnabledButtons = new List<ButtonInfo>(32);

        public static bool IsDisabledServerCheck(string buttonText) =>
            buttonText != null && DisabledServerChecks.Contains(buttonText);

        public static void RebuildRuntimeList()
        {
            RuntimeEnabledButtons.Clear();
            HashSet<string> seen = new HashSet<string>();

            for (int i = 0; i < Buttons.buttons.Length; i++)
            {
                if (i == 2 || i == 3 || Buttons.buttons[i] == null) continue;

                foreach (ButtonInfo btn in Buttons.buttons[i])
                {
                    if (!btn.enabled || btn.method == null || IsDisabledServerCheck(btn.buttonText)) continue;
                    if (seen.Add(btn.buttonText))
                        RuntimeEnabledButtons.Add(btn);
                }
            }
        }

        public static void RefreshEnabledTab()
        {
            List<ButtonInfo> enabledList = new List<ButtonInfo>();

            enabledList.Add(new ButtonInfo
            {
                buttonText = "Return to Main",
                method = () => Main.currentCategory = 0,
                isTogglable = false
            });

            for (int i = 0; i < Buttons.buttons.Length; i++)
            {
                if (i == 2) continue;

                foreach (ButtonInfo btn in Buttons.buttons[i])
                {
                    if (IsDisabledServerCheck(btn.buttonText))
                    {
                        btn.enabled = false;
                        continue;
                    }

                    if (btn.enabled)
                    {
                        if (!enabledList.Any(b => b.buttonText == btn.buttonText))
                        {
                            enabledList.Add(btn);
                        }
                    }
                }
            }

            Buttons.buttons[2] = enabledList.ToArray();
            RebuildRuntimeList();
        }
    }
}
