using Betreten_Verboten.Components.Base;
using Betreten_Verboten.Components.Base.Boards.BV;
using Betreten_Verboten.Components.BV.Player;
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
        protected GeonDefaultRenderer _geonRenderer;

        private VirtualJoystick VirtualJoystick;
        private VirtualJoystick VirtualJoystickB;

        public override void Initialize()
        {
            base.Initialize();

            //Register rendering system(Renderers drawing on RenderTextures have negative renderOrders)
            ClearColor = Color.Black;
            Core.DebugRenderEnabled = false;
            AddRenderer(_geonRenderer = new GeonDefaultRenderer(0, this)); //Render render 3D space
            AddRenderer(new ScreenSpaceRenderer(1, RENDER_LAYER_HUD) { WantsToRenderAfterPostProcessors = true}); //Afterwards render HUD on top
            AddPostProcessor(new QualityBloomPostProcessor(0) { BloomPreset = QualityBloomPostProcessor.BloomPresets.Focussed, BloomStrengthMultiplier = 0.6f, BloomThreshold = 0.5f});

            GeonDefaultRenderer.ActiveLightsManager.ShadowsEnabed = false;
            GeonDefaultRenderer.ActiveLightsManager.ShadowViewMatrix = Matrix.CreateLookAt(Vector3.Up * 21, Vector3.Down, Vector3.Forward);

            //Create dice
            Core.Schedule(5f, x => CreateGeonEntity("dice", new Vector3(0, 25, 0)).AddComponent(new Dice()));

            //Config camera
            Camera.Node.Position = new Vector3(0, 15, 50);

            //Prepare physics
            AddSceneComponent(new PhysicsWorld());
            InitEnvironment();

            VirtualJoystick = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
            VirtualJoystickB = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.Left, Microsoft.Xna.Framework.Input.Keys.Right, Microsoft.Xna.Framework.Input.Keys.Up, Microsoft.Xna.Framework.Input.Keys.Down));
        }

        public override void Update()
        {
            base.Update();
            Camera.Node.Rotation -= new Vector3(VirtualJoystick.Value.Y, VirtualJoystick.Value.X, 0) * 0.01f;
            Camera.Node.Position -= new Vector3(0, VirtualJoystickB.Value.Y, VirtualJoystickB.Value.X);
        }

        protected void InitEnvironment()
        {
            CreateGeonEntity("skybox").AddComponent(new SkyBox() { RenderingQueue = RenderingQueue.SolidBackNoCull }); //Create skybox

            //Create playing field
            var board = CreateGeonEntity("board", NodeType.Simple).AddComponent(new BVPlusBoard());
			for (int i = 0; i < board.PlayerCount; i++)
			{
                CreateGeonEntity("player_" + i).AddComponent(new LocalPlayer(i));
			}
        }
    }
}
