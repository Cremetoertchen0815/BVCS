using Microsoft.Xna.Framework.Audio;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base
{
    public static class SFXManager
    {
        public static Dictionary<string, SoundEffect> LoadRegularSFX(Scene scene)
        {
            string[] titles = {"jump", "land", "sad", "shock", "happy", "stomp", "switch", "triumph" };
            return titles.Select(s => (s, scene.Content.Load<SoundEffect>("sound/sfx/" + s))).ToDictionary(x => x.Item1, y => y.Item2);
        }
    }
}
