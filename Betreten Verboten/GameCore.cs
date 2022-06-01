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
			DebugRenderEnabled = false;
			Scene = new FunniTestClass();
			Window.AllowUserResizing = true;
			Scene.SetDefaultDesignResolution(1920, 1080, Scene.SceneResolutionPolicy.BestFit);

		}
	}

	public class FunniTestClass : GeonScene
	{

		VirtualJoystick con;
		VirtualJoystick cam;
		VirtualButton joomp;

		public override void Initialize()
		{
			var phy = AddSceneComponent(new Nez.GeonBit.Physics.PhysicsWorld() { Enabled = false });
			AddRenderer(new GeonRenderer(0, this));
			Camera.Node.PositionZ = 40;
			Camera.Node.PositionY = 5;
			Core.Schedule(1.5f, x => phy.Enabled = true);

			con = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
			cam = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.Left, Microsoft.Xna.Framework.Input.Keys.Right, Microsoft.Xna.Framework.Input.Keys.Up, Microsoft.Xna.Framework.Input.Keys.Down));
			joomp = new VirtualButton(new VirtualButton.KeyboardKey(Microsoft.Xna.Framework.Input.Keys.Space));

			/// Example 3: add 3d shape to scene
			var lol = CreateGeonEntity("test", Vector3.Up * 5);
			var pop = lol.AddComponent(new ShapeRenderer(ShapeMeshes.Sphere), lol.Node);
			pop.Node.Scale = new Vector3(2.7f);
			pop.MaterialOverride.DiffuseColor = Color.Lime;


			// create a physical body for the player (note: make it start height in the air so the player ship will "drop" into scene).
			Vector3 bodySize = new Vector3(2f, 2f, 2f);
			RigidBody shipPhysics = new RigidBody(new SphereInfo(3), mass: 50f, inertia: 4f, 1f);
			shipPhysics.SetDamping(0.5f, 0.5f);
			shipPhysics.Restitution = 1f;
			//shipPhysics.AngularVelocity = new Vector3(5, 5, 0);
			//shipPhysics.LinearVelocity = new Vector3(5, 5, 0);
			shipPhysics.Gravity = Vector3.Down * 27;
			shipPhysics.AngularFactor = new Vector3(4f);
			shipPhysics.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.Player;
			lol.AddComponent(shipPhysics);

			var floor = CreateGeonEntity("floor", new Vector3(0, -2, 0));
			var popp = floor.AddComponent(new ShapeRenderer(ShapeMeshes.Plane), floor.Node);
			popp.RenderingQueue = RenderingQueue.BackgroundNoCull;
			popp.MaterialOverride.DiffuseColor = Color.Violet;
			spos = popp.Node;
			popp.Node.Rotation = new Vector3(MathHelper.PiOver2, 0, 0);
			popp.Node.RotationType = RotationType.Euler;
			popp.Node.Position = new Vector3(0, 2f, 0);
			popp.Node.Scale = new Vector3(50);

			var kin = floor.AddComponent(new KinematicBody(new EndlessPlaneInfo(Vector3.Up)));
			kin.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.Terrain;
			kin.Restitution = 5f;
			kin.Friction = 1f;

			for (int i = 0; i < 1000; i++) SpawnRandomCube();
		}

		private void SpawnRandomCube()
        {
			var e = CreateGeonEntity("lol", new Vector3(Random.NextFloat() * 40 - 20, 15, Random.NextFloat() * 40 - 20));
			var p = e.AddComponent(new RigidBody(new BoxInfo(new Vector3(1, 1, 1)), 1, 2, 0.8f));
			p.Position = e.Node.Position;
			p.SetDamping(0.8f, 0.8f);
			p.Restitution = 5f;
			//p.AngularVelocity = new Vector3(Random.NextFloat(), Random.NextFloat(), Random.NextFloat());
			//p.LinearVelocity = new Vector3(Random.NextFloat(), Random.NextFloat(), Random.NextFloat());
			p.Gravity = Vector3.Down * 5;
			p.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.DynamicObjects;
			var r = e.AddComponent(new ShapeRenderer(ShapeMeshes.Cube), e.Node);
			r.Node.Scale = new Vector3(0.5f);
		}

		Node spos;
		RigidBody nonononode;
		public override void Update()
		{
			base.Update();
			if (nonononode == null) nonononode = FindEntity("test").GetComponent<RigidBody>();
			if (joomp.IsPressed) nonononode.ApplyForce(Vector3.Up * 10000);
			nonononode.LinearVelocity += new Vector3(con.Value.X, 0, con.Value.Y) * 0.4f;
			Camera.Node.Position += new Vector3(cam.Value.X, 0, cam.Value.Y) * 0.4f;
		}
	}
}
