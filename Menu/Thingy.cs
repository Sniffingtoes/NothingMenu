using UnityEngine;
using UnityEngine.InputSystem;

namespace Nothing.Menu
{
    public class Thingy : MonoBehaviour
    {
        private GUIStyle _brandStyle, _fpsStyle, _labelStyle;
        private Texture2D _barTex, _panelTex, _accentTex, _goodTex, _warnTex, _badTex;
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

            _barTex = MakeTex(WithAlpha(Darken(theme.Background, 0.58f), 0.94f));
            _panelTex = MakeTex(WithAlpha(Mix(theme.Background, theme.Button, 0.55f), 0.96f));
            _accentTex = MakeTex(accent);
            _goodTex = MakeTex(new Color(0.32f, 0.78f, 0.48f, 1f));
            _warnTex = MakeTex(new Color(0.92f, 0.68f, 0.25f, 1f));
            _badTex = MakeTex(new Color(0.92f, 0.32f, 0.38f, 1f));

            _brandStyle = new GUIStyle { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = text } };
            _fpsStyle = new GUIStyle { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = text } };
            _labelStyle = new GUIStyle { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = mutedText } };
            _stylesInitialized = true;
            _styleThemeIndex = ThemeManager.currentThemeIndex;
        }

        void OnGUI()
        {
            if (!_showFPS) return;

            InitializeStyles();

            float width = 328f;
            float height = 42f;
            float xPos = (Screen.width - width) / 2f;
            Rect barRect = new Rect(xPos, 18f, width, height);

            GUI.DrawTexture(barRect, _barTex);
            GUI.DrawTexture(new Rect(barRect.x, barRect.y, 4f, barRect.height), _accentTex);

            GUI.Label(new Rect(barRect.x + 18f, barRect.y + 6f, 140f, 18f), "NOTHING MENU", _brandStyle);
            GUI.Label(new Rect(barRect.x + 18f, barRect.y + 23f, 140f, 12f), "DESKTOP OVERLAY", _labelStyle);

            Rect fpsPanel = new Rect(barRect.xMax - 100f, barRect.y + 7f, 78f, 28f);
            GUI.DrawTexture(fpsPanel, _panelTex);
            GUI.DrawTexture(new Rect(fpsPanel.x, fpsPanel.yMax - 3f, fpsPanel.width, 3f), GetFpsTexture());
            GUI.Label(fpsPanel, _fpsDisplay.ToString() + " FPS", _fpsStyle);
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
