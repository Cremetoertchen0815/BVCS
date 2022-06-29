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
		public Color Color { get; set; }
		public Model Model { get; set; }

		public MaterialAPI[] GetMaterials()
		{
			var ret = new MaterialAPI[1];
			var bodyMaterial = ret[0] = new BasicMaterial();
			bodyMaterial.DiffuseColor = Color;
			return ret;
		}
	}
}
