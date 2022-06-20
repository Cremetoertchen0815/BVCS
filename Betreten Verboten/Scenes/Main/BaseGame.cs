﻿using Betreten_Verboten.Components.Base;
using Betreten_Verboten.Components.Base.Boards.BV;
using Betreten_Verboten.Components.BV.Player;
using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Physics;
using Nez.UI;
using System.Collections.Generic;
using static Betreten_Verboten.GlobalFields;

namespace Betreten_Verboten.Scenes.Main
{
	[ManagedScene(100, false)]
	public class BaseGame : GeonScene, ITelegramReceiver
	{
		private const int ABUTTON_WIDTH = 350;
		private const int ABUTTON_HEIGHT = 70;
		private const int ABUTTON_MARGIN_RIGHT = 20;
		private const int ABUTTON_MARGIN_BOTTOM = 20;
		private const int ABUTTON_PADDING_X = 20;
		private const int ABUTTON_PADDING_Y = 20;
		private const int ABUTTON_SPACING = 20;

		protected GeonDefaultRenderer _geonRenderer;

		private VirtualJoystick VirtualJoystick;
		private VirtualJoystick VirtualJoystickB;

		private List<int> _diceNumbers = new List<int>();

		//UI
		private Container _uiPlayerControls;
		private Button _uiPlayerReroll;

		public string TelegramSender => "base";

		public override void Initialize()
		{
			base.Initialize();

			//Register rendering system(Renderers drawing on RenderTextures have negative renderOrders)
			ClearColor = Color.Black;
			Core.DebugRenderEnabled = false;
			AddRenderer(_geonRenderer = new GeonDefaultRenderer(0, this)); //Render render 3D space
			AddRenderer(new ScreenSpaceRenderer(1, RENDER_LAYER_HUD) { WantsToRenderAfterPostProcessors = true }); //Afterwards render HUD on top
			AddPostProcessor(new QualityBloomPostProcessor(0) { BloomPreset = QualityBloomPostProcessor.BloomPresets.Focussed, BloomStrengthMultiplier = 0.6f, BloomThreshold = 0.5f });

			GeonDefaultRenderer.ActiveLightsManager.ShadowsEnabed = false;
			GeonDefaultRenderer.ActiveLightsManager.ShadowViewMatrix = Matrix.CreateLookAt(Vector3.Up * 21, Vector3.Down, Vector3.Forward);

			//CreateGeonEntity("dice", new Vector3(0, 25, 0)).AddComponent(new Dice())

			//Config camera
			Camera.Node.Position = new Vector3(0, 15, 50);

			//Prepare physics
			AddSceneComponent(new PhysicsWorld()).SetGravity(new Vector3(0, -100, 0));
			InitEnvironment();
			InitUI();

			this.TeleRegister();

			VirtualJoystick = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
			VirtualJoystickB = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.Left, Microsoft.Xna.Framework.Input.Keys.Right, Microsoft.Xna.Framework.Input.Keys.Up, Microsoft.Xna.Framework.Input.Keys.Down));
		}

		public void MessageReceived(Telegram message)
		{
			switch(message.Head)
			{
				case "dice_value_set":
					var nr = (int)message.Body;
					_diceNumbers.Add(nr);
					System.Console.WriteLine(nr);
					if (Dice.ShouldReroll(_diceNumbers)) _uiPlayerReroll.SetIsVisible(true); else SwitchToDefaultView();
					break;
			}
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

		protected void InitUI()
        {
			var canvas = CreateEntity("UI").AddComponent(new UICanvas());
			canvas.SetRenderLayer(RENDER_LAYER_HUD);
			//var button = canvas.Stage.AddElement(new Button(ButtonStyle.Create(Color.Gray, Color.Lime, Color.Red)));
			//button.SetBounds(200, 200, 200, 200);
			//button.AddElement(new Label("Moin"));

			//Generate action buttons
			var actBtnStyle = ButtonStyle.Create(Color.Gray, Color.Lime, Color.Red);
			var labelStyle = new LabelStyle();
			_uiPlayerControls = canvas.Stage.AddElement(new Container());
			var panelSize = new Vector2(ABUTTON_WIDTH + ABUTTON_MARGIN_RIGHT + ABUTTON_PADDING_X * 2, ABUTTON_MARGIN_BOTTOM + ABUTTON_HEIGHT * 4 + ABUTTON_SPACING * 3 + ABUTTON_PADDING_Y * 2);
			_uiPlayerControls.SetBounds(1920 - panelSize.X, 1080 - panelSize.Y - ABUTTON_MARGIN_BOTTOM, panelSize.X, panelSize.Y);
			string[] btnNames = { "Dice", "Anger", "Sacrifice", "AfK" };
            for (int i = 0; i < 4; i++)
			{
				//Generate button base
				var actBtnA = _uiPlayerControls.AddElement(new Button(actBtnStyle));
				actBtnA.SetBounds(ABUTTON_SPACING, ABUTTON_SPACING + (ABUTTON_HEIGHT + ABUTTON_SPACING) * i, ABUTTON_WIDTH, ABUTTON_HEIGHT);
				//Add label
				var actBtnLabel = actBtnA.AddElement(new Label(btnNames[i]));

                switch (i)
                {
					case 0:
						actBtnA.OnClicked += x => SwitchToDiceRoll();
						break;
                    default:
                        break;
                }
            }

			//Generate reroll button
			_uiPlayerReroll = canvas.Stage.AddElement(new Button(actBtnStyle));
			_uiPlayerReroll.SetBounds(1920 - ABUTTON_WIDTH - ABUTTON_MARGIN_RIGHT * 2, 1080 - ABUTTON_HEIGHT - ABUTTON_MARGIN_BOTTOM, ABUTTON_WIDTH, ABUTTON_HEIGHT);
			_uiPlayerReroll.SetIsVisible(false);
			_uiPlayerReroll.OnClicked += x => RollDice();
		}

		private void SwitchToDiceRoll()
		{
			//Set camera position
			Camera.LookAt = new Vector3(-500, 2, -500);
			Camera.OverridePosition = new Vector3(-480, 25, -480);
			_uiPlayerControls.SetIsVisible(false);
			_uiPlayerReroll.SetIsVisible(true);
		}

		private void SwitchToDefaultView()
		{
			//Set camera position
			Camera.OverridePosition = Camera.LookAt = null;
			_uiPlayerControls.SetIsVisible(true);
			_uiPlayerReroll.SetIsVisible(false);
		}

		private void RollDice()
        {
			Dice.Throw(this);
			_uiPlayerReroll.SetIsVisible(false);
		}
	}
}
