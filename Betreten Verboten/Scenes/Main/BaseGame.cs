using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using Nez.GeonBit.Physics;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Betreten_Verboten.GlobalFields;

namespace Betreten_Verboten.Scenes.Main
{
    [ManagedScene(100, false)]
    public class BaseGame : GeonScene
    {
        protected GeonRenderer _geonRenderer;

        private VirtualJoystick VirtualJoystick;

        public override void Initialize()
        {
            base.Initialize();

            //Register rendering system(Renderers drawing on RenderTextures have negative renderOrders)
            AddRenderer(_geonRenderer = new GeonRenderer(0, this)); //Render render 3D space
            AddRenderer(new ScreenSpaceRenderer(2, RENDER_LAYER_HUD) { WantsToRenderAfterPostProcessors = true}); //Afterwards render HUD on top

            //Config camera
            Camera.Node.Position = new Vector3(0, 10, 100);

            //Prepare physics
            AddSceneComponent(new PhysicsWorld());
            InitEnvironment();

            VirtualJoystick = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
        }

        public override void Update()
        {
            base.Update();
            Camera.Node.Position += new Vector3(0, VirtualJoystick.Value.Y, VirtualJoystick.Value.X);
        }

        protected void InitEnvironment()
        {
            CreateGeonEntity("skybox").AddComponent(new SkyBox()); //Create skybox

            //Create playing field
            var playfield = CreateGeonEntity("playfield", NodeType.Simple);
        }
    }
}
