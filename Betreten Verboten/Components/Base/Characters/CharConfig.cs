using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Materials;

namespace Betreten_Verboten.Components.Base.Characters
{
    public struct CharConfig
    {
        public Texture2D FaceTexture { get; set; }
        public SoundEffect SoundEffects { get; set; }
        public object[] Emotes { get; set; }
        public string MOTD { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public Model Model { get; set; }

        public void SetStdColorScheme(int i) => Color = GetStdColor(i);
        public static Color GetStdColor(int i)
        {
            switch (i)
            {
                case 0:
                    return Color.Magenta;
                case 1:
                    return Color.Lime;
                case 2:
                    return Color.Cyan;
                case 3:
                    return Color.Yellow;
                default:
                    return Color.White;
            }
        }

        public MaterialAPI[] GetMaterials()
        {
            var ret = new MaterialAPI[1];
            var bodyMaterial = ret[0] = new BasicMaterial();
            bodyMaterial.DiffuseColor = Color;
            return ret;
        }
    }
}
