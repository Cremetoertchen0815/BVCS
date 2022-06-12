﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
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
    }
}
