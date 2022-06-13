using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base.Characters
{
    public struct CharConfig
    {
        public Texture2D FaceTexture { get; set; }
        public SoundEffect SoundEffects { get; set; }
        public object[] Emotes { get; set; }
        public string MOTD { get; set; }
        public Color Color;
        public Model Model { get; set; }

        public MaterialAPI[] GetMaterials()
		{
            var ret = new MaterialAPI[1];
            var bodyMaterial = ret[0] = new BasicMaterial();
            bodyMaterial.DiffuseColor = Color.White;
            return ret;
		}
    }
}
