using UnityEngine;

namespace Nothing.Menu.UI
{
    internal static class UiStyle
    {
        public static Texture2D MakeTex(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public static void Release(ref Texture2D texture)
        {
            if (texture == null) return;
            Object.Destroy(texture);
            texture = null;
        }

        public static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
        public static Color Darken(Color color, float amount) => Mix(color, Color.black, amount);
        public static Color Mix(Color first, Color second, float amount) => Color.Lerp(first, second, Mathf.Clamp01(amount));
    }
}
