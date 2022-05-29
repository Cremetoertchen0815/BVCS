using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;

namespace Betreten_Verboten
{
    class GameCore : Core
	{
        protected override void Initialize()
        {
            base.Initialize();
            Scene = new FunniTestClass();

        }
    }

    public class FunniTestClass : Scene
    {
        public override void Initialize()
        {
            base.Initialize();

            AddRenderer(new GeonBitRenderer(0, this));

            var soos = Camera.Entity.AddComponent(new Camera3D());
            soos.Node.PositionZ = 5;

            /// Example 3: add 3d shape to scene
            var lol = CreateEntity("test");
            lol.AddComponent(new GeonNode());
            lol.AddComponent(new ShapeRenderer(ShapeMeshes.Sphere));


        }
    }
}
