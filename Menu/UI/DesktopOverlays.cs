using GorillaNetworking;
using Nothing.Notifications;
using Photon.Pun;
using UnityEngine;
using static Nothing.Menu.UI.UiStyle;

namespace Nothing.Menu
{
    public class Thingy : MonoBehaviour
    {
        private GUIStyle _brandStyle, _fpsStyle, _labelStyle;
        private Texture2D _barTex, _panelTex, _accentTex, _softAccentTex, _shadowTex, _goodTex, _warnTex, _badTex;
        private Color _shadowColor;
        private bool _stylesInitialized;
        private int _styleThemeIndex = -1;
        private float _deltaTime;
        private int _fpsDisplay;
        private float _fpsUpdateTimer = 0.05f;

        void Update()
        {
            if (!Settings.pcWatermark) return;

            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _fpsUpdateTimer -= Time.unscaledDeltaTime;
            if (_fpsUpdateTimer <= 0)
            {
                _fpsDisplay = Mathf.CeilToInt(1f / _deltaTime);
                _fpsUpdateTimer = 0.05f;
            }
        }

        void OnGUI()
        {
            if (!Settings.pcWatermark) return;

            InitializeStyles();

            const float width = 408f;
            const float height = 56f;
            Rect barRect = new Rect((Screen.width - width) / 2f, 18f, width, height);

            GUI.DrawTexture(new Rect(barRect.x + 7f, barRect.y + 9f, barRect.width, barRect.height), _shadowTex);
            GUI.DrawTexture(barRect, _barTex);
            GUI.DrawTexture(new Rect(barRect.x, barRect.y, 5f, barRect.height), _accentTex);
            GUI.DrawTexture(new Rect(barRect.x + 18f, barRect.yMax - 3f, barRect.width - 36f, 2f), _softAccentTex);
            GUI.DrawTexture(new Rect(barRect.x + 18f, barRect.y + 14f, 8f, 8f), _goodTex);

            DrawShadowLabel(new Rect(barRect.x + 38f, barRect.y + 8f, 210f, 20f), "NOTHING MENU", _brandStyle);

            Rect fpsPanel = new Rect(barRect.xMax - 124f, barRect.y + 10f, 104f, 34f);
            GUI.DrawTexture(fpsPanel, _panelTex);
            GUI.DrawTexture(new Rect(fpsPanel.x, fpsPanel.yMax - 3f, fpsPanel.width, 3f), GetFpsTexture());
            GUI.Label(fpsPanel, _fpsDisplay + " FPS", _fpsStyle);
        }

        void InitializeStyles()
        {
            if (_stylesInitialized && _styleThemeIndex == ThemeManager.currentThemeIndex) return;

            ReleaseStyleTextures();

            ThemeManager.Theme theme = ThemeManager.GetColors();
            Color accent = Mix(theme.Button, theme.Text, 0.58f);
            Color text = theme.Text;
            Color mutedText = Mix(theme.Text, theme.Background, 0.38f);
            _shadowColor = WithAlpha(Color.black, 0.72f);

            _barTex = MakeTex(WithAlpha(Darken(theme.Background, 0.82f), 0.95f));
            _panelTex = MakeTex(WithAlpha(Mix(theme.Background, theme.Button, 0.32f), 0.98f));
            _accentTex = MakeTex(accent);
            _softAccentTex = MakeTex(WithAlpha(accent, 0.15f));
            _shadowTex = MakeTex(WithAlpha(Color.black, 0.4f));
            _goodTex = MakeTex(new Color(0.32f, 0.78f, 0.48f, 1f));
            _warnTex = MakeTex(new Color(0.92f, 0.68f, 0.25f, 1f));
            _badTex = MakeTex(new Color(0.92f, 0.32f, 0.38f, 1f));

            _brandStyle = new GUIStyle { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = text } };
            _fpsStyle = new GUIStyle { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = text } };
            _labelStyle = new GUIStyle { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = mutedText } };
            _stylesInitialized = true;
            _styleThemeIndex = ThemeManager.currentThemeIndex;
        }

        void DrawShadowLabel(Rect rect, string text, GUIStyle style)
        {
            Color original = style.normal.textColor;
            style.normal.textColor = _shadowColor;
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
            style.normal.textColor = original;
            GUI.Label(rect, text, style);
        }

        Texture2D GetFpsTexture()
        {
            if (_fpsDisplay >= 90) return _goodTex;
            if (_fpsDisplay >= 55) return _warnTex;
            return _badTex;
        }

        void OnDestroy() => ReleaseStyleTextures();

        void ReleaseStyleTextures()
        {
            Release(ref _barTex);
            Release(ref _panelTex);
            Release(ref _accentTex);
            Release(ref _softAccentTex);
            Release(ref _shadowTex);
            Release(ref _goodTex);
            Release(ref _warnTex);
            Release(ref _badTex);
        }
    }

    public class RoomGUI : MonoBehaviour
    {
        private const float WindowWidth = 380f;
        private const float WindowHeight = 274f;
        private Rect _windowRect;
        private string _roomCode = "";
        private GUIStyle _titleStyle, _captionStyle, _inputStyle, _buttonStyle, _leaveButtonStyle, _statusStyle;
        private Texture2D _backgroundTexture, _panelTexture, _inputTexture, _accentTexture, _softAccentTexture, _primaryTexture, _dangerTexture, _lineTexture, _shadowTexture;
        private Color _shadowColor;
        private bool _stylesInitialized;
        private int _styleThemeIndex = -1;

        void OnGUI()
        {
            if (!Settings.pcRoomJoiner) return;

            InitializeStyles();
            _windowRect = new Rect(Screen.width - WindowWidth - 22f, 22f, WindowWidth, WindowHeight);
            GUI.depth = -100;
            GUI.DrawTexture(new Rect(_windowRect.x + 7f, _windowRect.y + 9f, _windowRect.width, _windowRect.height), _shadowTexture);
            GUI.Window(101, _windowRect, DrawRoomWindow, "", GUIStyle.none);
        }

        void DrawRoomWindow(int id)
        {
            GUI.depth = -100;
            GUI.DrawTexture(new Rect(0f, 0f, _windowRect.width, _windowRect.height), _backgroundTexture);
            GUI.DrawTexture(new Rect(0f, 0f, 5f, _windowRect.height), _accentTexture);
            GUI.DrawTexture(new Rect(18f, 18f, _windowRect.width - 36f, 68f), _panelTexture);
            GUI.DrawTexture(new Rect(18f, 84f, _windowRect.width - 36f, 2f), _softAccentTexture);
            GUI.DrawTexture(new Rect(18f, 96f, _windowRect.width - 36f, 1f), _lineTexture);
            GUI.DrawTexture(new Rect(32f, 34f, 10f, 10f), _primaryTexture);

            DrawShadowLabel(new Rect(54f, 25f, 240f, 26f), "ROOM JOINER", _titleStyle);
            GUI.Label(new Rect(32f, 108f, 180f, 14f), "ROOM CODE", _captionStyle);

            Rect inputRect = new Rect(32f, 124f, _windowRect.width - 64f, 50f);
            GUI.DrawTexture(new Rect(inputRect.x, inputRect.yMax - 3f, inputRect.width, 3f), _softAccentTexture);
            string roomCodeInput = GUI.TextField(inputRect, _roomCode, _inputStyle);
            if (roomCodeInput != _roomCode) _roomCode = roomCodeInput.ToUpperInvariant();

            GUI.Label(new Rect(32f, 176f, _windowRect.width - 64f, 12f), _roomCode.Length == 0 ? "ENTER A CODE OR JOIN RANDOM" : _roomCode.Length + " CHARACTERS READY", _statusStyle);

            Rect joinRect = new Rect(32f, 190f, _windowRect.width - 64f, 36f);
            GUI.DrawTexture(joinRect, _primaryTexture);
            if (GUI.Button(joinRect, "JOIN ROOM", _buttonStyle) && !string.IsNullOrEmpty(_roomCode))
            {
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(_roomCode, JoinType.Solo);
                NotifiLib.SendNotification("Joining: " + _roomCode);
            }

            Rect randomRect = new Rect(32f, 240f, 150f, 22f);
            GUI.DrawTexture(randomRect, _panelTexture);
            if (GUI.Button(randomRect, "JOIN RANDOM", _buttonStyle))
            {
                PhotonNetwork.JoinRandomRoom();
                NotifiLib.SendNotification("Joining a random room");
            }

            Rect disconnectRect = new Rect(198f, 240f, _windowRect.width - 230f, 22f);
            GUI.DrawTexture(disconnectRect, _dangerTexture);
            if (GUI.Button(disconnectRect, "DISCONNECT", _leaveButtonStyle)) PhotonNetwork.Disconnect();
        }

        void InitializeStyles()
        {
            if (_stylesInitialized && _styleThemeIndex == ThemeManager.currentThemeIndex) return;

            ReleaseStyleTextures();

            ThemeManager.Theme theme = ThemeManager.GetColors();
            Color accent = Mix(theme.Button, theme.Text, 0.58f);
            Color text = theme.Text;
            Color mutedText = Mix(theme.Text, theme.Background, 0.38f);
            _shadowColor = WithAlpha(Color.black, 0.68f);

            _backgroundTexture = MakeTex(WithAlpha(Darken(theme.Background, 0.8f), 0.98f));
            _panelTexture = MakeTex(WithAlpha(Mix(theme.Background, theme.Button, 0.24f), 0.97f));
            _inputTexture = MakeTex(WithAlpha(Mix(theme.Button, theme.Background, 0.08f), 0.99f));
            _accentTexture = MakeTex(accent);
            _softAccentTexture = MakeTex(WithAlpha(accent, 0.22f));
            _primaryTexture = MakeTex(WithAlpha(accent, 0.9f));
            _dangerTexture = MakeTex(new Color(0.92f, 0.32f, 0.38f, 0.24f));
            _lineTexture = MakeTex(WithAlpha(theme.Text, 0.14f));
            _shadowTexture = MakeTex(WithAlpha(Color.black, 0.42f));

            _titleStyle = new GUIStyle { fontSize = 21, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = text } };
            _captionStyle = new GUIStyle { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = mutedText } };
            _inputStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 23, fontStyle = FontStyle.Bold, normal = { background = _inputTexture, textColor = text }, focused = { background = _inputTexture, textColor = text }, padding = new RectOffset(10, 10, 6, 6) };
            _buttonStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = text }, hover = { textColor = text }, active = { textColor = text } };
            _leaveButtonStyle = new GUIStyle(_buttonStyle) { normal = { textColor = new Color(1f, 0.72f, 0.76f, 1f) } };
            _statusStyle = new GUIStyle { fontSize = 11, alignment = TextAnchor.MiddleCenter, normal = { textColor = mutedText } };
            _stylesInitialized = true;
            _styleThemeIndex = ThemeManager.currentThemeIndex;
        }

        void DrawShadowLabel(Rect rect, string text, GUIStyle style)
        {
            Color original = style.normal.textColor;
            style.normal.textColor = _shadowColor;
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
            style.normal.textColor = original;
            GUI.Label(rect, text, style);
        }

        void OnDestroy() => ReleaseStyleTextures();

        void ReleaseStyleTextures()
        {
            Release(ref _backgroundTexture);
            Release(ref _panelTexture);
            Release(ref _inputTexture);
            Release(ref _accentTexture);
            Release(ref _softAccentTexture);
            Release(ref _primaryTexture);
            Release(ref _dangerTexture);
            Release(ref _lineTexture);
            Release(ref _shadowTexture);
        }
    }
}
