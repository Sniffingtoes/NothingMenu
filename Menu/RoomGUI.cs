using GorillaNetworking;

using Nothing.Notifications;

using Photon.Pun;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Nothing.Menu
{
    public class RoomGUI : MonoBehaviour
    {
        private bool _showRoomMenu = true;
        private float _wWidth = 322;
        private float _wHeight = 232;
        private Rect _windowRect;
        private string _roomCode = "";

        private GUIStyle _titleStyle, _captionStyle, _inputStyle, _btnStyle, _ghostBtnStyle, _statusStyle;
        private Texture2D _bgTex, _panelTex, _inputBgTex, _accentTex, _softAccentTex, _primaryTex, _dangerTex, _lineTex, _shadowTex;
        private Color _shadowColor;
        private bool _stylesInitialized = false;
        private int _styleThemeIndex = -1;

        void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.backslashKey.wasPressedThisFrame ||
                    Keyboard.current.backquoteKey.wasPressedThisFrame ||
                    Keyboard.current.insertKey.wasPressedThisFrame)
                {
                    _showRoomMenu = !_showRoomMenu;
                }
            }
        }

        void InitializeStyles()
        {
            if (_stylesInitialized && _styleThemeIndex == ThemeManager.currentThemeIndex) return;

            ThemeManager.Theme theme = ThemeManager.GetColors();
            Color accent = Mix(theme.Button, theme.Text, 0.35f);
            Color text = theme.Text;
            Color mutedText = Mix(theme.Text, theme.Background, 0.38f);
            _shadowColor = WithAlpha(Color.black, 0.68f);

            _bgTex = MakeTex(WithAlpha(Darken(theme.Background, 0.68f), 0.93f));
            _panelTex = MakeTex(WithAlpha(Mix(theme.Background, theme.Button, 0.32f), 0.88f));
            _inputBgTex = MakeTex(WithAlpha(Mix(theme.Button, theme.Background, 0.12f), 0.95f));
            _accentTex = MakeTex(accent);
            _softAccentTex = MakeTex(WithAlpha(accent, 0.14f));
            _primaryTex = MakeTex(WithAlpha(accent, 0.78f));
            _dangerTex = MakeTex(new Color(0.92f, 0.32f, 0.38f, 0.18f));
            _lineTex = MakeTex(WithAlpha(theme.Text, 0.09f));
            _shadowTex = MakeTex(WithAlpha(Color.black, 0.24f));

            _titleStyle = new GUIStyle { fontSize = 19, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = text } };
            _captionStyle = new GUIStyle { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = mutedText } };
            _inputStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 23, fontStyle = FontStyle.Bold, normal = { background = _inputBgTex, textColor = text }, focused = { background = _inputBgTex, textColor = text }, padding = new RectOffset(10, 10, 6, 6) };
            _btnStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = text }, hover = { textColor = text }, active = { textColor = text } };
            _ghostBtnStyle = new GUIStyle(_btnStyle) { normal = { textColor = new Color(1f, 0.72f, 0.76f, 1f) } };
            _statusStyle = new GUIStyle { fontSize = 11, alignment = TextAnchor.MiddleCenter, normal = { textColor = mutedText } };
            _stylesInitialized = true;
            _styleThemeIndex = ThemeManager.currentThemeIndex;
        }

        void OnGUI()
        {
            if (!_showRoomMenu) return;

            InitializeStyles();
            _windowRect = new Rect(Screen.width - _wWidth - 22, 22, _wWidth, _wHeight);
            GUI.depth = -100;
            GUI.DrawTexture(new Rect(_windowRect.x + 7f, _windowRect.y + 9f, _windowRect.width, _windowRect.height), _shadowTex);
            GUI.Window(101, _windowRect, DrawRoomWindow, "", GUIStyle.none);
        }

        void DrawRoomWindow(int id)
        {
            GUI.depth = -100;
            GUI.DrawTexture(new Rect(0, 0, _windowRect.width, _windowRect.height), _bgTex);
            GUI.DrawTexture(new Rect(0, 0, 5, _windowRect.height), _accentTex);
            GUI.DrawTexture(new Rect(18, 18, _windowRect.width - 36, 58), _panelTex);
            GUI.DrawTexture(new Rect(18, 74, _windowRect.width - 36, 2), _softAccentTex);
            GUI.DrawTexture(new Rect(18, 84, _windowRect.width - 36, 1), _lineTex);

            DrawShadowLabel(new Rect(32, 24, 190, 24), "ROOM JOINER", _titleStyle);
            GUI.Label(new Rect(33, 52, 190, 16), "PRIVATE ROOM TOOLS", _captionStyle);

            Rect inputRect = new Rect(32, 98, _windowRect.width - 64, 48);
            GUI.DrawTexture(new Rect(inputRect.x, inputRect.yMax - 2f, inputRect.width, 2f), _softAccentTex);
            _roomCode = GUI.TextField(inputRect, _roomCode.ToUpperInvariant(), _inputStyle);

            Rect joinRect = new Rect(32, 160, _windowRect.width - 64, 36);
            GUI.DrawTexture(joinRect, _primaryTex);
            if (GUI.Button(joinRect, "JOIN ROOM", _btnStyle))
            {
                if (!string.IsNullOrEmpty(_roomCode))
                {
                    PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(_roomCode, JoinType.Solo);
                    NotifiLib.SendNotification("Joining: " + _roomCode);
                }
            }

            Rect leaveRect = new Rect(32, 204, 112, 22);
            GUI.DrawTexture(leaveRect, _dangerTex);
            if (GUI.Button(leaveRect, "LEAVE", _ghostBtnStyle)) PhotonNetwork.LeaveRoom();

            GUI.Label(new Rect(154, 204, _windowRect.width - 186, 22), _roomCode.Length == 0 ? "ENTER CODE" : _roomCode.Length + " CHARS", _statusStyle);
        }

        void DrawShadowLabel(Rect rect, string text, GUIStyle style)
        {
            Color original = style.normal.textColor;
            style.normal.textColor = _shadowColor;
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
            style.normal.textColor = original;
            GUI.Label(rect, text, style);
        }

        Texture2D MakeTex(Color col)
        {
            Texture2D pix = new Texture2D(1, 1);
            pix.SetPixel(0, 0, col);
            pix.Apply();
            return pix;
        }

        static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
        static Color Darken(Color color, float amount) => Mix(color, Color.black, amount);
        static Color Mix(Color a, Color b, float t) => Color.Lerp(a, b, Mathf.Clamp01(t));
    }
}
