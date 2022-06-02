using Microsoft.Xna.Framework;
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
			ClearColor = Color.Black;

			var phy = AddSceneComponent(new Nez.GeonBit.Physics.PhysicsWorld() { Enabled = false });
			AddRenderer(new GeonRenderer(0, this));
			GeonRenderer.ActiveLightsManager.AmbientLight = Color.White * 0.2f;
			GeonRenderer.ActiveLightsManager.AddLight(new Nez.GeonBit.Lights.LightSource() { Color = Color.White, Position = new Vector3() });
			GeonRenderer.ActiveLightsManager.AddLight(new Nez.GeonBit.Lights.LightSource() { Color = Color.LightYellow, Position = new Vector3(1, 5, 5), Specular = 1 });
			AddRenderer(new DefaultRenderer(-1));
			
			CreateEntity("Waddup").AddComponent(new TextComponent(Graphics.Instance.BitmapFont, "Halloel", new Vector2(50, 50), Color.White));


			CreateGeonEntity("sky").AddComponent(new SkyBox(null));


			Camera.Node.PositionZ = 40;
			Camera.Node.PositionY = 5;
			Core.Schedule(1.5f, x => phy.Enabled = true);

			con = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
			cam = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.Left, Microsoft.Xna.Framework.Input.Keys.Right, Microsoft.Xna.Framework.Input.Keys.Up, Microsoft.Xna.Framework.Input.Keys.Down));
			joomp = new VirtualButton(new VirtualButton.KeyboardKey(Microsoft.Xna.Framework.Input.Keys.Space));

			/// Example 3: add 3d shape to scene
			var lol = CreateGeonEntity("test", Vector3.Up * 5);
			var pop = lol.AddComponent(new ModelRenderer("laal"), lol.Node);
			popo = pop.Node;
			pop.Node.Scale = new Vector3(2.7f);
			pop.Node.RotationX = 0.2f;
			pop.Node.RotationY = 0.2f;

			pop.SetMaterial(new NormalMapLitMaterial() { NormalTexture = Content.LoadTexture("normal"), Texture = Content.LoadTexture("tex"), TextureEnabled = true });
		}

		Node popo;

        public override void Update()
        {
			base.Update();
			popo.RotationX += 0.005f;
			popo.RotationY += 0.005f;

		}
    }
}
