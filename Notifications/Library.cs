using BepInEx;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nothing.Menu;
using static Nothing.Menu.UI.UiStyle;

using static Nothing.Settings;

namespace Nothing.Notifications
{
    [BepInPlugin("org.gorillatag.megamind.notifications", "NothingNotifications", "1.2.0")]
    public class NotifiLib : BaseUnityPlugin
    {
        public static bool IsInCategory = false;
        public static bool IsEnabled = true;
        public static bool IsGuiEnabled = true;
        private bool HasInit;

        public static List<NotifData> activeNotifs = new List<NotifData>();
        public static GameObject HUDObj;
        public static GameObject HUDObj2;
        public static GameObject MainCamera;

        private void Awake() => Logger.LogInfo("Nothing Notification Loaded");

        public static void SetEnabled()
        {
            IsEnabled = !IsEnabled;
            if (!IsEnabled && HUDObj != null)
            {
                foreach (var n in activeNotifs)
                {
                    if (n.container != null) UnityEngine.Object.Destroy(n.container);
                }
                HUDObj.SetActive(false);
            }
        }

        public static void SetGuiEnabled()
        {
            IsGuiEnabled = false;
        }

        private void Init()
        {
            MainCamera = GameObject.Find("Main Camera");
            HUDObj2 = new GameObject("NOTIF_PARENT");
            HUDObj = new GameObject("NOTIF_CANVAS");
            Canvas canvas = HUDObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = MainCamera.GetComponent<Camera>();
            HUDObj.AddComponent<CanvasScaler>();
            HUDObj.transform.SetParent(HUDObj2.transform, false);
            HUDObj.transform.localPosition = new Vector3(-0.25f, -0.50f, 1.1f);
            HUDObj.transform.localRotation = Quaternion.identity;
            HUDObj.transform.localScale = Vector3.one;
        }

        private void FixedUpdate()
        {
            if (!HasInit && GameObject.Find("Main Camera") != null)
            {
                Init();
                HasInit = true;
            }

            IsGuiEnabled = true;

            if (HUDObj2 != null && MainCamera != null)
            {
                HUDObj2.transform.position = MainCamera.transform.position;
                HUDObj2.transform.rotation = MainCamera.transform.rotation;
            }

            if (HUDObj != null)
            {
                bool shouldBeActive = IsEnabled && !IsInCategory && !Nothing.Settings.disableNotifications;
                if (HUDObj.activeSelf != shouldBeActive) HUDObj.SetActive(shouldBeActive);
            }

            for (int i = 0; i < activeNotifs.Count; i++)
            {
                NotifData data = activeNotifs[i];
                data.timer -= Time.fixedDeltaTime;

                if (data.accentBar != null)
                    data.accentBar.anchorMax = new Vector2(Mathf.Clamp01(data.timer / 4f), 0);

                if (data.timer <= 0)
                {
                    if (data.container != null) Destroy(data.container);
                    activeNotifs.RemoveAt(i);
                    i--;
                }
                else if (data.container != null)
                {
                    float targetY = (activeNotifs.Count - 1 - i) * 0.13f;
                    data.container.transform.localPosition = Vector3.Lerp(
                        data.container.transform.localPosition,
                        new Vector3(0f, targetY, 0f),
                        Time.fixedDeltaTime * 12f
                    );
                }
            }
        }

        private GUIStyle _notifStyle;
        private GUIStyle _notifTitleStyle;
        private Texture2D _notifBgTex;
        private Texture2D _notifAccentTex;
        private Texture2D _notifShadowTex;
        private int _notifThemeIndex = -1;

        private void OnGUI()
        {
            if (!IsGuiEnabled || IsInCategory) return;

            if (_notifStyle == null || _notifThemeIndex != ThemeManager.currentThemeIndex)
            {
                ThemeManager.Theme theme = ThemeManager.GetColors();
                _notifBgTex = MakeTex(WithAlpha(Mix(theme.Background, Color.black, 0.8f), 0.98f));
                _notifAccentTex = MakeTex(WithAlpha(Mix(theme.Button, theme.Text, 0.6f), 1f));
                _notifShadowTex = MakeTex(new Color(0f, 0f, 0f, 0.48f));
                _notifTitleStyle = new GUIStyle(GUI.skin.label);
                _notifTitleStyle.alignment = TextAnchor.MiddleLeft;
                _notifTitleStyle.fontSize = 10;
                _notifTitleStyle.fontStyle = FontStyle.Bold;
                _notifTitleStyle.normal.textColor = Mix(theme.Text, theme.Background, 0.3f);

                _notifStyle = new GUIStyle(GUI.skin.label);
                _notifStyle.alignment = TextAnchor.MiddleLeft;
                _notifStyle.fontSize = 15;
                _notifStyle.fontStyle = FontStyle.Bold;
                _notifStyle.normal.textColor = theme.Text;
                _notifStyle.padding = new RectOffset(20, 14, 0, 0);
                _notifThemeIndex = ThemeManager.currentThemeIndex;
            }

            for (int i = 0; i < activeNotifs.Count; i++)
            {
                float yPos = Screen.height - 24f - 64f - ((activeNotifs.Count - 1 - i) * 72f);
                Rect rect = new Rect(22f, yPos, 360f, 64f);
                GUI.DrawTexture(new Rect(rect.x + 7f, rect.y + 9f, rect.width, rect.height), _notifShadowTex);
                GUI.DrawTexture(rect, _notifBgTex);
                GUI.DrawTexture(new Rect(rect.x, rect.y, 5f, rect.height), _notifAccentTex);
                GUI.DrawTexture(new Rect(rect.x + 20f, rect.yMax - 3f, rect.width - 40f, 2f), _notifAccentTex);
                GUI.Label(new Rect(rect.x, rect.y + 29f, rect.width, 24f), activeNotifs[i].notificationText, _notifStyle);
            }
        }

        public static void SendNotification(string text)
        {
            if (activeNotifs.Count > 4)
            {
                if (activeNotifs[0].container != null) Destroy(activeNotifs[0].container);
                activeNotifs.RemoveAt(0);
            }

            GameObject container = null;
            RectTransform barRect = null;

            if (IsEnabled && !Nothing.Settings.disableNotifications && !IsInCategory && HUDObj != null)
            {
                ThemeManager.Theme theme = ThemeManager.GetColors();
                Color accent = Mix(theme.Button, theme.Text, 0.6f);
                container = new GameObject("Notif_Container");
                container.transform.SetParent(HUDObj.transform, false);
                RectTransform contRect = container.AddComponent<RectTransform>();
                contRect.sizeDelta = new Vector2(0.46f, 0.14f);
                Image bgImage = container.AddComponent<Image>();
                bgImage.color = WithAlpha(Mix(theme.Background, Color.black, 0.8f), 0.98f);

                GameObject bar = new GameObject("Bar");
                bar.transform.SetParent(container.transform, false);
                bar.transform.localPosition = new Vector3(0, 0, -0.001f);
                Image barImg = bar.AddComponent<Image>();
                barImg.color = accent;
                barRect = barImg.rectTransform;
                barRect.anchorMin = Vector2.zero;
                barRect.anchorMax = new Vector2(1, 0);
                barRect.sizeDelta = new Vector2(0, 0.006f);
                barRect.pivot = Vector2.zero;

                GameObject titleObj = new GameObject("Title");
                titleObj.transform.SetParent(container.transform, false);
                titleObj.transform.localPosition = new Vector3(-0.055f, 0.042f, -0.002f);
                titleObj.transform.localScale = Vector3.one * 0.001f;
                Text titleT = titleObj.AddComponent<Text>();
                titleT.text = "Notification";
                titleT.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                titleT.fontStyle = FontStyle.Bold;
                titleT.fontSize = 36;
                titleT.color = Mix(theme.Text, theme.Background, 0.3f);
                titleT.rectTransform.sizeDelta = new Vector2(300, 50);

                GameObject bodyObj = new GameObject("Body");
                bodyObj.transform.SetParent(container.transform, false);
                bodyObj.transform.localPosition = new Vector3(0f, -0.03f, -0.002f);
                bodyObj.transform.localScale = Vector3.one * 0.00085f;
                Text bodyT = bodyObj.AddComponent<Text>();
                bodyT.text = text;
                bodyT.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                bodyT.fontSize = 30;
                bodyT.color = theme.Text;
                bodyT.rectTransform.sizeDelta = new Vector2(450, 100);
            }

            activeNotifs.Add(new NotifData
            {
                container = container,
                accentBar = barRect,
                timer = 4f,
                notificationText = text
            });
        }

        public class NotifData
        {
            public GameObject container;
            public RectTransform accentBar;
            public float timer;
            public string notificationText;
        }
    }
}
