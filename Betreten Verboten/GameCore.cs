using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;

namespace Betreten_Verboten
{
	class GameCore : Core
	{
		protected override void Initialize()
		{
			base.Initialize();
			DebugRenderEnabled = true;
			Window.AllowUserResizing = true;
			RegisterGlobalManager(new SceneManager(System.Reflection.Assembly.GetAssembly(GetType()), "Betreten_Verboten.Scenes"));

			GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
			Screen.EnableAA = true;
			Screen.ApplyChanges();
			Scene.SetDefaultDesignResolution(1920, 1080, Scene.SceneResolutionPolicy.BestFit);

			Scene = new Scenes.Main.BaseGame();

		}
	}

	public class FunniTestClass : GeonScene
	{

		VirtualJoystick con;
		VirtualJoystick cam;
		VirtualButton joomp;

		public override void Initialize()
		{
			ClearColor = Color.Black;

			var phy = AddSceneComponent(new Nez.GeonBit.Physics.PhysicsWorld() { Enabled = false });
			var rend = AddRenderer(new GeonDefaultRenderer(0, this));
			GeonDefaultRenderer.ActiveLightsManager.AmbientLight = Color.White * 0.2f;
			GeonDefaultRenderer.ActiveLightsManager.AddLight(new Nez.GeonBit.Lights.LightSource() { Color = Color.White, Position = new Vector3() });
			GeonDefaultRenderer.ActiveLightsManager.AddLight(new Nez.GeonBit.Lights.LightSource() { Color = Color.LightYellow, Position = new Vector3(1, 5, 5), Specular = 1 });
			GeonDefaultRenderer.ActiveLightsManager.AddLight(new Nez.GeonBit.Lights.LightSource() { Color = Color.Lime, Position = new Vector3(-1, 5, 0), Specular = 1 });
			AddRenderer(new DefaultRenderer(-1));
			Core.Schedule(1.5f, x => phy.Enabled = true);

			CreateEntity("Waddup").AddComponent(new TextComponent(Graphics.Instance.BitmapFont, "Halloel", new Vector2(50, 50), Color.White));


			CreateGeonEntity("sky").AddComponent(new SkyBox(null)).RenderingQueue = RenderingQueue.BackgroundNoCull;
			AddPostProcessor(new QualityBloomPostProcessor(1) { BloomPreset = QualityBloomPostProcessor.BloomPresets.Focussed, BloomThreshold = 0.3f, BloomStrengthMultiplier = 0.8f});
			

			Camera.Node.PositionZ = 40;
			Camera.Node.PositionY = 5;

			var leel = CreateGeonEntity("ref", Vector3.Up * 5);
			RigidBody ss = new RigidBody(new SphereInfo(3), mass: 50f, inertia: 4f, 1f);
			ss.SetDamping(0.95f, 0.95f);
			ss.Restitution = 1f;
			//shipPhysics.AngularVelocity = new Vector3(5, 5, 0);
			//shipPhysics.LinearVelocity = new Vector3(5, 5, 0);
			ss.Gravity = Vector3.Down * 27;
			ss.AngularFactor = new Vector3(4f);
			ss.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.Player;
			leel.AddComponent(ss);


			var peep = leel.AddComponent(new ShapeRenderer(ShapeMeshes.Cube), leel.Node);
			peep.Node.Scale = new Vector3(2.7f);

			var loopo = new TextureCube(Core.GraphicsDevice, 1000, true, SurfaceFormat.Color);
			var arr = new Color[1000 * 1000];
			Content.LoadTexture("cubemap/left").GetData(arr);
			loopo.SetData(CubeMapFace.PositiveX, arr);
			Content.LoadTexture("cubemap/right").GetData(arr);
			loopo.SetData(CubeMapFace.NegativeX, arr);
			Content.LoadTexture("cubemap/top").GetData(arr);
			loopo.SetData(CubeMapFace.PositiveY, arr);
			Content.LoadTexture("cubemap/down").GetData(arr);
			loopo.SetData(CubeMapFace.NegativeY, arr);
			Content.LoadTexture("cubemap/front").GetData(arr);
			loopo.SetData(CubeMapFace.PositiveZ, arr);
			Content.LoadTexture("cubemap/back").GetData(arr);
			loopo.SetData(CubeMapFace.NegativeZ, arr);

			peep.SetMaterial(new ReflectiveMaterial() { EnvironmentMap = loopo, EnvironmentAmount = 0.8f, FresnelFactor = 0.5f, EnvironmentSpecular = Color.White * 0.4f, EmissiveLight = Color.Blue, DiffuseColor = Color.Pink });

			//peep.SetMaterial(new ReflectiveMaterial() { EnvironmentMap = loopo, EnvironmentAmount = 1 });


			con = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
			cam = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.Left, Microsoft.Xna.Framework.Input.Keys.Right, Microsoft.Xna.Framework.Input.Keys.Up, Microsoft.Xna.Framework.Input.Keys.Down));
			joomp = new VirtualButton(new VirtualButton.KeyboardKey(Microsoft.Xna.Framework.Input.Keys.Space));

			/// Example 3: add 3d shape to scene
			var lol = CreateGeonEntity("test", Vector3.Up * 5);
			var pop = lol.AddComponent(new ModelRenderer("piece_std"), lol.Node);
			pop.Node.Scale = new Vector3(0.5f);


			// create a physical body for the player (note: make it start height in the air so the player ship will "drop" into scene).
			Vector3 bodySize = new Vector3(2f, 2f, 2f);
			RigidBody shipPhysics = new RigidBody(new SphereInfo(3), mass: 50f, inertia: 4f, 1f);
			shipPhysics.SetDamping(0.95f, 0.95f);
			shipPhysics.Restitution = 1f;
			//shipPhysics.AngularVelocity = new Vector3(5, 5, 0);
			//shipPhysics.LinearVelocity = new Vector3(5, 5, 0);
			shipPhysics.Gravity = Vector3.Down * 27;
			shipPhysics.AngularFactor = new Vector3(4f);
			shipPhysics.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.Player;
			lol.AddComponent(shipPhysics);

			var floor = CreateGeonEntity("floor", new Vector3(0, -2, 0));
			/*var popp = floor.AddComponent(new ShapeRenderer(ShapeMeshes.Plane), floor.Node);
			popp.RenderingQueue = RenderingQueue.BackgroundNoCull;
			popp.MaterialOverride.DiffuseColor = Color.Violet;
			spos = popp.Node;
			popp.Node.Rotation = new Vector3(MathHelper.PiOver2, 0, 0);
			popp.Node.RotationType = RotationType.Euler;
			popp.Node.Position = new Vector3(0, 2f, 0);
			popp.Node.Scale = new Vector3(50); */

			var kin = floor.AddComponent(new KinematicBody(new EndlessPlaneInfo(Vector3.Up)));
			kin.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.Terrain;
			kin.Restitution = 1f;
			kin.Friction = 1f;

			NormalMapLitMaterial = new LitMaterial() { Texture = Content.LoadTexture("tex"), TextureEnabled = true };
			NormalMapLitMaterial.SamplerState = SamplerState.AnisotropicClamp;
			for (int i = 0; i < 1000; i++) SpawnRandomCube();

			pop.SetMaterial(new NormalMapLitMaterial() { NormalTexture = Content.LoadTexture("normal"), Texture = Content.LoadTexture("albedo"), TextureEnabled = true, SamplerState = SamplerStates.AnisotropicClamp });

			var r = new RenderTargetCube(Core.GraphicsDevice, 128, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
			Core.Schedule(0.2f, true, x =>
			{
				var soooooos = rend.CaptureEnvironmentMap(r, peep.Node.WorldPosition, 128);
				pop.SetMaterial(new ReflectiveMaterial() { EnvironmentMap = soooooos, EnvironmentAmount = 0.8f, EnvironmentSpecular = Color.Red * 0.4f, SpecularPower = 2, FresnelFactor = 0.7f, SamplerState = SamplerState.AnisotropicWrap });
			}); 
		}

		private void SpawnRandomCube()
		{
			var e = CreateGeonEntity("lol", new Vector3(Random.NextFloat() * 40 - 20, 15, Random.NextFloat() * 40 - 20));
			var p = e.AddComponent(new RigidBody(new BoxInfo(new Vector3(1, 1, 1)), 1, 2, 0.8f));
			p.Position = e.Node.Position;
			p.SetDamping(0.95f, 0.95f);
			p.Restitution = 1f;
			//p.AngularVelocity = new Vector3(Random.NextFloat(), Random.NextFloat(), Random.NextFloat());
			//p.LinearVelocity = new Vector3(Random.NextFloat(), Random.NextFloat(), Random.NextFloat());
			p.Gravity = Vector3.Down * 20;
			p.CollisionGroup = (short)Nez.GeonBit.Physics.CollisionGroups.DynamicObjects;
			var r = e.AddComponent(new ShapeRenderer(ShapeMeshes.Cube), e.Node);
			r.Node.Scale = new Vector3(0.5f);
			r.SetMaterial(NormalMapLitMaterial);
		}

		Node spos;
		RigidBody nonononode;
		LitMaterial NormalMapLitMaterial;
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
