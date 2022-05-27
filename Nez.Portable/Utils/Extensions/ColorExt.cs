using Microsoft.Xna.Framework;


namespace Nez
{
    public static class ColorExt
    {
        private const string HEX = "0123456789ABCDEF";


        public static byte HexToByte(char c) => (byte)HEX.IndexOf(char.ToUpper(c));

        public static Color Invert(this Color color) => new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);

        public static Color HexToColor(string hex)
        {
            float r = (HexToByte(hex[0]) * 16 + HexToByte(hex[1])) / 255.0f;
            float g = (HexToByte(hex[2]) * 16 + HexToByte(hex[3])) / 255.0f;
            float b = (HexToByte(hex[4]) * 16 + HexToByte(hex[5])) / 255.0f;

            return new Color(r, g, b);
        }

        public static Color HexToColor(int hex)
        {
            byte r = (byte)(hex >> 16);
            byte g = (byte)(hex >> 8);
            byte b = (byte)(hex >> 0);

            return new Color(r, g, b);
        }

        public static Color Create(Color color, int alpha)
        {
            var newColor = new Color
            {
                PackedValue = 0,

                R = color.R,
                G = color.G,
                B = color.B,
                A = (byte)MathHelper.Clamp(alpha, byte.MinValue, byte.MaxValue)
            };
            return newColor;
        }

        public static Color Create(Color color, float alpha)
        {
            var newColor = new Color
            {
                PackedValue = 0,

                R = color.R,
                G = color.G,
                B = color.B,
                A = (byte)MathHelper.Clamp(alpha * 255, byte.MinValue, byte.MaxValue)
            };
            return newColor;
        }

        public static Color Grayscale(this Color color) => new Color((int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11),
                (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11),
                (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11),
                color.A);

        public static Color Add(this Color color, Color second) => new Color(color.R + second.R, color.G + second.G, color.B + second.B, color.A + second.A);

        /// <summary>
        /// first - second
        /// </summary>
        public static Color Subtract(this Color color, Color second) => new Color(color.R - second.R, color.G - second.G, color.B - second.B, color.A - second.A);

        public static Color Multiply(this Color self, Color second) => new Color
        {
            R = (byte)(self.R * second.R / 255),
            G = (byte)(self.G * second.G / 255),
            B = (byte)(self.B * second.B / 255),
            A = (byte)(self.A * second.A / 255)
        };

        /// <summary>
        /// linearly interpolates Color from - to
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="t">T.</param>
        public static Color Lerp(Color from, Color to, float t)
        {
            int t255 = (int)(t * 255);
            return new Color(from.R + (to.R - from.R) * t255 / 255, from.G + (to.G - from.G) * t255 / 255,
                from.B + (to.B - from.B) * t255 / 255, from.A + (to.A - from.A) * t255 / 255);
        }

        /// <summary>
        /// linearly interpolates Color from - to
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="t">T.</param>
        public static void Lerp(ref Color from, ref Color to, out Color result, float t)
        {
            result = new Color();
            int t255 = (int)(t * 255);
            result.R = (byte)(from.R + (to.R - from.R) * t255 / 255);
            result.G = (byte)(from.G + (to.G - from.G) * t255 / 255);
            result.B = (byte)(from.B + (to.B - from.B) * t255 / 255);
            result.A = (byte)(from.A + (to.A - from.A) * t255 / 255);
        }

        public static (float, float, float) RgbToHsl(Color color)
        {
            float h = 0f;
            float s = 0f;
            float l = 0f;

            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float min = MathHelper.Min(MathHelper.Min(r, g), b);
            float max = MathHelper.Max(MathHelper.Max(r, g), b);
            float delta = max - min;

            // luminance is the ave of max and min
            l = (max + min) / 2f;

            if (delta > 0)
            {
                if (l < 0.5f)
                    s = delta / (max + min);
                else
                    s = delta / (2 - max - min);

                float deltaR = (((max - r) / 6f) + (delta / 2f)) / delta;
                float deltaG = (((max - g) / 6f) + (delta / 2f)) / delta;
                float deltaB = (((max - b) / 6f) + (delta / 2f)) / delta;

                if (r == max)
                    h = deltaB - deltaG;
                else if (g == max)
                    h = (1f / 3f) + deltaR - deltaB;
                else if (b == max)
                    h = (2f / 3f) + deltaG - deltaR;

                if (h < 0)
                    h += 1;

                if (h > 1)
                    h -= 1;
            }

            return (h, s, l);
        }

        public static Color HslToRgb(float h, float s, float l)
        {
            float HueToRgb(float v1, float v2, float vH)
            {
                vH += (vH < 0) ? 1 : 0;
                vH -= (vH > 1) ? 1 : 0;
                float ret = v1;

                if ((6 * vH) < 1)
                    ret = (v1 + (v2 - v1) * 6 * vH);
                else if ((2 * vH) < 1)
                    ret = v2;

                else if ((3 * vH) < 2)
                    ret = v1 + (v2 - v1) * ((2f / 3f) - vH) * 6f;

                return Mathf.Clamp01(ret);
            }

            var c = new Color
            {
                A = 255
            };

            if (s == 0)
            {
                c.R = (byte)(l * 255f);
                c.G = (byte)(l * 255f);
                c.B = (byte)(l * 255f);
            }
            else
            {
                float v2 = (l + s) - (s * l);
                if (l < 0.5f)
                    v2 = l * (1 + s);

                float v1 = 2f * l - v2;

                c.R = (byte)(255f * HueToRgb(v1, v2, h + (1f / 3f)));
                c.G = (byte)(255f * HueToRgb(v1, v2, h));
                c.B = (byte)(255f * HueToRgb(v1, v2, h - (1f / 3f)));
            }

            return c;
        }
    }
}