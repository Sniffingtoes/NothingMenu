using BepInEx;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using static Nothing.Settings;

namespace Nothing.Notifications
{
    [BepInPlugin("org.gorillatag.megamind.notifications", "NotificationLibrary", "1.0.0")]
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

        private void Awake() => Logger.LogInfo("Notification Library Loaded");

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

        private void OnGUI()
        {
            if (!IsGuiEnabled || IsInCategory) return;

            if (_notifStyle == null)
            {
                _notifStyle = new GUIStyle(GUI.skin.box);
                _notifStyle.alignment = TextAnchor.MiddleLeft;
                _notifStyle.fontSize = 14;
                _notifStyle.normal.textColor = Color.white;
            }

            for (int i = 0; i < activeNotifs.Count; i++)
            {
                float yPos = 10 + (i * 35);
                GUI.Box(new Rect(10, yPos, 250, 30), " [!] " + activeNotifs[i].notificationText, _notifStyle);
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
                container = new GameObject("Notif_Container");
                container.transform.SetParent(HUDObj.transform, false);
                RectTransform contRect = container.AddComponent<RectTransform>();
                contRect.sizeDelta = new Vector2(0.42f, 0.12f);
                Image bgImage = container.AddComponent<Image>();
                bgImage.color = new Color(0.04f, 0.04f, 0.04f, 0.94f);

                GameObject bar = new GameObject("Bar");
                bar.transform.SetParent(container.transform, false);
                bar.transform.localPosition = new Vector3(0, 0, -0.001f);
                Image barImg = bar.AddComponent<Image>();
                barImg.color = new Color(0.2f, 0.55f, 1f, 1f);
                barRect = barImg.rectTransform;
                barRect.anchorMin = Vector2.zero;
                barRect.anchorMax = new Vector2(1, 0);
                barRect.sizeDelta = new Vector2(0, 0.006f);
                barRect.pivot = Vector2.zero;

                GameObject titleObj = new GameObject("Title");
                titleObj.transform.SetParent(container.transform, false);
                titleObj.transform.localPosition = new Vector3(-0.05f, 0.035f, -0.002f);
                titleObj.transform.localScale = Vector3.one * 0.001f;
                Text titleT = titleObj.AddComponent<Text>();
                titleT.text = "Notification";
                titleT.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                titleT.fontStyle = FontStyle.Bold;
                titleT.fontSize = 36;
                titleT.color = Color.white;
                titleT.rectTransform.sizeDelta = new Vector2(300, 50);

                GameObject bodyObj = new GameObject("Body");
                bodyObj.transform.SetParent(container.transform, false);
                bodyObj.transform.localPosition = new Vector3(0f, -0.028f, -0.002f);
                bodyObj.transform.localScale = Vector3.one * 0.00085f;
                Text bodyT = bodyObj.AddComponent<Text>();
                bodyT.text = text;
                bodyT.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                bodyT.fontSize = 30;
                bodyT.color = new Color(0.9f, 0.9f, 0.9f, 1f);
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
