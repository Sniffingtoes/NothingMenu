using UnityEngine;
using UnityEngine.InputSystem;

using Nothing.Classes;
using NothingMenu.Utils;
using static Nothing.Menu.UI.UiStyle;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nothing.Menu
{
    public class PCUI : MonoBehaviour
    {
        private bool _showMenu = true;
        private const int ConsoleCategory = -1;
        private const float ConsoleClickResetSeconds = 1.25f;
        private Rect _windowRect = new Rect(90, 70, 1040, 680);
        private Vector2 _scrollPos = Vector2.zero;
        private Vector2 _consoleScrollPos = Vector2.zero;
        private string _searchText = "";
        private string _consoleInput = "";
        private string _lastBuiltSearchText = null;
        private int _pcCategory = 0;
        private int _lastBuiltCategory = -1;
        private int _titleClickCount = 0;
        private float _lastTitleClickTime = -10f;
        private GUIStyle _titleStyle, _subtleStyle, _navStyle, _activeNavStyle, _modButtonStyle, _searchStyle, _emptyStyle, _pillStyle, _consoleStyle, _consoleInputStyle, _consoleButtonStyle, _arrayTitleStyle, _arrayRowStyle, _scrollbarStyle, _scrollThumbStyle, _headerBadgeStyle;
        private Texture2D _windowTex, _sidebarTex, _panelTex, _surfaceTex, _activeTex, _accentTex, _softAccentTex, _hoverTex, _lineTex, _searchTex, _enabledTex, _disabledTex, _scrollTrackTex, _scrollThumbTex, _scrollThumbHoverTex, _windowShadowTex, _logoTex;
        private Color _shadowColor;
        private bool _stylesInitialized = false;
        private int _styleThemeIndex = -1;
        private readonly List<ButtonInfo> _displayList = new List<ButtonInfo>(64);
        private readonly List<string> _consoleLines = new List<string>();
        private readonly GUIContent _consoleContent = new GUIContent();
        private static readonly string PluginTitle = PluginInfo.Name.ToUpperInvariant();

        private static readonly int[] NavTabs = new int[] { 0, 1, 4, 5, 7, 8, 9, 10 };

        void Start() => SoundboardHandler.RefreshSoundboardButtons();

        void Update()
        {
            if (Keyboard.current != null && (Keyboard.current.backslashKey.wasPressedThisFrame || Keyboard.current.backquoteKey.wasPressedThisFrame || Keyboard.current.insertKey.wasPressedThisFrame))
                ToggleMenu();
        }

        void ToggleMenu()
        {
            _showMenu = !_showMenu;
            Cursor.visible = true;
            Cursor.lockState = _showMenu ? CursorLockMode.None : CursorLockMode.Confined;
        }

        void InitializeStyles()
        {
            if (_stylesInitialized && _styleThemeIndex == ThemeManager.currentThemeIndex) return;

            ReleaseStyleTextures();

            ThemeManager.Theme theme = ThemeManager.GetColors();
            Color background = WithAlpha(Darken(theme.Background, 0.82f), 0.985f);
            Color sidebar = WithAlpha(Mix(theme.Background, Color.black, 0.35f), 0.98f);
            Color panel = WithAlpha(Mix(theme.Background, theme.Button, 0.2f), 0.96f);
            Color surface = WithAlpha(Mix(theme.Button, theme.Background, 0.08f), 0.99f);
            Color accent = WithAlpha(Mix(theme.Button, theme.Text, 0.58f), 1f);
            Color accentDim = WithAlpha(accent, 0.26f);
            Color text = theme.Text;
            Color mutedText = WithAlpha(Mix(theme.Text, theme.Background, 0.48f), 1f);
            _shadowColor = WithAlpha(Color.black, 0.74f);

            _windowTex = MakeTex(background);
            _sidebarTex = MakeTex(sidebar);
            _panelTex = MakeTex(panel);
            _surfaceTex = MakeTex(surface);
            _activeTex = MakeTex(accentDim);
            _accentTex = MakeTex(accent);
            _softAccentTex = MakeTex(WithAlpha(accent, 0.22f));
            _hoverTex = MakeTex(WithAlpha(theme.Text, 0.13f));
            _lineTex = MakeTex(WithAlpha(theme.Text, 0.14f));
            _searchTex = MakeTex(WithAlpha(Mix(theme.Button, theme.Background, 0.16f), 0.98f));
            _enabledTex = MakeTex(new Color(0.32f, 0.78f, 0.48f, 0.9f));
            _disabledTex = MakeTex(WithAlpha(Mix(theme.Button, theme.Background, 0.5f), 0.82f));
            _scrollTrackTex = MakeTex(WithAlpha(theme.Text, 0.045f));
            _scrollThumbTex = MakeTex(WithAlpha(accent, 0.72f));
            _scrollThumbHoverTex = MakeTex(WithAlpha(Mix(accent, theme.Text, 0.25f), 0.92f));
            _windowShadowTex = MakeTex(WithAlpha(Color.black, 0.38f));

            _titleStyle = new GUIStyle { fontSize = 27, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = text } };
            _subtleStyle = new GUIStyle { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = mutedText } };
            _navStyle = new GUIStyle { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = mutedText }, hover = { textColor = text, background = _hoverTex }, active = { textColor = text, background = _softAccentTex }, padding = new RectOffset(20, 10, 0, 0) };
            _activeNavStyle = new GUIStyle(_navStyle) { normal = { textColor = text, background = _activeTex } };
            _modButtonStyle = new GUIStyle { alignment = TextAnchor.MiddleLeft, fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = text }, hover = { textColor = text, background = _hoverTex }, active = { textColor = text, background = _softAccentTex }, padding = new RectOffset(20, 78, 0, 0) };
            _searchStyle = new GUIStyle { fontSize = 14, alignment = TextAnchor.MiddleLeft, normal = { background = _searchTex, textColor = text }, focused = { background = _searchTex, textColor = text }, padding = new RectOffset(15, 14, 7, 7) };
            _emptyStyle = new GUIStyle { fontSize = 15, alignment = TextAnchor.MiddleCenter, normal = { textColor = mutedText } };
            _pillStyle = new GUIStyle { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = text } };
            _headerBadgeStyle = new GUIStyle { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = text } };
            _consoleStyle = new GUIStyle { fontSize = 13, alignment = TextAnchor.UpperLeft, wordWrap = true, normal = { textColor = text }, padding = new RectOffset(12, 12, 10, 10) };
            _consoleInputStyle = new GUIStyle(_searchStyle) { fontSize = 13 };
            _consoleButtonStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 12, fontStyle = FontStyle.Bold, normal = { textColor = text }, hover = { textColor = text } };
            _arrayTitleStyle = new GUIStyle { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = mutedText } };
            _arrayRowStyle = new GUIStyle { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = text } };
            _scrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar) { fixedWidth = 10, margin = new RectOffset(6, 0, 0, 0), padding = new RectOffset(2, 2, 2, 2) };
            _scrollbarStyle.normal.background = _scrollTrackTex;
            _scrollbarStyle.hover.background = _scrollTrackTex;
            _scrollbarStyle.active.background = _scrollTrackTex;
            _scrollThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb) { fixedWidth = 6 };
            _scrollThumbStyle.normal.background = _scrollThumbTex;
            _scrollThumbStyle.hover.background = _scrollThumbHoverTex;
            _scrollThumbStyle.active.background = _scrollThumbHoverTex;

            _stylesInitialized = true;
            _styleThemeIndex = ThemeManager.currentThemeIndex;
            LoadLogoTexture();
        }

        void LoadLogoTexture()
        {
            if (_logoTex != null) return;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NothingMenu.Resources.logo.png"))
            {
                if (stream == null) return;

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                _logoTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                _logoTex.LoadImage(bytes);
                _logoTex.filterMode = FilterMode.Bilinear;
            }
        }

        void OnGUI()
        {
            InitializeStyles();
            GUI.depth = 100;
            DrawPcArrayList();

            if (!_showMenu) return;

            _windowRect.width = Mathf.Min(_windowRect.width, Screen.width - 32f);
            _windowRect.height = Mathf.Min(_windowRect.height, Screen.height - 32f);
            _windowRect.x = Mathf.Clamp(_windowRect.x, 16f, Mathf.Max(16f, Screen.width - _windowRect.width - 16f));
            _windowRect.y = Mathf.Clamp(_windowRect.y, 16f, Mathf.Max(16f, Screen.height - _windowRect.height - 16f));

            GUI.DrawTexture(new Rect(_windowRect.x + 8f, _windowRect.y + 10f, _windowRect.width, _windowRect.height), _windowShadowTex);
            _windowRect = GUI.Window(99, _windowRect, DrawWindow, "", GUIStyle.none);
        }

        void DrawWindow(int id)
        {
            GUI.depth = 100;
            DrawRect(new Rect(0, 0, _windowRect.width, _windowRect.height), _windowTex);
            DrawRect(new Rect(0, 0, _windowRect.width, 94), _panelTex);
            DrawRect(new Rect(0, 94, 214, _windowRect.height - 94), _sidebarTex);
            DrawRect(new Rect(0, 93, _windowRect.width, 1), _lineTex);
            DrawRect(new Rect(0, 0, 6, _windowRect.height), _accentTex);
            DrawRect(new Rect(18, 92, _windowRect.width - 36, 2), _softAccentTex);

            DrawHeader();
            DrawNavigation();
            DrawContent();

            GUI.DragWindow(new Rect(310, 0, _windowRect.width - 670, 94));
        }

        void DrawHeader()
        {
            if (_logoTex != null)
            {
                DrawRect(new Rect(24, 18, 58, 58), _softAccentTex);
                DrawRect(new Rect(24, 18, 4, 58), _accentTex);
                GUI.DrawTexture(new Rect(32, 26, 42, 42), _logoTex, ScaleMode.ScaleToFit, true);
            }

            Rect titleRect = new Rect(_logoTex != null ? 96 : 28, 22, 300, 30);
            DrawShadowLabel(titleRect, PluginTitle, _titleStyle);
            if (GUI.Button(titleRect, GUIContent.none, GUIStyle.none))
                HandleTitleClick();

            GUI.SetNextControlName("SearchField");
            string lastSearch = _searchText;
            _searchText = GUI.TextField(new Rect(_windowRect.width - 370, 28, 330, 36), _searchText, _searchStyle);
            if (lastSearch != _searchText)
            {
                _scrollPos = Vector2.zero;
                _lastBuiltSearchText = null;
            }
        }

        void DrawNavigation()
        {
            Rect navRect = new Rect(14, 118, 186, _windowRect.height - 142);
            DrawRect(new Rect(navRect.x, navRect.y - 24, navRect.width, 16), _sidebarTex);
            GUI.Label(new Rect(navRect.x + 12, navRect.y - 24, navRect.width - 20, 16), "NAVIGATION", _subtleStyle);
            DrawRect(new Rect(navRect.xMax + 14, navRect.y - 24, 1, navRect.height + 24), _lineTex);

            GUILayout.BeginArea(new Rect(navRect.x + 10, navRect.y + 12, navRect.width - 20, navRect.height - 24));
            foreach (int i in NavTabs)
            {
                DrawNavRow(GetManualTabName(i), i);
                GUILayout.Space(6);
            }
            GUILayout.EndArea();
        }

        void DrawPcArrayList()
        {
            if (!Nothing.Settings.pcArrayList) return;

            float x = 14f;
            float y = 12f;
            float width = 260f;
            DrawShadowLabel(new Rect(x, y, width, 18f), "Enabled Mods", _arrayTitleStyle);
            y += 22f;
            int row = 0;
            foreach (ButtonInfo btn in EnabledMods.RuntimeEnabledButtons)
            {
                if (!btn.enabled || ContainsIgnoreCase(btn.buttonText, "return")) continue;

                Rect rowRect = new Rect(x, y + row * 20f, width, 20f);
                if (rowRect.yMax > Screen.height - 12f) break;

                DrawShadowLabel(rowRect, btn.buttonText, _arrayRowStyle);
                row++;
            }
        }

        void DrawShadowLabel(Rect rect, string text, GUIStyle style)
        {
            Color original = style.normal.textColor;
            style.normal.textColor = _shadowColor;
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
            style.normal.textColor = original;
            GUI.Label(rect, text, style);
        }

        void DrawContent()
        {
            Rect contentRect = new Rect(236, 118, _windowRect.width - 260, _windowRect.height - 142);
            DrawRect(contentRect, _panelTex);
            DrawRect(new Rect(contentRect.x, contentRect.y, contentRect.width, 2), _softAccentTex);

            if (_pcCategory == ConsoleCategory && string.IsNullOrEmpty(_searchText))
            {
                DrawConsole(contentRect);
                return;
            }

            RebuildDisplayListIfNeeded();
            DrawShadowLabel(new Rect(contentRect.x + 18, contentRect.y + 13, 300, 24), string.IsNullOrEmpty(_searchText) ? GetManualTabName(_pcCategory) : "Search Results", _titleStyle);
            GUI.Label(new Rect(contentRect.x + 20, contentRect.y + 44, 300, 18), _displayList.Count + " AVAILABLE MODULES", _subtleStyle);

            Rect listRect = new Rect(contentRect.x + 16, contentRect.y + 76, contentRect.width - 32, contentRect.height - 92);
            GUILayout.BeginArea(listRect);
            GUIStyle previousThumb = GUI.skin.verticalScrollbarThumb;
            GUI.skin.verticalScrollbarThumb = _scrollThumbStyle;
            float cardGap = 14f;
            float cardHeight = 68f;
            const int columnCount = 2;
            float cardWidth = (listRect.width - 28f - (cardGap * (columnCount - 1))) / columnCount;
            int rows = Mathf.CeilToInt(_displayList.Count / (float)columnCount);
            Rect viewRect = new Rect(0, 0, listRect.width - 18f, Mathf.Max(listRect.height, rows * (cardHeight + cardGap)));

            _scrollPos = GUI.BeginScrollView(new Rect(0, 0, listRect.width, listRect.height), _scrollPos, viewRect, false, true, GUIStyle.none, _scrollbarStyle);

            if (_displayList.Count == 0)
                GUI.Label(new Rect(0, 120, viewRect.width, 40), "No matching mods", _emptyStyle);

            for (int i = 0; i < _displayList.Count; i++)
            {
                int col = i % columnCount;
                int row = i / columnCount;
                Rect cardRect = new Rect(col * (cardWidth + cardGap), row * (cardHeight + cardGap), cardWidth, cardHeight);
                DrawMod(_displayList[i], cardRect);
            }

            GUI.EndScrollView();
            GUI.skin.verticalScrollbarThumb = previousThumb;
            GUILayout.EndArea();
        }

        void DrawNavRow(string label, int category)
        {
            bool isActive = _pcCategory == category && string.IsNullOrEmpty(_searchText);
            Rect row = GUILayoutUtility.GetRect(150, 38);
            if (isActive)
            {
                DrawRect(new Rect(row.x, row.y, row.width, row.height), _softAccentTex);
                DrawRect(new Rect(row.x, row.y, 4, row.height), _accentTex);
            }

            if (GUI.Button(row, label, isActive ? _activeNavStyle : _navStyle))
            {
                _pcCategory = category;
                _searchText = "";
                _scrollPos = Vector2.zero;
                GUI.FocusControl(null);
                if (category == 8) SoundboardHandler.RefreshSoundboardButtons();
            }
        }

        void DrawConsole(Rect contentRect)
        {
            ConsoleCommands.AddWelcomeLines(_consoleLines);

            DrawShadowLabel(new Rect(contentRect.x + 18, contentRect.y + 13, 300, 24), "Console", _titleStyle);
            GUI.Label(new Rect(contentRect.x + 20, contentRect.y + 44, 420, 18), "COMMANDS AND QUICK TOOLS", _subtleStyle);

            Rect copyRect = new Rect(contentRect.x + contentRect.width - 176, contentRect.y + 20, 144, 30);
            DrawRect(copyRect, _accentTex);
            if (GUI.Button(copyRect, "COPY PLAYFAB ID", _consoleButtonStyle))
                RunCopyPlayFabId();

            Rect consoleRect = new Rect(contentRect.x + 16, contentRect.y + 76, contentRect.width - 32, contentRect.height - 132);
            DrawRect(consoleRect, _surfaceTex);

            string consoleText = string.Join("\n", _consoleLines);
            _consoleContent.text = consoleText;
            float textHeight = Mathf.Max(consoleRect.height - 20f, _consoleStyle.CalcHeight(_consoleContent, consoleRect.width - 38f));
            GUIStyle previousThumb = GUI.skin.verticalScrollbarThumb;
            GUI.skin.verticalScrollbarThumb = _scrollThumbStyle;
            _consoleScrollPos = GUI.BeginScrollView(consoleRect, _consoleScrollPos, new Rect(0, 0, consoleRect.width - 18f, textHeight), false, true, GUIStyle.none, _scrollbarStyle);
            GUI.Label(new Rect(0, 0, consoleRect.width - 34f, textHeight), consoleText, _consoleStyle);
            GUI.EndScrollView();
            GUI.skin.verticalScrollbarThumb = previousThumb;

            Rect inputRect = new Rect(contentRect.x + 16, contentRect.yMax - 42, contentRect.width - 132, 30);
            Rect runRect = new Rect(contentRect.xMax - 104, contentRect.yMax - 42, 72, 30);

            Event evt = Event.current;
            bool enterPressed = evt.type == EventType.KeyDown
                && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                && GUI.GetNameOfFocusedControl() == "ConsoleInput";
            if (enterPressed) evt.Use();

            GUI.SetNextControlName("ConsoleInput");
            _consoleInput = GUI.TextField(inputRect, _consoleInput, _consoleInputStyle);
            DrawRect(runRect, _accentTex);
            if (GUI.Button(runRect, "RUN", _consoleButtonStyle) || enterPressed)
                RunConsoleCommand();
        }

        void HandleTitleClick()
        {
            if (Time.unscaledTime - _lastTitleClickTime > ConsoleClickResetSeconds)
                _titleClickCount = 0;

            _lastTitleClickTime = Time.unscaledTime;
            _titleClickCount++;

            if (_titleClickCount < 3) return;

            _titleClickCount = 0;
            _pcCategory = ConsoleCategory;
            _searchText = "";
            _scrollPos = Vector2.zero;
            _consoleScrollPos.y = float.MaxValue;
            GUI.FocusControl(null);
        }

        void RunConsoleCommand()
        {
            string command = _consoleInput.Trim();
            if (command.Length == 0) return;

            _consoleInput = "";
            ConsoleCommands.Execute(command, _consoleLines);
            _consoleScrollPos.y = float.MaxValue;
        }

        void RunCopyPlayFabId()
        {
            ConsoleCommands.CopyPlayFabId(_consoleLines);
            _consoleScrollPos.y = float.MaxValue;
        }

        void DrawMod(ButtonInfo btn, Rect r)
        {
            if (ContainsIgnoreCase(btn.buttonText, "return")) return;

            bool active = btn.isTogglable && btn.enabled;
            DrawRect(r, active ? _activeTex : _surfaceTex);
            DrawRect(new Rect(r.x, r.y, r.width, 2f), active ? _accentTex : _lineTex);
            DrawRect(new Rect(r.x, r.y, 4f, r.height), active ? _enabledTex : _disabledTex);
            DrawRect(new Rect(r.x + 16f, r.yMax - 2f, r.width - 32f, 1f), active ? _accentTex : _lineTex);

            Rect actionRect = btn.isTogglable ? new Rect(r.x, r.y, r.width - 86f, r.height) : r;
            if (GUI.Button(actionRect, btn.buttonText, _modButtonStyle))
            {
                if (btn.isTogglable) TogglePcMod(btn);
                else if (btn.method != null) { btn.method.Invoke(); }
                else
                {
                    foreach (int i in NavTabs)
                        if (btn.buttonText == GetManualTabName(i))
                        {
                            _pcCategory = i;
                            _searchText = "";
                            _scrollPos = Vector2.zero;
                            if (i == 8) SoundboardHandler.RefreshSoundboardButtons();
                            break;
                        }
                }
            }

            if (btn.isTogglable)
            {
                Rect pill = new Rect(r.xMax - 76, r.y + 22, 56, 24);
                DrawRect(pill, active ? _enabledTex : _disabledTex);
                GUI.Label(pill, active ? "ON" : "OFF", _pillStyle);
                if (GUI.Button(pill, "", GUIStyle.none)) TogglePcMod(btn);
            }
        }

        void TogglePcMod(ButtonInfo btn)
        {
            if (EnabledMods.IsDisabledServerCheck(btn.buttonText))
            {
                btn.enabled = false;
                return;
            }

            btn.enabled = !btn.enabled;
            if (btn.enabled) btn.enableMethod?.Invoke();
            else btn.disableMethod?.Invoke();
            EnabledMods.RebuildRuntimeList();
            EnabledMods.RefreshEnabledTab();
        }

        void DrawRect(Rect rect, Texture2D texture) => GUI.DrawTexture(rect, texture);

        void OnDestroy()
        {
            ReleaseStyleTextures();
            Release(ref _logoTex);
        }

        void ReleaseStyleTextures()
        {
            Release(ref _windowTex);
            Release(ref _sidebarTex);
            Release(ref _panelTex);
            Release(ref _surfaceTex);
            Release(ref _activeTex);
            Release(ref _accentTex);
            Release(ref _softAccentTex);
            Release(ref _hoverTex);
            Release(ref _lineTex);
            Release(ref _searchTex);
            Release(ref _enabledTex);
            Release(ref _disabledTex);
            Release(ref _scrollTrackTex);
            Release(ref _scrollThumbTex);
            Release(ref _scrollThumbHoverTex);
            Release(ref _windowShadowTex);
        }

        string CurrentHeaderText()
        {
            if (!string.IsNullOrEmpty(_searchText)) return "filtered by search";
            if (_pcCategory == ConsoleCategory) return "console";
            return _pcCategory == 0 ? "quick access" : GetManualTabName(_pcCategory).ToLowerInvariant();
        }

        string GetManualTabName(int index)
        {
            switch (index)
            {
                case 0: return "Home";
                case 1: return "Settings";
                case 4: return "Movement";
                case 5: return "Visuals";
                case 6: return "Fun";
                case 7: return "Usefull";
                case 8: return "Soundboard";
                case 9: return "Player";
                case 10: return "Beta";
                default: return "Cat " + index;
            }
        }

        void RebuildDisplayListIfNeeded()
        {
            if (_lastBuiltCategory == _pcCategory && _lastBuiltSearchText == _searchText) return;

            _displayList.Clear();
            if (!string.IsNullOrEmpty(_searchText))
            {
                HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (int i in NavTabs)
                {
                    if (Buttons.buttons[i] == null) continue;
                    foreach (var btn in Buttons.buttons[i])
                        if (ContainsIgnoreCase(btn.buttonText, _searchText) && !ContainsIgnoreCase(btn.buttonText, "return") && seen.Add(btn.buttonText))
                            _displayList.Add(btn);
                }
            }
            else if (_pcCategory == 0)
            {
                foreach (int i in NavTabs)
                {
                    if (i == 0) continue;
                    _displayList.Add(new ButtonInfo { buttonText = GetManualTabName(i), isTogglable = false });
                }
            }
            else if (Buttons.buttons[_pcCategory] != null)
            {
                foreach (var btn in Buttons.buttons[_pcCategory])
                    _displayList.Add(btn);
            }

            _lastBuiltCategory = _pcCategory;
            _lastBuiltSearchText = _searchText;
        }

        static bool ContainsIgnoreCase(string value, string query) =>
            value != null && query != null && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
