using Betreten_Verboten.Components.Base;
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
            AddRenderer(_geonRenderer = new GeonDefaultRenderer(0, this)); //Render render 3D space
            AddRenderer(new ScreenSpaceRenderer(1, RENDER_LAYER_HUD) { WantsToRenderAfterPostProcessors = true}); //Afterwards render HUD on top

            GeonDefaultRenderer.ActiveLightsManager.ShadowsEnabed = true;
            GeonDefaultRenderer.ActiveLightsManager.ShadowViewMatrix = Matrix.CreateLookAt(Vector3.Up * 21, Vector3.Down, Vector3.Forward);

            var soooooos = CreateGeonEntity("sadadsf", new Vector3(0, 0, 0)).AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Cube));
            soooooos.Node.Scale = new Vector3(2.5f);
            var sdop = soooooos.Entity.AddComponent(new Nez.GeonBit.RigidBody(new BoxInfo(Vector3.One * 5), 10, 10, 1));
            sdop.SetDamping(0.95f, 0.95f);
            sdop.Restitution = 2f;
            //shipPhysics.AngularVelocity = new Vector3(5, 5, 0);
            //shipPhysics.LinearVelocity = new Vector3(5, 5, 0);
            sdop.Position = new Vector3(0, 100, 0);
            sdop.Gravity = Vector3.Down * 50;
            sdop.CollisionGroup = CollisionGroups.Player;

            for (int i = 0; i < 50; i++)
            {
                SpawnRandomCube();
            }

            //Config camera
            Camera.Node.Position = new Vector3(0, 1, 50);

            //Prepare physics
            AddSceneComponent(new PhysicsWorld());
            InitEnvironment();

            VirtualJoystick = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
            VirtualJoystickB = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.Left, Microsoft.Xna.Framework.Input.Keys.Right, Microsoft.Xna.Framework.Input.Keys.Up, Microsoft.Xna.Framework.Input.Keys.Down));
        }


        private void SpawnRandomCube()
        {
            var e = CreateGeonEntity("lol", new Vector3(Nez.Random.NextFloat() * 40 - 20, 15, Nez.Random.NextFloat() * 40 - 20));
            var p = e.AddComponent(new Nez.GeonBit.RigidBody(new BoxInfo(new Vector3(1, 1, 1)), 1, 2, 0.8f));
            p.Position = e.Node.Position;
            p.SetDamping(0.95f, 0.95f);
            p.Restitution = 2f;
            //p.AngularVelocity = new Vector3(Random.NextFloat(), Random.NextFloat(), Random.NextFloat());
            //p.LinearVelocity = new Vector3(Random.NextFloat(), Random.NextFloat(), Random.NextFloat());
            p.Gravity = Vector3.Down * 50;
            p.CollisionGroup = (short)CollisionGroups.DynamicObjects;
            var r = e.AddComponent(new ShapeRenderer(ShapeMeshes.Cube), e.Node);
            r.Node.Scale = new Vector3(0.5f);
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
            CreateGeonEntity("board", NodeType.Simple).AddComponent(new Board());
        }
    }
}
