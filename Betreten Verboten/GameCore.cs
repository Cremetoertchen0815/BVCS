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
			DebugRenderEnabled = true;
			Scene = new FunniTestClass();

		}
	}

	public class FunniTestClass : GeonScene
	{

		VirtualJoystick con;
		VirtualJoystick cam;

		public override void Initialize()
		{
			AddSceneComponent(new Nez.GeonBit.Physics.PhysicsWorld());
			AddRenderer(new GeonRenderer(0, this));
			Camera.Node.PositionZ = 40;

			con = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
			cam = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.Left, Microsoft.Xna.Framework.Input.Keys.Right, Microsoft.Xna.Framework.Input.Keys.Up, Microsoft.Xna.Framework.Input.Keys.Down));

			/// Example 3: add 3d shape to scene
			var lol = CreateGeonEntity("test", Vector3.Up * 15);
			var pop = lol.AddComponent(new ShapeRenderer(ShapeMeshes.Cube));


			// create a physical body for the player (note: make it start height in the air so the player ship will "drop" into scene).
			Vector3 bodySize = new Vector3(2f, 2f, 2f);
			RigidBody shipPhysics = new RigidBody(new BoxInfo(bodySize), mass: 10f, inertia: 4f, 1f);
			shipPhysics.SetDamping(0.5f, 0.5f);
			shipPhysics.Restitution = 1f;
			shipPhysics.AngularVelocity = new Vector3(5, 5, 0);
			shipPhysics.LinearVelocity = new Vector3(5, 5, 0);
			shipPhysics.Gravity = Vector3.Down * 27;
			shipPhysics.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.Player;
			lol.AddComponent(shipPhysics);

			var floor = CreateGeonEntity("floor", new Vector3(0, -2, 0));
			var popp = floor.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Plane));
			popp.Node.Rotation = new Vector3(0, 0, MathHelper.PiOver2);
			popp.Node.RotationType = RotationType.Euler;

			var kin = floor.AddComponent(new KinematicBody(new EndlessPlaneInfo(Vector3.Up)));
			kin.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.Terrain;
			kin.Restitution = 0.4f;
			kin.Friction = 1f;
		}

		RigidBody nonononode;
		public override void Update()
		{
			base.Update();
			if (nonononode == null) nonononode = FindEntity("test").GetComponent<RigidBody>();
			nonononode.AngularVelocity += new Vector3(con.Value.X, 0, con.Value.Y) * 0.4f;
			Camera.Node.Position += new Vector3(cam.Value.X, 0, cam.Value.Y) * 0.4f;
		}
	}
}
