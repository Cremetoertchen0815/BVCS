using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.GeonBit
{
    public class UIRenderer : Renderer
    {
        private UserInterface _ui;
        private SpriteBatch _spr;

        public UIRenderer(int renderOrder) : base(renderOrder) => _ui = UserInterface.Active;
        public override void Render(Scene scene)
        {
            _ui.Draw(null);
        }
    }
}
