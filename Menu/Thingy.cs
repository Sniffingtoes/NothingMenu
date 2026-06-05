using UnityEngine;
using UnityEngine.InputSystem;

namespace Nothing.Menu
{
    public class Thingy : MonoBehaviour
    {
        private GUIStyle _brandStyle, _fpsStyle, _labelStyle;
        private Texture2D _barTex, _panelTex, _accentTex, _softAccentTex, _shadowTex, _goodTex, _warnTex, _badTex;
        private Color _shadowColor;
        private bool _stylesInitialized = false;
        private int _styleThemeIndex = -1;
        private float _deltaTime = 0.0f;
        private int _fpsDisplay = 0;
        private float _fpsUpdateTimer = 0.05f;
        private bool _showFPS = true;

        void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.backslashKey.wasPressedThisFrame ||
                    Keyboard.current.backquoteKey.wasPressedThisFrame ||
                    Keyboard.current.insertKey.wasPressedThisFrame)
                {
                    _showFPS = !_showFPS;
                }
            }

            if (!_showFPS) return;

            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _fpsUpdateTimer -= Time.unscaledDeltaTime;
            if (_fpsUpdateTimer <= 0)
            {
                _fpsDisplay = Mathf.CeilToInt(1.0f / _deltaTime);
                _fpsUpdateTimer = 0.05f;
            }
        }

        void InitializeStyles()
        {
            if (_stylesInitialized && _styleThemeIndex == ThemeManager.currentThemeIndex) return;

            ThemeManager.Theme theme = ThemeManager.GetColors();
            Color accent = Mix(theme.Button, theme.Text, 0.35f);
            Color text = theme.Text;
            Color mutedText = Mix(theme.Text, theme.Background, 0.38f);
            _shadowColor = WithAlpha(Color.black, 0.72f);

            _barTex = MakeTex(WithAlpha(Darken(theme.Background, 0.68f), 0.84f));
            _panelTex = MakeTex(WithAlpha(Mix(theme.Background, theme.Button, 0.44f), 0.88f));
            _accentTex = MakeTex(accent);
            _softAccentTex = MakeTex(WithAlpha(accent, 0.15f));
            _shadowTex = MakeTex(WithAlpha(Color.black, 0.24f));
            _goodTex = MakeTex(new Color(0.32f, 0.78f, 0.48f, 1f));
            _warnTex = MakeTex(new Color(0.92f, 0.68f, 0.25f, 1f));
            _badTex = MakeTex(new Color(0.92f, 0.32f, 0.38f, 1f));

            _brandStyle = new GUIStyle { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = text } };
            _fpsStyle = new GUIStyle { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = text } };
            _labelStyle = new GUIStyle { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = mutedText } };
            _stylesInitialized = true;
            _styleThemeIndex = ThemeManager.currentThemeIndex;
        }

        void OnGUI()
        {
            if (!_showFPS) return;

            InitializeStyles();

            float width = 318f;
            float height = 40f;
            float xPos = (Screen.width - width) / 2f;
            Rect barRect = new Rect(xPos, 14f, width, height);

            GUI.DrawTexture(new Rect(barRect.x + 5f, barRect.y + 6f, barRect.width, barRect.height), _shadowTex);
            GUI.DrawTexture(barRect, _barTex);
            GUI.DrawTexture(new Rect(barRect.x, barRect.y, 4f, barRect.height), _accentTex);
            GUI.DrawTexture(new Rect(barRect.x + 16f, barRect.yMax - 2f, barRect.width - 32f, 2f), _softAccentTex);

            DrawShadowLabel(new Rect(barRect.x + 18f, barRect.y + 5f, 150f, 18f), "NOTHING MENU", _brandStyle);
            GUI.Label(new Rect(barRect.x + 18f, barRect.y + 23f, 150f, 12f), "DESKTOP OVERLAY", _labelStyle);

            Rect fpsPanel = new Rect(barRect.xMax - 98f, barRect.y + 7f, 78f, 26f);
            GUI.DrawTexture(fpsPanel, _panelTex);
            GUI.DrawTexture(new Rect(fpsPanel.x, fpsPanel.yMax - 3f, fpsPanel.width, 3f), GetFpsTexture());
            GUI.Label(fpsPanel, _fpsDisplay.ToString() + " FPS", _fpsStyle);
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
