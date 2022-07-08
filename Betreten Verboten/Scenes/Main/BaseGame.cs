using Betreten_Verboten.Components.Base;
using Betreten_Verboten.Components.Base.Boards.BV;
using Betreten_Verboten.Components.Base.HUD;
using Betreten_Verboten.Components.BV.Player;
using Microsoft.Xna.Framework;
using System.Linq;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.ECS;
using Nez.GeonBit.Physics;
using Nez.GeonBit.UI;
using Nez.GeonBit.UI.Entities;
using System.Collections.Generic;
using static Betreten_Verboten.GlobalFields;
using Betreten_Verboten.Components.Base.Characters;

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

        private int _activePlayer = -1;
        private Player[] _players;
        private BVBoard _board;
        private List<int> _diceNumbers = new List<int>();
        private GameState _gameState = GameState.OtherAction;

        //UI
        private int[] _playerIndices;
        private bool _isScoreVisible = false;
        private VirtualButton _scoreBtn;
        private Panel[] _uiPlayerHUDs;
        private Panel _uiPlayerControls;
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

            //Config camera5
            Camera.Node.Position = new Vector3(0, 25, 40);
            Camera.Node.RotationX = -0.5f;

            //Prepare physics
            AddSceneComponent(new PhysicsWorld()).SetGravity(new Vector3(0, -100, 0));
            InitEnvironment();

            //Init UI
            _scoreBtn = new VirtualButton(new VirtualButton.KeyboardKey(Microsoft.Xna.Framework.Input.Keys.Tab));
            FinalRenderDelegate = Core.GetGlobalManager<FinalUIRender>();
            UserInterface.Active.Clear();
            InitUI();

            //Generate player indices
            _playerIndices = new int[_board.PlayerCount];
            for (int i = 0; i < _playerIndices.Length; i++) _playerIndices[_playerIndices.Length - i - 1] = i;

            //Init
            AdvancePlayer();

            this.TeleRegister();
        }
        public override void Unload() => this.TeleDeregister();

        public void MessageReceived(Telegram message)
        {
            switch (message.Head)
            {
                case "dice_value_set":
                    int nr = (int)message.Body;
                    _diceNumbers.Add(nr);
                    if (Dice.ShouldReroll(_diceNumbers))
                    {
                        _uiPlayerReroll.Visible = true;
                    }
                    else
                    {
                        _players[_activePlayer].DecideAfterDiceroll(_diceNumbers);
                        GameState = GameState.PieceSelect;
                    }
                    break;
                case "char_move_done":
                case "advance_player":
                    AdvancePlayer();
                    break;
                case "resort_score":
                    ReorderPlayerHUD();
                    break;
            }
        }

        protected void InitEnvironment()
        {
            CreateGeonEntity("skybox").AddComponent(new SkyBox() { RenderingQueue = RenderingQueue.SolidBackNoCull }); //Create skybox

            //Create playing field
            _board = CreateGeonEntity("board", NodeType.Simple).AddComponent(new BVPlusBoard());
            _players = new Player[_board.PlayerCount];
            for (int i = 0; i < _board.PlayerCount; i++) _players[i] = CreateGeonEntity("player_" + i).AddComponent(new LocalPlayer(i));
        }

        protected void InitUI()
        {
            //Add controll panel
            _uiPlayerControls = UserInterface.Active.AddEntity(new Panel(new Vector2(250, 400), PanelSkin.Simple, Anchor.BottomRight, new Vector2(15)));
            var btnA = _uiPlayerControls.AddChild(new Button("Dice", ButtonSkin.Alternative, Anchor.TopCenter, new Vector2(200, 80)) { OnClick = x => GameState = GameState.DiceRoll });
            var btnB = _uiPlayerControls.AddChild(new Button("Anger", ButtonSkin.Alternative, Anchor.AutoCenter, new Vector2(200, 80)));
            var btnC = _uiPlayerControls.AddChild(new Button("Sacrifice", ButtonSkin.Alternative, Anchor.AutoCenter, new Vector2(200, 80)));
            var btnD = _uiPlayerControls.AddChild(new Button("AfK", ButtonSkin.Alternative, Anchor.AutoCenter, new Vector2(200, 80)));
            _uiPlayerReroll = UserInterface.Active.AddEntity(new Button("Roll Dice", ButtonSkin.Alternative, Anchor.BottomRight, new Vector2(200, 80), new Vector2(15)) { Visible = false, OnClick = x => RollDice() });

            //Add controll panel hints
            btnA.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.A), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));
            btnB.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.B), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));
            btnC.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.X), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));
            btnD.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.Y), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));
            _uiPlayerReroll.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.A), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));

            //Add player hud
            _uiPlayerHUDs = new Panel[_board.PlayerCount];
            var scores = new DynamicLabel[_board.PlayerCount];
            var chrCfg = new CharConfig();
            for (int i = 0; i < _board.PlayerCount; i++)
            {
                int ii = i;
                chrCfg.SetStdColorScheme(i);
                _uiPlayerHUDs[i] = UserInterface.Active.AddEntity(new Panel(new Vector2(200, 70), PanelSkin.Default, Anchor.TopLeft, new Vector2(5, 15 + i * 75)) { FillColor = chrCfg.Color }); //
                _uiPlayerHUDs[i].AddChild(new Image(Graphics.Instance.DebugSprite.Texture2D, new Vector2(40), ImageDrawMode.Stretch, Anchor.CenterLeft));
                _uiPlayerHUDs[i].AddChild(new Label("Player " + i, Anchor.TopLeft, null, new Vector2(50, 0)));
                scores[i] = _uiPlayerHUDs[i].AddChild(new DynamicLabel(() => _players[ii].Points.ToString(), Anchor.TopRight));
                scores[i].Visible = false;
                
            }
            UserInterface.Active.AddEntity(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.LT), new Vector2(50, 51), ImageDrawMode.Stretch, Anchor.Auto, new Vector2(80, 0)));

            //Implement score toggle
            _scoreBtn.ButtonReleased += () =>
            {
                if (_isScoreVisible)
                {
                    for (int i = 0; i < _board.PlayerCount; i++)
                    {
                        scores[i].Visible = false;
                        _uiPlayerHUDs[i].Tween("Size", new Vector2(200, 70), 0.1f).Start();
                        _isScoreVisible = false;
                    }
                }
            };
            _scoreBtn.ButtonPressed += () =>
            {
                if (!_isScoreVisible)
                {
                    for (int i = 0; i < _board.PlayerCount; i++)
                    {
                        int ii = i;
                        _uiPlayerHUDs[ii].Tween("Size", new Vector2(400, 70), 0.1f).SetCompletionHandler(x => scores[ii].Visible = true).Start();
                        _isScoreVisible = true;
                    }
                }
            };
        }

        public void ReorderPlayerHUD()
        {
            //Resort
            var ord = _playerIndices.OrderBy(x => _players[x].Points);
            if (ord.SequenceEqual(_playerIndices)) return;
            _playerIndices = ord.ToArray();

            //Execute animations
            for (int i = 0; i < _board.PlayerCount; i++)
            {
                int ii = i;
                _uiPlayerHUDs[ _playerIndices[_playerIndices.Length - 1 - i]].Tween("Offset", new Vector2(5, 15 + ii * 75), 0.5f).SetEaseType(Nez.Tweens.EaseType.QuadInOut).Start();
            }
        }

        public GameState GameState
        {
            get => _gameState;
            set
            {
                //Switch for old state
                switch (_gameState)
                {
                    case GameState.DiceRoll:
                        FindEntitiesWithTag(Dice.ENTITY_TAG).ForEach(x => x.Destroy());
                        break;
                }

                //Switch for new state
                switch (_gameState = value)
                {
                    case GameState.DiceRoll:
                        //Set camera position
                        Camera.LookAt = new Vector3(-500, 2, -500);
                        Camera.OverridePosition = new Vector3(-480, 25, -480);
                        //Refresh UI elements
                        _uiPlayerControls.Visible = false;
                        _uiPlayerReroll.Visible = true;
                        _diceNumbers.Clear(); //Clear dice queue
                        break;
                    case GameState.ActionSelect:
                        //Set camera position
                        Camera.OverridePosition = Camera.LookAt = null;
                        _uiPlayerControls.Visible = true;
                        _uiPlayerReroll.Visible = false;
                        break;
                    default:
                        //Set camera position
                        Camera.OverridePosition = Camera.LookAt = null;
                        _uiPlayerControls.Visible = false;
                        _uiPlayerReroll.Visible = false;
                        break;
                }
            }
        }

        private void RollDice()
        {
            Dice.Throw(this);
            _uiPlayerReroll.Visible = false;
        }

        public void AdvancePlayer()
        {
            _activePlayer = ++_activePlayer % _board.PlayerCount;
            _uiPlayerControls.FillColor = CharConfig.GetStdColor(_activePlayer) * 0.95f;
            GameState = GameState.ActionSelect;
        }
    }
}
