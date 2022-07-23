using Microsoft.Xna.Framework.Graphics;
using Nez.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.GeonBit
{
    public static class Extension
    {
        public static Model LoadModel(this NezContentManager c, string path) => c.LoadModel(path, x => Materials.DefaultMaterialsFactory.GetDefaultMaterial(x));
    }
}
