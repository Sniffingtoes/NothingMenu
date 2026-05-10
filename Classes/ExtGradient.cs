using System;
using System.Linq;

using UnityEngine;

namespace Nothing.Classes
{
    public class ExtGradient
    {
        public static GradientColorKey[] GetSolidGradient(Color color) =>
            new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) };

        public GradientColorKey[] colors = GetSolidGradient(Color.black);

        public Color GetColor(int index)
        {
            if (rainbow) return Color.HSVToRGB((Time.time + (index / 8f)) % 1f, 1f, 1f);
            if (pastelRainbow) return Color.HSVToRGB((Time.time + (index / 8f)) % 1f, 0.3f, 1f);
            if (epileptic) return UnityEngine.Random.ColorHSV();
            if (customColor != null) return customColor?.Invoke() ?? Color.black;
            return colors[index].color;
        }

        public Color GetColorTime(float time)
        {
            if (rainbow) return Color.HSVToRGB(time, 1f, 1f);
            if (pastelRainbow) return Color.HSVToRGB(time, 0.3f, 1f);
            if (customColor != null) return customColor?.Invoke() ?? Color.black;
            return new Gradient { colorKeys = colors }.Evaluate(time);
        }

        public Color GetCurrentColor(float offset = 0f) =>
            GetColorTime((offset + (Time.time * 0.5f)) % 1f);

        public bool IsFlat() =>
            !rainbow && !pastelRainbow && !epileptic && !copyRigColor &&
            colors != null && colors.Length > 0 && colors.All(key => key.color == colors[0].color);

        public bool rainbow;
        public bool pastelRainbow;
        public bool epileptic;
        public bool copyRigColor;
        public bool transparent;
        public Func<Color> customColor;
    }
}
