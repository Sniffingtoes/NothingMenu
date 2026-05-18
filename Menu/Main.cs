using BepInEx;
using Nothing.Classes;
using Nothing.Notifications;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;
using Custom.Inputs;

using static Nothing.Menu.Buttons;
using static Nothing.Settings;

namespace Nothing.Menu
{
    [HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
    public partial class Main : MonoBehaviour
    {
        public static bool hasLoadedSave = false;
        public static bool menuPinned = false;
        public static bool searchActive = false;
        public static bool pcSearchMode = false;
        public static string searchQuery = "";
        private const float SnapDistance = 1.2f;

        private static TMP_Text _cachedCocHeading, _cachedCocBody, _cachedMotdHeading, _cachedMotdBody;
        private static bool _hudTextsSet = false;
        private static TMP_FontAsset _cachedMotdFont;
        private static Shader _cachedUberShader;
        private static GameObject _cachedVcam1;
        private static GameObject _keyboardBackgroundCameraObject;
        private static Sprite _cachedSearchSprite;
        private static Sprite _cachedSettingsSprite;
        private static readonly Dictionary<int, Material> _materialCache = new Dictionary<int, Material>();
        private static readonly KeyCode[] _keyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));
        public static Shader UberShader => _cachedUberShader ??= Shader.Find("GorillaTag/UberShader");

        private static void EnsureHudTexts()
        {
            if (_hudTextsSet) return;

            _cachedCocHeading ??= GameObject.Find("cocHeadingText")?.GetComponent<TMP_Text>();
            _cachedCocBody ??= GameObject.Find("cocBodyText")?.GetComponent<TMP_Text>();
            _cachedMotdHeading ??= GameObject.Find("motdHeadingText")?.GetComponent<TMP_Text>();
            _cachedMotdBody ??= GameObject.Find("motdBodyText")?.GetComponent<TMP_Text>();

            if (_cachedCocHeading != null) _cachedCocHeading.text = "\n<color=blue>Nothing Menu</color>";
            if (_cachedCocBody != null)
            {
                _cachedCocBody.text = "";
                _cachedCocBody.fontSize = 50f;
            }
            if (_cachedMotdHeading != null) _cachedMotdHeading.text = "\n<color=blue>Nothing Menu</color>";
            if (_cachedMotdBody != null)
            {
                _cachedMotdBody.text = $"\n\nty for choosing Nothing Menu <3\n\n- this is a fun project of mine\n- there are over 70+ mods\n- Current version: {PluginInfo.Version} \n \nhave fun using this menu";
                _cachedMotdBody.fontSize = 65f;
            }

            _hudTextsSet = _cachedCocHeading != null && _cachedCocBody != null && _cachedMotdHeading != null && _cachedMotdBody != null;
        }

        private static void ApplyMenuMaterial(Renderer renderer, Color color)
        {
            if (renderer == null) return;

            Color32 c = color;
            int key = (c.r << 24) | (c.g << 16) | (c.b << 8) | c.a;
            if (!_materialCache.TryGetValue(key, out Material material) || material == null)
            {
                material = new Material(UberShader) { color = color };
                _materialCache[key] = material;
            }

            renderer.sharedMaterial = material;
        }

        private static Sprite LoadEmbeddedSprite(ref Sprite cachedSprite, string resourceName)
        {
            if (cachedSprite != null) return cachedSprite;

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;

                byte[] imgBytes = new byte[stream.Length];
                stream.Read(imgBytes, 0, imgBytes.Length);

                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(imgBytes);
                tex.filterMode = FilterMode.Bilinear;

                cachedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                return cachedSprite;
            }
        }

        private static bool ButtonMatchesSearch(ButtonInfo button, string query) =>
            button?.buttonText != null && button.buttonText.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

        private static void CleanupKeyboardBackgroundCamera()
        {
            if (_keyboardBackgroundCameraObject == null) return;

            Destroy(_keyboardBackgroundCameraObject);
            _keyboardBackgroundCameraObject = null;
        }

        private static void EnsureKeyboardBackgroundCamera(float depth)
        {
            if (_keyboardBackgroundCameraObject != null) return;

            _keyboardBackgroundCameraObject = new GameObject("MenuBackgroundCamera");
            Camera bgCam = _keyboardBackgroundCameraObject.AddComponent<Camera>();
            _keyboardBackgroundCameraObject.transform.position = new Vector3(-68.5885f, 12.0878f, -83.9583f);
            _keyboardBackgroundCameraObject.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
            bgCam.fieldOfView = 75f;
            bgCam.depth = depth;
            bgCam.farClipPlane = 2000f;
        }

        public static void Prefix()
        {
            EnsureHudTexts();

            try
            {
                bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerSecondaryButton);
                bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton);
                bool effectiveKeyboard = keyboardOpen || (searchActive && pcSearchMode);

                if (toOpen || keyboardOpen || menuPinned || searchActive)
                {
                    if (menu == null)
                    {
                        SaveSystem.Load();
                        CreateMenu();
                        if (effectiveKeyboard)
                            RecenterMenu(rightHanded, true);
                        else if (searchActive)
                            PinMenuInFrontOfPlayer();
                        else
                            RecenterMenu(rightHanded, false);
                        if (reference == null) CreateReference(rightHanded);
                        if (searchActive && reference2 == null) CreateReference2(rightHanded);
                    }
                    else
                    {
                        if (!menuPinned)
                        {
                            if (effectiveKeyboard)
                                RecenterMenu(rightHanded, true);
                            else if (searchActive && !pcSearchMode)
                                PinMenuInFrontOfPlayer();
                            else if (!searchActive)
                                RecenterMenu(rightHanded, false);
                        }
                        else
                        {
                            Transform head = GorillaTagger.Instance.headCollider.transform;
                            if (Vector3.Distance(menu.transform.position, head.position) > SnapDistance)
                                PinMenuInFrontOfPlayer();
                        }

                        if (searchActive && reference2 == null) CreateReference2(rightHanded);
                        if (!searchActive && reference2 != null)
                        {
                            Destroy(reference2);
                            reference2 = null;
                        }
                    }
                }
                else
                {
                    if (menu != null)
                    {
                        if (_cachedVcam1 == null)
                            _cachedVcam1 = GameObject.Find("Shoulder Camera")?.transform.Find("CM vcam1")?.gameObject;
                        _cachedVcam1?.SetActive(true);
                        CleanupKeyboardBackgroundCamera();

                        Rigidbody comp = menu.AddComponent<Rigidbody>();
                        comp.linearVelocity = (rightHanded ? GTPlayer.Instance.LeftHand.velocityTracker : GTPlayer.Instance.RightHand.velocityTracker).GetAverageVelocity(true, 0);
                        Destroy(menu);
                        menu = null;
                        fpsText = null;
                        Destroy(reference);
                        reference = null;
                        if (reference2 != null) { Destroy(reference2); reference2 = null; }
                        searchQuery = "";
                        pcSearchMode = false;
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Menu Error: " + exc.Message);
            }

            try
            {
                if (fpsText != null)
                    fpsText.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();

                string lq = (searchActive && searchQuery.Length > 0) ? searchQuery : null;
                HashSet<string> invokedSearchButtons = lq != null ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : null;
                foreach (ButtonInfo button in EnabledMods.RuntimeEnabledButtons)
                {
                    if (!button.enabled || button.method == null) continue;
                    if (lq != null && !ButtonMatchesSearch(button, lq)) continue;
                    if (lq != null && !invokedSearchButtons.Add(button.buttonText)) continue;
                    try { button.method.Invoke(); }
                    catch (Exception exc) { Debug.LogError($"{PluginInfo.Name} - Error with mod {button.buttonText} at {exc.StackTrace}: {exc.Message}"); }
                }
            }
            catch (Exception exc)
            {
                Debug.LogError($"{PluginInfo.Name} - Error executing mods: {exc.Message}");
            }

            if (searchActive && pcSearchMode)
            {
                foreach (KeyCode kc in _keyCodes)
                {
                    bool isDown = false;
                    try { isDown = UnityInput.Current.GetKeyDown(kc); } catch { continue; }
                    if (!isDown) continue;

                    if (kc == KeyCode.Backspace)
                    {
                        if (searchQuery.Length > 0)
                            searchQuery = searchQuery.Substring(0, searchQuery.Length - 1);
                        pageNumber = 0;
                        RecreateMenu();
                        break;
                    }
                    else if (kc == KeyCode.Escape)
                    {
                        searchActive = false;
                        searchQuery = "";
                        pcSearchMode = false;
                        pageNumber = 0;
                        RecreateMenu();
                        break;
                    }
                    else if (kc == KeyCode.Space)
                    {
                        searchQuery += " ";
                        pageNumber = 0;
                        RecreateMenu();
                        break;
                    }
                    else
                    {
                        string s = kc.ToString();
                        if (s.Length == 1 || (s.StartsWith("Alpha") && s.Length == 6))
                        {
                            char c;
                            if (s.StartsWith("Alpha"))
                                c = s[5];
                            else
                                c = char.ToUpper(s[0]);

                            searchQuery += c;
                            pageNumber = 0;
                            RecreateMenu();
                            break;
                        }
                    }
                }
            }

        }

        public static GameObject BG;
        public static List<GameObject> allButtons = new List<GameObject>();
        public static List<TextMeshPro> allTexts = new List<TextMeshPro>();
        public static GameObject catBar;

        public static void CreateMenu()
        {
            allButtons.Clear();
            allTexts.Clear();

            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

            GameObject roundedBG = new GameObject("Background");
            roundedBG.transform.parent = menu.transform;
            roundedBG.transform.localRotation = Quaternion.identity;
            roundedBG.transform.localPosition = new Vector3(0.5f, 0f, 0f);

            float bgWidth = 0.19f;
            float bgHeight = 0.3f;
            float bgThick = 0.01f;
            float bgRad = 0.015f;

            ThemeManager.Theme bgT = ThemeManager.themes[ThemeManager.currentThemeIndex];

            void AddBGPart(PrimitiveType type, Vector3 pos, Vector3 scale)
            {
                GameObject part = GameObject.CreatePrimitive(type);
                UnityEngine.Object.Destroy(part.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(part.GetComponent<Collider>());
                part.transform.parent = roundedBG.transform;
                part.transform.localPosition = pos;
                part.transform.localScale = scale;

                if (type == PrimitiveType.Cylinder)
                    part.transform.localRotation = Quaternion.Euler(0, 0, 90);

                ApplyMenuMaterial(part.GetComponent<Renderer>(), bgT.Background);
            }

            float bgX = (bgWidth / 2) - bgRad;
            float bgZ = (bgHeight / 2) - bgRad;
            Vector3 bgCylScale = new Vector3(bgRad * 2, bgThick / 2.01f, bgRad * 2);

            AddBGPart(PrimitiveType.Cylinder, new Vector3(0, bgX, bgZ), bgCylScale);
            AddBGPart(PrimitiveType.Cylinder, new Vector3(0, -bgX, bgZ), bgCylScale);
            AddBGPart(PrimitiveType.Cylinder, new Vector3(0, bgX, -bgZ), bgCylScale);
            AddBGPart(PrimitiveType.Cylinder, new Vector3(0, -bgX, -bgZ), bgCylScale);

            AddBGPart(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(bgThick, bgWidth - (bgRad * 2), bgHeight));
            AddBGPart(PrimitiveType.Cube, new Vector3(0, bgX, 0), new Vector3(bgThick, bgRad * 2, bgHeight - (bgRad * 2)));
            AddBGPart(PrimitiveType.Cube, new Vector3(0, -bgX, 0), new Vector3(bgThick, bgRad * 2, bgHeight - (bgRad * 2)));

            if (menuBackground != null) UnityEngine.Object.Destroy(menuBackground);

            canvasObject = new GameObject("MenuCanvas");
            canvasObject.transform.parent = menu.transform;
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObject.layer = 0;

            if (_cachedMotdFont == null)
            {
                GameObject motd = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText");
                if (motd != null) _cachedMotdFont = motd.GetComponent<TextMeshPro>().font;
            }
            TMP_FontAsset motdFont = _cachedMotdFont;

            void CreateMenuText(string textContent, Vector3 localPos, Vector2 sizeDelta, float fontSize = 2.5f)
            {
                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(canvasObject.transform, false);
                txtObj.layer = 0;

                TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
                tmp.text = textContent;
                tmp.fontSize = fontSize;
                tmp.color = ThemeManager.themes[ThemeManager.currentThemeIndex].Text;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.overflowMode = TextOverflowModes.Overflow;
                if (motdFont != null) tmp.font = motdFont;

                RectTransform rect = tmp.GetComponent<RectTransform>();
                rect.sizeDelta = sizeDelta;

                float posX = (localPos.x == 0f) ? 0.06f : localPos.x;
                rect.localPosition = new Vector3(posX, localPos.y, localPos.z);

                rect.localRotation = Quaternion.Euler(0f, -90f, -90f);
                rect.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                allTexts.Add(tmp);
            }

            CreateMenuText(PluginInfo.Name + " V1.3", new Vector3(0.056f, 0f, 0.13f), new Vector2(10f, 4f), 2f);

            if (fpsCounter)
            {
                string fpsStr = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                CreateMenuText(fpsStr, new Vector3(0.056f, 0f, 0.11f), new Vector2(8f, 2f), 1.8f);
                fpsText = allTexts[allTexts.Count - 1];
            }

            if (disconnectButton)
            {
                GameObject dcBtnObj = new GameObject("DisconnectButton");
                dcBtnObj.transform.parent = menu.transform;
                dcBtnObj.transform.rotation = Quaternion.identity;
                dcBtnObj.transform.localPosition = new Vector3(0.5f, 0f, 0.48f);

                float dWidth = 0.19f;
                float dHeight = 0.035f;
                float dThick = 0.005f;
                float dRad = 0.01f;

                ThemeManager.Theme dcT = ThemeManager.themes[ThemeManager.currentThemeIndex];

                void AddDisconnectPart(PrimitiveType dType, Vector3 dPos, Vector3 dScale)
                {
                    GameObject dPart = GameObject.CreatePrimitive(dType);
                    UnityEngine.Object.Destroy(dPart.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(dPart.GetComponent<Collider>());
                    dPart.transform.parent = dcBtnObj.transform;
                    dPart.transform.localPosition = dPos;
                    dPart.transform.localScale = dScale;

                    if (dType == PrimitiveType.Cylinder)
                        dPart.transform.localRotation = Quaternion.Euler(0, 0, 90);

                    ApplyMenuMaterial(dPart.GetComponent<Renderer>(), dcT.Button);
                }

                float dX = (dWidth / 2) - dRad;
                float dZ = (dHeight / 2) - dRad;
                Vector3 dCylScale = new Vector3(dRad * 2, dThick / 2.01f, dRad * 2);

                AddDisconnectPart(PrimitiveType.Cylinder, new Vector3(0, dX, dZ), dCylScale);
                AddDisconnectPart(PrimitiveType.Cylinder, new Vector3(0, -dX, dZ), dCylScale);
                AddDisconnectPart(PrimitiveType.Cylinder, new Vector3(0, dX, -dZ), dCylScale);
                AddDisconnectPart(PrimitiveType.Cylinder, new Vector3(0, -dX, -dZ), dCylScale);

                AddDisconnectPart(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(dThick, dWidth - (dRad * 2), dHeight));
                AddDisconnectPart(PrimitiveType.Cube, new Vector3(0, dX, 0), new Vector3(dThick, dRad * 2, dHeight - (dRad * 2)));
                AddDisconnectPart(PrimitiveType.Cube, new Vector3(0, -dX, 0), new Vector3(dThick, dRad * 2, dHeight - (dRad * 2)));

                if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive) dcBtnObj.layer = 2;

                BoxCollider dTrigger = dcBtnObj.AddComponent<BoxCollider>();
                dTrigger.isTrigger = true;
                dTrigger.size = new Vector3(dThick * 2f, dWidth, dHeight);
                dTrigger.center = Vector3.zero;

                dcBtnObj.AddComponent<Classes.Button>().relatedText = "Disconnect";
                allButtons.Add(dcBtnObj);

                CreateMenuText("Disconnect", new Vector3(0f, 0f, 0.18f), new Vector2(10f, 3f), 2.5f);
            }

            {
                GameObject searchBtnObj = new GameObject("SearchButton");
                searchBtnObj.transform.parent = menu.transform;
                searchBtnObj.transform.rotation = Quaternion.identity;
                searchBtnObj.transform.localPosition = new Vector3(0.5f, -0.4f, 0.48f);

                float pBtnWidth = 0.035f;
                float pBtnHeight = 0.035f;
                float pBtnThick = 0.005f;
                float pBtnRad = 0.01f;

                ThemeManager.Theme searchT = ThemeManager.themes[ThemeManager.currentThemeIndex];
                Color searchColor = searchT.Button;

                void AddSearchPart(PrimitiveType pType, Vector3 pPos, Vector3 pScale)
                {
                    GameObject pPart = GameObject.CreatePrimitive(pType);
                    UnityEngine.Object.Destroy(pPart.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pPart.GetComponent<Collider>());
                    pPart.transform.parent = searchBtnObj.transform;
                    pPart.transform.localPosition = pPos;
                    pPart.transform.localScale = pScale;
                    pPart.transform.localRotation = (pType == PrimitiveType.Cylinder) ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;

                    ApplyMenuMaterial(pPart.GetComponent<Renderer>(), searchColor);
                }

                float searchBtnX = (pBtnWidth / 2) - pBtnRad;
                float searchBtnZ = (pBtnHeight / 2) - pBtnRad;
                Vector3 searchCylScale = new Vector3(pBtnRad * 2, pBtnThick / 2.01f, pBtnRad * 2);

                AddSearchPart(PrimitiveType.Cylinder, new Vector3(0, searchBtnX, searchBtnZ), searchCylScale);
                AddSearchPart(PrimitiveType.Cylinder, new Vector3(0, -searchBtnX, searchBtnZ), searchCylScale);
                AddSearchPart(PrimitiveType.Cylinder, new Vector3(0, searchBtnX, -searchBtnZ), searchCylScale);
                AddSearchPart(PrimitiveType.Cylinder, new Vector3(0, -searchBtnX, -searchBtnZ), searchCylScale);

                AddSearchPart(PrimitiveType.Cube, Vector3.zero, new Vector3(pBtnThick, pBtnWidth - (pBtnRad * 2), pBtnHeight));
                AddSearchPart(PrimitiveType.Cube, new Vector3(0, searchBtnX, 0), new Vector3(pBtnThick, pBtnRad * 2, pBtnHeight - (pBtnRad * 2)));
                AddSearchPart(PrimitiveType.Cube, new Vector3(0, -searchBtnX, 0), new Vector3(pBtnThick, pBtnRad * 2, pBtnHeight - (pBtnRad * 2)));

                if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive) searchBtnObj.layer = 2;

                BoxCollider searchTrigger = searchBtnObj.AddComponent<BoxCollider>();
                searchTrigger.isTrigger = true;
                searchTrigger.size = new Vector3(pBtnThick * 2f, pBtnWidth, pBtnHeight);
                searchTrigger.center = Vector3.zero;

                searchBtnObj.AddComponent<Classes.Button>().relatedText = "SearchButton";
                allButtons.Add(searchBtnObj);

                Sprite searchSprite = LoadEmbeddedSprite(ref _cachedSearchSprite, "NothingMenu.Resources.search.png");
                if (searchSprite != null)
                {
                    GameObject iconObj = new GameObject("SearchIcon");
                    iconObj.transform.parent = searchBtnObj.transform;
                    iconObj.layer = 0;

                    SpriteRenderer sr = iconObj.AddComponent<SpriteRenderer>();
                    sr.sprite = searchSprite;
                    sr.color = ThemeManager.themes[ThemeManager.currentThemeIndex].Text;

                    iconObj.transform.localPosition = new Vector3(0.007f, 0f, 0f);
                    iconObj.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);

                    float iconScale = 0.0015f;
                    iconObj.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
                }
            }

            {
                GameObject settingsBtnObj = new GameObject("SettingsButton");
                settingsBtnObj.transform.parent = menu.transform;
                settingsBtnObj.transform.rotation = Quaternion.identity;
                settingsBtnObj.transform.localPosition = new Vector3(0.5f, -0.4f, 0.38f);

                float sBtnWidth = 0.035f;
                float sBtnHeight = 0.035f;
                float sBtnThick = 0.005f;
                float sBtnRad = 0.01f;

                ThemeManager.Theme settingsT = ThemeManager.themes[ThemeManager.currentThemeIndex];
                Color settingsColor = settingsT.Button;

                void AddSettingsPart(PrimitiveType sType2, Vector3 sPos2, Vector3 sScale2)
                {
                    GameObject sPart2 = GameObject.CreatePrimitive(sType2);
                    UnityEngine.Object.Destroy(sPart2.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(sPart2.GetComponent<Collider>());
                    sPart2.transform.parent = settingsBtnObj.transform;
                    sPart2.transform.localPosition = sPos2;
                    sPart2.transform.localScale = sScale2;
                    sPart2.transform.localRotation = (sType2 == PrimitiveType.Cylinder) ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;

                    ApplyMenuMaterial(sPart2.GetComponent<Renderer>(), settingsColor);
                }

                float settingsBtnX = (sBtnWidth / 2) - sBtnRad;
                float settingsBtnZ = (sBtnHeight / 2) - sBtnRad;
                Vector3 settingsCylScale = new Vector3(sBtnRad * 2, sBtnThick / 2.01f, sBtnRad * 2);

                AddSettingsPart(PrimitiveType.Cylinder, new Vector3(0, settingsBtnX, settingsBtnZ), settingsCylScale);
                AddSettingsPart(PrimitiveType.Cylinder, new Vector3(0, -settingsBtnX, settingsBtnZ), settingsCylScale);
                AddSettingsPart(PrimitiveType.Cylinder, new Vector3(0, settingsBtnX, -settingsBtnZ), settingsCylScale);
                AddSettingsPart(PrimitiveType.Cylinder, new Vector3(0, -settingsBtnX, -settingsBtnZ), settingsCylScale);

                AddSettingsPart(PrimitiveType.Cube, Vector3.zero, new Vector3(sBtnThick, sBtnWidth - (sBtnRad * 2), sBtnHeight));
                AddSettingsPart(PrimitiveType.Cube, new Vector3(0, settingsBtnX, 0), new Vector3(sBtnThick, sBtnRad * 2, sBtnHeight - (sBtnRad * 2)));
                AddSettingsPart(PrimitiveType.Cube, new Vector3(0, -settingsBtnX, 0), new Vector3(sBtnThick, sBtnRad * 2, sBtnHeight - (sBtnRad * 2)));

                if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive) settingsBtnObj.layer = 2;

                BoxCollider settingsTrigger = settingsBtnObj.AddComponent<BoxCollider>();
                settingsTrigger.isTrigger = true;
                settingsTrigger.size = new Vector3(sBtnThick * 2f, sBtnWidth, sBtnHeight);
                settingsTrigger.center = Vector3.zero;

                settingsBtnObj.AddComponent<Classes.Button>().relatedText = "SettingsButton";
                allButtons.Add(settingsBtnObj);

                Sprite settingsSprite = LoadEmbeddedSprite(ref _cachedSettingsSprite, "NothingMenu.Resources.settings.png");
                if (settingsSprite != null)
                {
                    GameObject iconObj = new GameObject("SettingsIcon");
                    iconObj.transform.parent = settingsBtnObj.transform;
                    iconObj.layer = 0;

                    SpriteRenderer sr = iconObj.AddComponent<SpriteRenderer>();
                    sr.sprite = settingsSprite;
                    sr.color = ThemeManager.themes[ThemeManager.currentThemeIndex].Text;

                    iconObj.transform.localPosition = new Vector3(0.007f, 0f, 0f);
                    iconObj.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);

                    float iconScale = 0.0015f;
                    iconObj.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
                }
            }

            if (searchActive && !pcSearchMode)
            {
                CreateSearchKeyboard(motdFont);
            }

            if (searchActive && pcSearchMode)
            {
                GameObject qTxtObj = new GameObject("SearchQueryDisplay");
                qTxtObj.transform.SetParent(canvasObject.transform, false);
                qTxtObj.layer = 0;
                TextMeshPro qTmp = qTxtObj.AddComponent<TextMeshPro>();
                if (motdFont != null) qTmp.font = motdFont;
                qTmp.text = searchQuery.Length > 0 ? searchQuery : "Type to search...";
                qTmp.fontSize = 3f;
                qTmp.alignment = TextAlignmentOptions.Center;
                qTmp.color = ThemeManager.themes[ThemeManager.currentThemeIndex].Text;
                qTmp.overflowMode = TextOverflowModes.Overflow;
                RectTransform qRect = qTmp.GetComponent<RectTransform>();
                qRect.sizeDelta = new Vector2(14f, 3f);
                qRect.localPosition = new Vector3(0.06f, 0f, -0.2f);
                qRect.localRotation = Quaternion.Euler(0f, -90f, -90f);
                qRect.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            }

            GameObject prevBtnObj = new GameObject("PrevButton");
            prevBtnObj.transform.parent = menu.transform;
            prevBtnObj.transform.rotation = Quaternion.identity;
            prevBtnObj.transform.localPosition = new Vector3(0.56f, 0.2f, -0.32f);

            float pWidth = 0.055f;
            float pHeight = 0.04f;
            float pThick = 0.005f;
            float pRad = 0.012f;

            ThemeManager.Theme prevT = ThemeManager.themes[ThemeManager.currentThemeIndex];

            void AddPrevPart(PrimitiveType pType, Vector3 pPos, Vector3 pScale)
            {
                GameObject pPart = GameObject.CreatePrimitive(pType);
                UnityEngine.Object.Destroy(pPart.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(pPart.GetComponent<Collider>());
                pPart.transform.parent = prevBtnObj.transform;
                pPart.transform.localPosition = pPos;
                pPart.transform.localScale = pScale;

                if (pType == PrimitiveType.Cylinder)
                    pPart.transform.localRotation = Quaternion.Euler(0, 0, 90);

                ApplyMenuMaterial(pPart.GetComponent<Renderer>(), prevT.Button);
            }

            float pX = (pWidth / 2) - pRad;
            float pZ = (pHeight / 2) - pRad;
            Vector3 pCylScale = new Vector3(pRad * 2, pThick / 2.01f, pRad * 2);

            AddPrevPart(PrimitiveType.Cylinder, new Vector3(0, pX, pZ), pCylScale);
            AddPrevPart(PrimitiveType.Cylinder, new Vector3(0, -pX, pZ), pCylScale);
            AddPrevPart(PrimitiveType.Cylinder, new Vector3(0, pX, -pZ), pCylScale);
            AddPrevPart(PrimitiveType.Cylinder, new Vector3(0, -pX, -pZ), pCylScale);

            AddPrevPart(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(pThick, pWidth - (pRad * 2), pHeight));
            AddPrevPart(PrimitiveType.Cube, new Vector3(0, pX, 0), new Vector3(pThick, pRad * 2, pHeight - (pRad * 2)));
            AddPrevPart(PrimitiveType.Cube, new Vector3(0, -pX, 0), new Vector3(pThick, pRad * 2, pHeight - (pRad * 2)));

            if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive) prevBtnObj.layer = 2;

            BoxCollider pTrigger = prevBtnObj.AddComponent<BoxCollider>();
            pTrigger.isTrigger = true;
            pTrigger.size = new Vector3(pThick * 2f, pWidth, pHeight);
            pTrigger.center = Vector3.zero;

            prevBtnObj.AddComponent<Classes.Button>().relatedText = "PreviousPage";
            allButtons.Add(prevBtnObj);

            CreateMenuText("<", new Vector3(0f, 0.06f, -0.123f), new Vector2(4f, 3f), 3f);

            GameObject nextBtnObj = new GameObject("NextButton");
            nextBtnObj.transform.parent = menu.transform;
            nextBtnObj.transform.rotation = Quaternion.identity;
            nextBtnObj.transform.localPosition = new Vector3(0.56f, -0.2f, -0.32f);

            float nWidth = 0.055f;
            float nHeight = 0.04f;
            float nThick = 0.005f;
            float nRad = 0.012f;

            ThemeManager.Theme currentT = ThemeManager.themes[ThemeManager.currentThemeIndex];

            void AddNextPart(PrimitiveType nType, Vector3 nPos, Vector3 nScale)
            {
                GameObject nPart = GameObject.CreatePrimitive(nType);
                UnityEngine.Object.Destroy(nPart.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(nPart.GetComponent<Collider>());
                nPart.transform.parent = nextBtnObj.transform;
                nPart.transform.localPosition = nPos;
                nPart.transform.localScale = nScale;

                if (nType == PrimitiveType.Cylinder)
                    nPart.transform.localRotation = Quaternion.Euler(0, 0, 90);

                ApplyMenuMaterial(nPart.GetComponent<Renderer>(), currentT.Button);
            }

            float nX = (nWidth / 2) - nRad;
            float nZ = (nHeight / 2) - nRad;
            Vector3 nCylScale = new Vector3(nRad * 2, nThick / 2.01f, nRad * 2);

            AddNextPart(PrimitiveType.Cylinder, new Vector3(0, nX, nZ), nCylScale);
            AddNextPart(PrimitiveType.Cylinder, new Vector3(0, -nX, nZ), nCylScale);
            AddNextPart(PrimitiveType.Cylinder, new Vector3(0, nX, -nZ), nCylScale);
            AddNextPart(PrimitiveType.Cylinder, new Vector3(0, -nX, -nZ), nCylScale);

            AddNextPart(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(nThick, nWidth - (nRad * 2), nHeight));
            AddNextPart(PrimitiveType.Cube, new Vector3(0, nX, 0), new Vector3(nThick, nRad * 2, nHeight - (nRad * 2)));
            AddNextPart(PrimitiveType.Cube, new Vector3(0, -nX, 0), new Vector3(nThick, nRad * 2, nHeight - (nRad * 2)));

            if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive) nextBtnObj.layer = 2;

            BoxCollider nTrigger = nextBtnObj.AddComponent<BoxCollider>();
            nTrigger.isTrigger = true;
            nTrigger.size = new Vector3(nThick * 2f, nWidth, nHeight);
            nTrigger.center = Vector3.zero;

            nextBtnObj.AddComponent<Classes.Button>().relatedText = "NextPage";
            allButtons.Add(nextBtnObj);

            CreateMenuText(">", new Vector3(0f, -0.06f, -0.123f), new Vector2(4f, 3f), 3f);

            GameObject homeBtn = new GameObject("HomeButton");
            homeBtn.transform.parent = menu.transform;
            homeBtn.transform.rotation = Quaternion.identity;
            homeBtn.transform.localPosition = new Vector3(0.56f, 0f, -0.32f);

            float totalWidth = 0.06f;
            float totalHeight = 0.04f;
            float thickness = 0.005f;
            float radius = 0.012f;

            ThemeManager.Theme t = ThemeManager.themes[ThemeManager.currentThemeIndex];

            void AddHomePart(PrimitiveType type, Vector3 pos, Vector3 scale)
            {
                GameObject part = GameObject.CreatePrimitive(type);
                UnityEngine.Object.Destroy(part.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(part.GetComponent<Collider>());
                part.transform.parent = homeBtn.transform;
                part.transform.localPosition = pos;
                part.transform.localScale = scale;

                if (type == PrimitiveType.Cylinder)
                    part.transform.localRotation = Quaternion.Euler(0, 0, 90);

                ApplyMenuMaterial(part.GetComponent<Renderer>(), t.Button);
            }

            float xOff = (totalWidth / 2) - radius;
            float zOff = (totalHeight / 2) - radius;
            Vector3 cylScale = new Vector3(radius * 2, thickness / 2.01f, radius * 2);

            AddHomePart(PrimitiveType.Cylinder, new Vector3(0, xOff, zOff), cylScale);
            AddHomePart(PrimitiveType.Cylinder, new Vector3(0, -xOff, zOff), cylScale);
            AddHomePart(PrimitiveType.Cylinder, new Vector3(0, xOff, -zOff), cylScale);
            AddHomePart(PrimitiveType.Cylinder, new Vector3(0, -xOff, -zOff), cylScale);

            AddHomePart(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(thickness, totalWidth - (radius * 2), totalHeight));
            AddHomePart(PrimitiveType.Cube, new Vector3(0, xOff, 0), new Vector3(thickness, radius * 2, totalHeight - (radius * 2)));
            AddHomePart(PrimitiveType.Cube, new Vector3(0, -xOff, 0), new Vector3(thickness, radius * 2, totalHeight - (radius * 2)));

            if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive) homeBtn.layer = 2;
            BoxCollider trigger = homeBtn.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(thickness * 2f, totalWidth, totalHeight);
            trigger.center = Vector3.zero;

            homeBtn.AddComponent<Classes.Button>().relatedText = "HomeButton";
            allButtons.Add(homeBtn);

            CreateMenuText("Home", new Vector3(0f, 0f, -0.122f), new Vector2(8f, 3f), 2f);

            CreateVisibleButtons(motdFont);
        }

        private static void CreateVisibleButtons(TMP_FontAsset motdFont)
        {
            int start = pageNumber * buttonsPerPage;
            int shown = 0;

            if (searchActive && searchQuery.Length > 0)
            {
                int matched = 0;
                HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (ButtonInfo[] list in buttons)
                {
                    foreach (ButtonInfo button in list)
                    {
                        if (!ButtonMatchesSearch(button, searchQuery)) continue;
                        if (!seen.Add(button.buttonText)) continue;
                        if (matched++ < start) continue;

                        CreateButton(shown * 0.1f, button, motdFont);
                        if (++shown >= buttonsPerPage) return;
                    }
                }

                return;
            }

            ButtonInfo[] categoryButtons = buttons[currentCategory];
            for (int i = start; i < categoryButtons.Length && shown < buttonsPerPage; i++)
            {
                CreateButton(shown * 0.1f, categoryButtons[i], motdFont);
                shown++;
            }
        }

        private static int CountSearchResults()
        {
            int count = 0;
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ButtonInfo[] list in buttons)
            {
                foreach (ButtonInfo button in list)
                {
                    if (!ButtonMatchesSearch(button, searchQuery)) continue;
                    if (!seen.Add(button.buttonText)) continue;
                    count++;
                }
            }

            return count;
        }

        private static readonly string[] KeyboardRows = new string[]
        {
            "POIUYTREWQ",
            "LKJHGFDSA",
            "CLR<MNBVCXZ"
        };

        public static void CreateSearchKeyboard(TMP_FontAsset motdFont)
        {
            if (!searchActive) return;
            {
                GameObject qTxtObj = new GameObject("SearchQueryDisplay");
                qTxtObj.transform.SetParent(canvasObject.transform, false);
                qTxtObj.layer = 0;
                TextMeshPro qTmp = qTxtObj.AddComponent<TextMeshPro>();
                if (motdFont != null) qTmp.font = motdFont;
                qTmp.text = searchQuery.Length > 0 ? searchQuery : "...";
                qTmp.fontSize = 3f;
                qTmp.alignment = TextAlignmentOptions.Center;
                qTmp.color = ThemeManager.themes[ThemeManager.currentThemeIndex].Text;
                qTmp.overflowMode = TextOverflowModes.Overflow;
                RectTransform qRect = qTmp.GetComponent<RectTransform>();
                qRect.sizeDelta = new Vector2(14f, 3f);
                qRect.localPosition = new Vector3(0.06f, 0f, -0.2f);
                qRect.localRotation = Quaternion.Euler(0f, -90f, -90f);
                qRect.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            }

            float keySize = 0.05f;
            float keyGap = 0.12f;
            float keyThick = 0.005f;
            float keyRad = 0.008f;
            float startZ = -0.72f;
            float stepZ = keySize + keyGap;

            ThemeManager.Theme kTheme = ThemeManager.themes[ThemeManager.currentThemeIndex];

            string[][] KeyboardTokens = new string[][]
            {
                new string[] { "P","O","I","U","Y","T","R","E","W","Q" },
                new string[] { "L","K","J","H","G","F","D","S","A" },
                new string[] { "CLR","<","M","N","B","V","C","X","Z" }
            };

            for (int row = 0; row < KeyboardTokens.Length; row++)
            {
                string[] tokens = KeyboardTokens[row];
                float rowWidth = tokens.Length * (keySize + keyGap) - keyGap;
                float qwertyIndent = row * ((keySize + keyGap) * 0.5f);
                float startY = -(rowWidth / 2f) + (keySize / 2f) + qwertyIndent;
                float rowZ = startZ - row * stepZ;

                for (int col = 0; col < tokens.Length; col++)
                {
                    string keyLabel = tokens[col];
                    string keyId = "Key_" + keyLabel;

                    GameObject keyObj = new GameObject(keyId);
                    keyObj.transform.parent = menu.transform;
                    keyObj.transform.rotation = Quaternion.identity;
                    keyObj.transform.localPosition = new Vector3(0.56f, startY + col * (keySize + keyGap), rowZ);

                    void AddKeyPart(PrimitiveType kType, Vector3 kPos, Vector3 kScale)
                    {
                        GameObject kPart = GameObject.CreatePrimitive(kType);
                        UnityEngine.Object.Destroy(kPart.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(kPart.GetComponent<Collider>());
                        kPart.transform.parent = keyObj.transform;
                        kPart.transform.localPosition = kPos;
                        kPart.transform.localScale = kScale;
                        kPart.transform.localRotation = (kType == PrimitiveType.Cylinder) ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;

                        ApplyMenuMaterial(kPart.GetComponent<Renderer>(), kTheme.Button);
                    }

                    float kX = (keySize / 2) - keyRad;
                    float kZ = (keySize / 2) - keyRad;
                    Vector3 kCylScale = new Vector3(keyRad * 2, keyThick / 2.01f, keyRad * 2);

                    AddKeyPart(PrimitiveType.Cylinder, new Vector3(0, kX, kZ), kCylScale);
                    AddKeyPart(PrimitiveType.Cylinder, new Vector3(0, -kX, kZ), kCylScale);
                    AddKeyPart(PrimitiveType.Cylinder, new Vector3(0, kX, -kZ), kCylScale);
                    AddKeyPart(PrimitiveType.Cylinder, new Vector3(0, -kX, -kZ), kCylScale);

                    AddKeyPart(PrimitiveType.Cube, Vector3.zero, new Vector3(keyThick, keySize - (keyRad * 2), keySize));
                    AddKeyPart(PrimitiveType.Cube, new Vector3(0, kX, 0), new Vector3(keyThick, keyRad * 2, keySize - (keyRad * 2)));
                    AddKeyPart(PrimitiveType.Cube, new Vector3(0, -kX, 0), new Vector3(keyThick, keyRad * 2, keySize - (keyRad * 2)));

                    if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive) keyObj.layer = 2;

                    BoxCollider kTrigger = keyObj.AddComponent<BoxCollider>();
                    kTrigger.isTrigger = true;
                    kTrigger.size = new Vector3(keyThick * 4f, keySize, keySize);
                    kTrigger.center = Vector3.zero;

                    keyObj.AddComponent<Classes.Button>().relatedText = keyId;
                    allButtons.Add(keyObj);

                    GameObject kTxtObj = new GameObject("KeyText");
                    kTxtObj.transform.SetParent(keyObj.transform, false);
                    kTxtObj.layer = 0;
                    TextMeshPro kTmp = kTxtObj.AddComponent<TextMeshPro>();
                    if (motdFont != null) kTmp.font = motdFont;
                    kTmp.text = keyLabel;
                    kTmp.fontSize = 3f;
                    kTmp.alignment = TextAlignmentOptions.Center;
                    kTmp.color = kTheme.Text;

                    RectTransform kRect = kTmp.GetComponent<RectTransform>();
                    kRect.sizeDelta = new Vector2(keySize * 20f, keySize * 20f);
                    kRect.localPosition = new Vector3(0.006f, 0f, 0f);
                    kRect.localRotation = Quaternion.Euler(180f, 90f, 90f);
                    kRect.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                }
            }

            float spaceWidth = 0.3f;
            float spaceHeight = keySize;
            float spaceThick = keyThick;
            float spaceRad = keyRad;
            float spaceZ = startZ - KeyboardTokens.Length * stepZ;

            GameObject spaceObj = new GameObject("Key_ ");
            spaceObj.transform.parent = menu.transform;
            spaceObj.transform.rotation = Quaternion.identity;
            spaceObj.transform.localPosition = new Vector3(0.56f, 0f, spaceZ);

            void AddSpacePart(PrimitiveType sType, Vector3 sPos, Vector3 sScale)
            {
                GameObject sPart = GameObject.CreatePrimitive(sType);
                UnityEngine.Object.Destroy(sPart.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(sPart.GetComponent<Collider>());
                sPart.transform.parent = spaceObj.transform;
                sPart.transform.localPosition = sPos;
                sPart.transform.localScale = sScale;
                sPart.transform.localRotation = (sType == PrimitiveType.Cylinder) ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
                ApplyMenuMaterial(sPart.GetComponent<Renderer>(), kTheme.Button);
            }

            float spX = (spaceWidth / 2) - spaceRad;
            float spZ2 = (spaceHeight / 2) - spaceRad;
            Vector3 spCylScale = new Vector3(spaceRad * 2, spaceThick / 2.01f, spaceRad * 2);

            AddSpacePart(PrimitiveType.Cylinder, new Vector3(0, spX, spZ2), spCylScale);
            AddSpacePart(PrimitiveType.Cylinder, new Vector3(0, -spX, spZ2), spCylScale);
            AddSpacePart(PrimitiveType.Cylinder, new Vector3(0, spX, -spZ2), spCylScale);
            AddSpacePart(PrimitiveType.Cylinder, new Vector3(0, -spX, -spZ2), spCylScale);

            AddSpacePart(PrimitiveType.Cube, Vector3.zero, new Vector3(spaceThick, spaceWidth - (spaceRad * 2), spaceHeight));
            AddSpacePart(PrimitiveType.Cube, new Vector3(0, spX, 0), new Vector3(spaceThick, spaceRad * 2, spaceHeight - (spaceRad * 2)));
            AddSpacePart(PrimitiveType.Cube, new Vector3(0, -spX, 0), new Vector3(spaceThick, spaceRad * 2, spaceHeight - (spaceRad * 2)));

            if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive) spaceObj.layer = 2;

            BoxCollider spTrigger = spaceObj.AddComponent<BoxCollider>();
            spTrigger.isTrigger = true;
            spTrigger.size = new Vector3(spaceThick * 4f, spaceWidth, spaceHeight);
            spTrigger.center = Vector3.zero;

            spaceObj.AddComponent<Classes.Button>().relatedText = "Key_ ";
            allButtons.Add(spaceObj);

            GameObject spTxtObj = new GameObject("KeyText");
            spTxtObj.transform.SetParent(spaceObj.transform, false);
            spTxtObj.layer = 0;
            TextMeshPro spTmp = spTxtObj.AddComponent<TextMeshPro>();
            if (motdFont != null) spTmp.font = motdFont;
            spTmp.text = "SPACE";
            spTmp.fontSize = 3f;
            spTmp.alignment = TextAlignmentOptions.Center;
            spTmp.color = kTheme.Text;

            RectTransform spRect = spTmp.GetComponent<RectTransform>();
            spRect.sizeDelta = new Vector2(spaceWidth * 20f, spaceHeight * 20f);
            spRect.localPosition = new Vector3(0.006f, 0f, 0f);
            spRect.localRotation = Quaternion.Euler(180f, 90f, 90f);
            spRect.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }

        public static void CreateButton(float offset, ButtonInfo method, TMP_FontAsset motdFont)
        {
            GameObject buttonContainer = new GameObject("Buttons");
            buttonContainer.transform.parent = menu.transform;
            buttonContainer.transform.rotation = Quaternion.identity;
            buttonContainer.transform.localPosition = new Vector3(0.56f, 0f, 0.2f - offset);

            bool isValueChanger = IsValueChanger(method.buttonText);

            float totalWidth = isValueChanger ? 0.1f : 0.175f;
            float totalHeight = isValueChanger ? 0.035f : 0.035f;
            float thickness = 0.005f;
            float radius = 0.01f;

            Color themeButtonColor = ThemeManager.themes[ThemeManager.currentThemeIndex].Button;
            Color themeTextColor = ThemeManager.themes[ThemeManager.currentThemeIndex].Text;

            void AddPart(PrimitiveType type, Vector3 pos, Vector3 scale, Quaternion rot)
            {
                GameObject part = GameObject.CreatePrimitive(type);
                UnityEngine.Object.Destroy(part.GetComponent<Rigidbody>());
                part.transform.parent = buttonContainer.transform;
                part.transform.localPosition = pos;
                part.transform.localScale = scale;
                part.transform.localRotation = rot;

                ApplyMenuMaterial(part.GetComponent<Renderer>(), themeButtonColor);

                UnityEngine.Object.Destroy(part.GetComponent<Collider>());
            }

            Vector3 cylScale = new Vector3(radius * 2, thickness / 2.01f, radius * 2);
            Quaternion cylRot = Quaternion.Euler(0, 0, 90);
            float xOff = (totalWidth / 2) - radius;
            float zOff = (totalHeight / 2) - radius;

            AddPart(PrimitiveType.Cylinder, new Vector3(0, xOff, zOff), cylScale, cylRot);
            AddPart(PrimitiveType.Cylinder, new Vector3(0, -xOff, zOff), cylScale, cylRot);
            AddPart(PrimitiveType.Cylinder, new Vector3(0, xOff, -zOff), cylScale, cylRot);
            AddPart(PrimitiveType.Cylinder, new Vector3(0, -xOff, -zOff), cylScale, cylRot);

            AddPart(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(thickness, totalWidth - (radius * 2), totalHeight), Quaternion.identity);
            AddPart(PrimitiveType.Cube, new Vector3(0, xOff, 0), new Vector3(thickness, radius * 2, totalHeight - (radius * 2)), Quaternion.identity);
            AddPart(PrimitiveType.Cube, new Vector3(0, -xOff, 0), new Vector3(thickness, radius * 2, totalHeight - (radius * 2)), Quaternion.identity);

            if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive)
            {
                buttonContainer.layer = 2;
            }

            if (!isValueChanger)
            {
                BoxCollider trigger = buttonContainer.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(thickness * 4f, totalWidth, totalHeight);
                trigger.center = Vector3.zero;

                buttonContainer.AddComponent<Classes.Button>().relatedText = method.buttonText;
                allButtons.Add(buttonContainer);
            }

            GameObject txtObj = new GameObject("ButtonText");
            txtObj.transform.SetParent(buttonContainer.transform, false);
            txtObj.layer = 0;
            TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
            if (motdFont != null) tmp.font = motdFont;

            tmp.text = method.overlapText ?? method.buttonText;
            tmp.fontSize = 3f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = method.enabled ? Color.green : themeTextColor;

            RectTransform rect = tmp.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(totalWidth * 20f, totalHeight * 20f);

            rect.localPosition = new Vector3(0.006f, 0f, 0f);
            rect.localRotation = Quaternion.Euler(180f, 90f, 90f);
            rect.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            if (isValueChanger)
            {
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 1.15f;
                tmp.fontSizeMax = 1.9f;
                tmp.lineSpacing = -8f;
                rect.sizeDelta = new Vector2(totalWidth * 13.5f, totalHeight * 24f);

                void CreateValueSideButton(bool increment, float yOffset)
                {
                    string controlId = GetValueChangerControlId(method.buttonText, increment);
                    if (controlId == null) return;

                    GameObject sideButton = new GameObject(increment ? "ValuePlusButton" : "ValueMinusButton");
                    sideButton.transform.parent = menu.transform;
                    sideButton.transform.rotation = Quaternion.identity;
                    sideButton.transform.localPosition = buttonContainer.transform.localPosition + new Vector3(0f, yOffset, 0f);

                    float sideWidth = 0.035f;
                    float sideHeight = 0.035f;
                    float sideThickness = 0.005f;
                    float sideRadius = 0.01f;

                    void AddSidePart(PrimitiveType type, Vector3 pos, Vector3 scale, Quaternion rot)
                    {
                        GameObject part = GameObject.CreatePrimitive(type);
                        UnityEngine.Object.Destroy(part.GetComponent<Rigidbody>());
                        part.transform.parent = sideButton.transform;
                        part.transform.localPosition = pos;
                        part.transform.localScale = scale;
                        part.transform.localRotation = rot;
                        ApplyMenuMaterial(part.GetComponent<Renderer>(), themeButtonColor);
                        UnityEngine.Object.Destroy(part.GetComponent<Collider>());
                    }

                    Vector3 sideCylScale = new Vector3(sideRadius * 2, sideThickness / 2.01f, sideRadius * 2);
                    float sideXOff = (sideWidth / 2) - sideRadius;
                    float sideZOff = (sideHeight / 2) - sideRadius;
                    Quaternion sideCylRot = Quaternion.Euler(0, 0, 90);

                    AddSidePart(PrimitiveType.Cylinder, new Vector3(0, sideXOff, sideZOff), sideCylScale, sideCylRot);
                    AddSidePart(PrimitiveType.Cylinder, new Vector3(0, -sideXOff, sideZOff), sideCylScale, sideCylRot);
                    AddSidePart(PrimitiveType.Cylinder, new Vector3(0, sideXOff, -sideZOff), sideCylScale, sideCylRot);
                    AddSidePart(PrimitiveType.Cylinder, new Vector3(0, -sideXOff, -sideZOff), sideCylScale, sideCylRot);

                    AddSidePart(PrimitiveType.Cube, Vector3.zero, new Vector3(sideThickness, sideWidth - (sideRadius * 2), sideHeight), Quaternion.identity);
                    AddSidePart(PrimitiveType.Cube, new Vector3(0, sideXOff, 0), new Vector3(sideThickness, sideRadius * 2, sideHeight - (sideRadius * 2)), Quaternion.identity);
                    AddSidePart(PrimitiveType.Cube, new Vector3(0, -sideXOff, 0), new Vector3(sideThickness, sideRadius * 2, sideHeight - (sideRadius * 2)), Quaternion.identity);

                    if (!UnityInput.Current.GetKey(keyboardButton) && !searchActive)
                        sideButton.layer = 2;

                    BoxCollider sideTrigger = sideButton.AddComponent<BoxCollider>();
                    sideTrigger.isTrigger = true;
                    sideTrigger.size = new Vector3(sideThickness * 4f, sideWidth, sideHeight);
                    sideTrigger.center = Vector3.zero;

                    sideButton.AddComponent<Classes.Button>().relatedText = controlId;
                    allButtons.Add(sideButton);

                    GameObject sideTextObj = new GameObject("ValueSideText");
                    sideTextObj.transform.SetParent(sideButton.transform, false);
                    sideTextObj.layer = 0;
                    TextMeshPro sideTmp = sideTextObj.AddComponent<TextMeshPro>();
                    if (motdFont != null) sideTmp.font = motdFont;
                    sideTmp.text = increment ? "+" : "-";
                    sideTmp.fontSize = 3.5f;
                    sideTmp.alignment = TextAlignmentOptions.Center;
                    sideTmp.color = themeTextColor;

                    RectTransform sideRect = sideTmp.GetComponent<RectTransform>();
                    sideRect.sizeDelta = new Vector2(sideWidth * 20f, sideHeight * 20f);
                    sideRect.localPosition = new Vector3(0.006f, 0f, 0f);
                    sideRect.localRotation = Quaternion.Euler(180f, 90f, 90f);
                    sideRect.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                }

                CreateValueSideButton(true, -0.23f);
                CreateValueSideButton(false, 0.23f);
            }
        }

        public static void RecreateMenu()
        {
            if (menu != null)
            {
                Vector3 savedPos = menu.transform.position;
                Quaternion savedRot = menu.transform.rotation;
                Transform savedParent = menu.transform.parent;

                Destroy(menu);
                menu = null;
                CreateMenu();

                bool kbOpen = UnityInput.Current.GetKey(keyboardButton) || (searchActive && pcSearchMode);

                if (menuPinned)
                {
                    menu.transform.parent = savedParent;
                    menu.transform.position = savedPos;
                    menu.transform.rotation = savedRot;
                }
                else if (kbOpen)
                {
                    RecenterMenu(rightHanded, true);
                }
                else if (searchActive)
                {
                    PinMenuInFrontOfPlayer();
                }
                else
                {
                    RecenterMenu(rightHanded, false);
                }
            }
        }

        public static void RecenterMenu(bool isRightHanded, bool isKeyboardCondition)
        {
            if (!isKeyboardCondition)
            {
                CleanupKeyboardBackgroundCamera();

                if (!isRightHanded)
                {
                    menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                }
                else
                {
                    menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                    rotation += new Vector3(0f, 0f, 180f);
                    menu.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                if (TPC == null)
                {
                    try { TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera")?.GetComponent<Camera>(); } catch { }
                }

                if (_cachedVcam1 == null)
                    _cachedVcam1 = GameObject.Find("Shoulder Camera")?.transform.Find("CM vcam1")?.gameObject;
                _cachedVcam1?.SetActive(false);

                if (TPC != null)
                {
                    TPC.transform.position = new Vector3(-999f, -999f, -999f);
                    TPC.transform.rotation = Quaternion.identity;

                    EnsureKeyboardBackgroundCamera(TPC.depth - 1);

                    menu.transform.parent = TPC.transform;
                    menu.transform.position = TPC.transform.position + (TPC.transform.forward * 0.5f) + (TPC.transform.up * -0.02f);
                    menu.transform.rotation = TPC.transform.rotation * Quaternion.Euler(-90f, 90f, 0f);

                    if (reference != null && Mouse.current.leftButton.isPressed)
                    {
                        Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                        if (Physics.Raycast(ray, out RaycastHit hit, 100))
                        {
                            hit.transform.gameObject.GetComponent<Classes.Button>()?.OnTriggerEnter(buttonCollider);
                        }
                    }
                }
            }
        }

        public static void PinMenuInFrontOfPlayer()
        {
            if (menu == null) return;

            menu.transform.parent = null;

            Transform head = GorillaTagger.Instance.headCollider.transform;
            Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (flatForward == Vector3.zero) flatForward = head.forward;

            menu.transform.position = head.position + flatForward * 0.60f + Vector3.up * 0.08f;
            menu.transform.rotation = Quaternion.LookRotation(-flatForward, Vector3.up) * Quaternion.Euler(-90f, -90f, 0f);
        }

        public static void Toggle(string buttonText)
        {
            if (buttonText == "HomeButton")
            {
                currentCategory = 0;
                pageNumber = 0;
                RecreateMenu();
                return;
            }

            if (buttonText == "Disconnect")
            {
                PhotonNetwork.Disconnect();
                return;
            }

            if (buttonText == "SettingsButton")
            {
                currentCategory = 1;
                pageNumber = 0;
                RecreateMenu();
                return;
            }

            if (buttonText == "SearchButton")
            {
                searchActive = !searchActive;
                if (searchActive)
                    pcSearchMode = UnityInput.Current.GetKey(keyboardButton);
                else
                {
                    searchQuery = "";
                    pcSearchMode = false;
                }
                RecreateMenu();
                if (menuPinned) PinMenuInFrontOfPlayer();
                return;
            }

            if (buttonText.StartsWith("Key_") && searchActive)
            {
                string keyChar = buttonText.Substring(4);

                if (keyChar == "<")
                {
                    if (searchQuery.Length > 0)
                        searchQuery = searchQuery.Substring(0, searchQuery.Length - 1);
                }
                else if (keyChar == "CLR")
                {
                    searchQuery = "";
                }
                else
                {
                    searchQuery += keyChar;
                }

                pageNumber = 0;
                RecreateMenu();
                if (menuPinned) PinMenuInFrontOfPlayer();
                return;
            }

            if (TryHandleValueChangerControl(buttonText))
            {
                RecreateMenu();
                if (menuPinned) PinMenuInFrontOfPlayer();
                return;
            }

            int resultCount = (searchActive && searchQuery.Length > 0) ? CountSearchResults() : buttons[currentCategory].Length;
            int lastPage = Math.Max(0, ((resultCount + buttonsPerPage - 1) / buttonsPerPage) - 1);
            if (buttonText == "PreviousPage")
            {
                pageNumber = (pageNumber <= 0) ? lastPage : pageNumber - 1;
                RecreateMenu();
                if (menuPinned) PinMenuInFrontOfPlayer();
                return;
            }
            else if (buttonText == "NextPage")
            {
                pageNumber = (pageNumber >= lastPage) ? 0 : pageNumber + 1;
                RecreateMenu();
                if (menuPinned) PinMenuInFrontOfPlayer();
                return;
            }

            ButtonInfo target = GetIndex(buttonText);
            if (target != null)
            {
                if (Get.leftGrab && Get.rightGrab)
                {
                    FavoriteMods.ToggleFavorite(target.buttonText);

                    bool isFav = FavoriteMods.favoritedNames.Contains(target.buttonText);
                    NotifiLib.SendNotification(isFav ? $"<color=yellow>Added {target.buttonText} to Favorites!</color>" : $"<color=orange>Removed {target.buttonText} from Favorites</color>");
                }
                else
                {
                    if (target.isTogglable)
                    {
                        target.enabled = !target.enabled;

                        string status = target.enabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>";
                        NotifiLib.SendNotification($"<color=grey>[</color>{status}<color=grey>]</color> {target.buttonText}");

                        if (target.enabled) target.enableMethod?.Invoke();
                        else target.disableMethod?.Invoke();
                        EnabledMods.RebuildRuntimeList();
                    }
                    else
                    {
                        NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLED</color><color=grey>]</color> " + target.buttonText);
                        target.method?.Invoke();
                    }
                }

                if (currentCategory == 2)
                {
                    EnabledMods.RefreshEnabledTab();
                }

                if (currentCategory == 3)
                {
                    FavoriteMods.RefreshFavoriteTab();
                }
            }
            else
            {
                Debug.LogError(buttonText + " does not exist in the button list.");
            }

            RecreateMenu();
            if (menuPinned) PinMenuInFrontOfPlayer();
        }

        private static readonly Dictionary<string, (int Category, int Index)> cacheGetIndex = new Dictionary<string, (int Category, int Index)>();
        public static ButtonInfo GetIndex(string buttonText)
        {
            if (buttonText == null)
                return null;

            if (cacheGetIndex.ContainsKey(buttonText))
            {
                var CacheData = cacheGetIndex[buttonText];
                try
                {
                    if (buttons[CacheData.Category][CacheData.Index].buttonText == buttonText)
                        return buttons[CacheData.Category][CacheData.Index];
                }
                catch { cacheGetIndex.Remove(buttonText); }
            }

            int categoryIndex = 0;
            foreach (ButtonInfo[] buttons in buttons)
            {
                int buttonIndex = 0;
                foreach (ButtonInfo button in buttons)
                {
                    if (button.buttonText == buttonText)
                    {
                        try
                        {
                            cacheGetIndex.Add(buttonText, (categoryIndex, buttonIndex));
                        }
                        catch
                        {
                            if (cacheGetIndex.ContainsKey(buttonText))
                                cacheGetIndex.Remove(buttonText);
                        }

                        return button;
                    }
                    buttonIndex++;
                }
                categoryIndex++;
            }

            return null;
        }

        public static Vector3 RandomVector3(float range = 1f) =>
            new Vector3(UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range));

        public static Quaternion RandomQuaternion(float range = 360f) =>
            Quaternion.Euler(UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range));

        public static Color RandomColor(byte range = 255, byte alpha = 255) =>
            new Color32((byte)UnityEngine.Random.Range(0, range),
                        (byte)UnityEngine.Random.Range(0, range),
                        (byte)UnityEngine.Random.Range(0, range),
                        alpha);

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
        {
            Quaternion rot = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handRotOffset;
            return (GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handRotOffset;
            return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static void WorldScale(GameObject obj, Vector3 targetWorldScale)
        {
            Vector3 parentScale = obj.transform.parent.lossyScale;
            obj.transform.localScale = new Vector3(
                targetWorldScale.x / parentScale.x,
                targetWorldScale.y / parentScale.y,
                targetWorldScale.z / parentScale.z
            );
        }

        public static void FixStickyColliders(GameObject platform)
        {
            Vector3[] localPositions = new Vector3[]
            {
                new Vector3(0, 1f, 0),
                new Vector3(0, -1f, 0),
                new Vector3(1f, 0, 0),
                new Vector3(-1f, 0, 0),
                new Vector3(0, 0, 1f),
                new Vector3(0, 0, -1f)
            };
            Quaternion[] localRotations = new Quaternion[]
            {
                Quaternion.Euler(90, 0, 0),
                Quaternion.Euler(-90, 0, 0),
                Quaternion.Euler(0, -90, 0),
                Quaternion.Euler(0, 90, 0),
                Quaternion.identity,
                Quaternion.Euler(0, 180, 0)
            };
            for (int i = 0; i < localPositions.Length; i++)
            {
                GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cube);
                try
                {
                    if (platform.GetComponent<GorillaSurfaceOverride>() != null)
                    {
                        side.AddComponent<GorillaSurfaceOverride>().overrideIndex = platform.GetComponent<GorillaSurfaceOverride>().overrideIndex;
                    }
                }
                catch { }
                float size = 0.025f;
                side.transform.SetParent(platform.transform);
                side.transform.position = localPositions[i] * (size / 2);
                side.transform.rotation = localRotations[i];
                WorldScale(side, new Vector3(size, size, 0.01f));
                side.GetComponent<Renderer>().enabled = false;
            }
        }

        private static int? noInvisLayerMask;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(
                1 << LayerMask.NameToLayer("TransparentFX") |
                1 << LayerMask.NameToLayer("Ignore Raycast") |
                1 << LayerMask.NameToLayer("Zone") |
                1 << LayerMask.NameToLayer("Gorilla Trigger") |
                1 << LayerMask.NameToLayer("Gorilla Boundary") |
                1 << LayerMask.NameToLayer("GorillaCosmetics") |
                1 << LayerMask.NameToLayer("GorillaParticle"));

            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }

        public static GameObject menu;
        public static GameObject menuBackground;
        public static GameObject reference;
        public static GameObject reference2;
        public static GameObject canvasObject;

        public static SphereCollider buttonCollider;
        public static SphereCollider buttonCollider2;
        public static Camera TPC;
        public static Text fpsObject;
        public static TextMeshPro fpsText;

        public static int pageNumber = 0;
        public static int _currentCategory;
        public static int currentCategory
        {
            get => _currentCategory;
            set
            {
                _currentCategory = value;
                pageNumber = 0;
            }
        }
    }
}
