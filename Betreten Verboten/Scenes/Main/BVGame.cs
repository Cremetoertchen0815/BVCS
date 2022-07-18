using Betreten_Verboten.Components.Base;
using Betreten_Verboten.Components.Base.Boards.BV;
using Betreten_Verboten.Components.Base.Characters;
using Betreten_Verboten.Components.BV;
using Betreten_Verboten.Components.BV.Backgrounds;
using Betreten_Verboten.Components.BV.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.ECS;
using Nez.GeonBit.Physics;
using Nez.GeonBit.UI;
using Nez.GeonBit.UI.Entities;
using Nez.GeonBit.UI.Utils;
using Nez.Tweens;
using System.Collections.Generic;
using System.Linq;

namespace Betreten_Verboten.Scenes.Main
{
    [ManagedScene(100, false)]
    public class BVGame : GeonScene, ITelegramReceiver
    {

        //Consts
        private const int ABUTTON_WIDTH = 350;
        private const int ABUTTON_HEIGHT = 70;
        private const int ABUTTON_MARGIN_RIGHT = 20;
        private const int ABUTTON_MARGIN_BOTTOM = 20;
        private const int ABUTTON_PADDING_X = 20;
        private const int ABUTTON_PADDING_Y = 20;
        private const int ABUTTON_SPACING = 20;

        protected GeonDefaultRenderer _geonRenderer;

        //Fields
        private int _activePlayer = -1;
        private BVPlayer[] _players;
        private BVBoard _board;
        private List<int> _diceNumbers = new List<int>();
        private GameState _gameState = GameState.OtherAction;
        private ThriceRollState _thriceRoll = ThriceRollState.UNABLE;

        //UI
        private int[] _playerIndices;
        private bool _isScoreVisible = false;
        private VirtualButton _scoreBtn;
        private Panel[] _uiPlayerHUDs;
        private Label _uiPlayerName;
        private Label _uiPlayerTutorial;
        private Panel _uiPlayerControls;
        private Button _uiPlayerReroll;
        private Button _uiPlayerAnger;
        private Button _uiPlayerSacrifice;
        private Button _uiPlayerAfK;

        public string TelegramSender => "base";
        public BVBoard Board => _board;

        public override void Initialize()
        {
            base.Initialize();

            //Register rendering system(Renderers drawing on RenderTextures have negative renderOrders)
            ClearColor = Color.Black;
            Core.DebugRenderEnabled = false;
            AddRenderer(_geonRenderer = new GeonDefaultRenderer(1, this)); //Render render 3D space
            AddPostProcessor(new QualityBloomPostProcessor(0) { BloomPreset = QualityBloomPostProcessor.BloomPresets.SuperWide, BloomStrengthMultiplier = 0.6f, BloomThreshold = 0.5f });

            GeonDefaultRenderer.ActiveLightsManager.ShadowsEnabed = false;
            GeonDefaultRenderer.ActiveLightsManager.ShadowViewMatrix = Matrix.CreateLookAt(Vector3.Up * 50, Vector3.Down, Vector3.Forward);

            //Config camera5
            Camera.Node.Position = new Vector3(0, 23, 38);
            Camera.Node.RotationX = -0.5f;

            //Prepare physics
            AddSceneComponent(new PhysicsWorld()).SetGravity(new Vector3(0, -150, 0));
            InitEnvironment();

            //Init UI
            _scoreBtn = new VirtualButton(new VirtualButton.KeyboardKey(Microsoft.Xna.Framework.Input.Keys.Tab));
            FinalRenderDelegate = Core.GetGlobalManager<FinalUIRender>();
            UserInterface.Active.Clear();
            InitUI();
            GameState = GameState.Intro;

            //Generate player indices
            _playerIndices = new int[_board.PlayerCount];
            for (int i = 0; i < _playerIndices.Length; i++) _playerIndices[_playerIndices.Length - i - 1] = i;

            //Init
            Core.Schedule(0.3f, x => AdvancePlayer());
#if DEBUG
            Camera.Entity.AddComponent(new Components.Debug.DebugCamMover());
            Core.DebugRenderEnabled = true;
#endif

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

                    if (_thriceRoll == ThriceRollState.ABLE_TO)
                    {
                        if (_diceNumbers.Count > 2)
                        {
                            _diceNumbers.Sort();
                            _thriceRoll = ThriceRollState.ALREADY_PERFORMED;
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (Dice.ShouldReroll(_diceNumbers, _players[_activePlayer].CanRollThrice()))
                    {
                        _uiPlayerReroll.Visible = true;
                    }
                    else
                    {
                        Core.Schedule(0.3f, x => _players[_activePlayer].DecideAfterDiceroll(_diceNumbers));
                        GameState = GameState.PieceSelect;
                    }
                    break;
                case "show_action_result":
                case "show_tutorial":
                    _uiPlayerTutorial.Text = (string)message.Body ?? string.Empty;
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
            var bg = new Nez.Textures.RenderTexture();
            AddRenderer(new PsygroundRenderer(0));
            //CreateGeonEntity("skybox").AddComponent(new SkyBox(bg) { RenderingQueue = RenderingQueue.SolidBackNoCull }); //Create skybox

            //Create playing field
            _board = CreateGeonEntity("board", NodeType.Simple).AddComponent(new BVPlusBoard());
            _players = new BVPlayer[_board.PlayerCount];
            for (int i = 0; i < _board.PlayerCount; i++) _players[i] = CreateGeonEntity("player_" + i).AddComponent(new LocalPlayer(i));

            //saucer
            CreateGeonEntity("saucer", new Vector3(0, 5, 0), NodeType.BoundingBoxCulling).AddComponent(new Saucer());
        }

        protected void InitUI()
        {
            //Add controll panel
            _uiPlayerControls = UserInterface.Active.AddEntity(new Panel(new Vector2(250, 400), PanelSkin.Simple, Anchor.TopRight, new Vector2(15)));
            var btnA = _uiPlayerControls.AddChild(new Button("Dice", ButtonSkin.Alternative, Anchor.TopCenter, new Vector2(200, 80)) { OnClick = x => GameState = GameState.DiceRoll });
            _uiPlayerAnger = _uiPlayerControls.AddChild(new Button("Anger", ButtonSkin.Alternative, Anchor.AutoCenter, new Vector2(200, 80)) { OnClick = x => OpenAnger() });
            _uiPlayerSacrifice = _uiPlayerControls.AddChild(new Button("Sacrifice", ButtonSkin.Alternative, Anchor.AutoCenter, new Vector2(200, 80)) { OnClick = x => OpenSacrifice() });
            _uiPlayerAfK = _uiPlayerControls.AddChild(new Button("AfK", ButtonSkin.Alternative, Anchor.AutoCenter, new Vector2(200, 80)));
            _uiPlayerReroll = UserInterface.Active.AddEntity(new Button("Roll Dice", ButtonSkin.Alternative, Anchor.TopRight, new Vector2(200, 80), new Vector2(15)) { Visible = false, OnClick = x => RollDice() });

            //Add controll panel hints
            btnA.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.A), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));
            _uiPlayerAnger.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.B), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));
            _uiPlayerSacrifice.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.X), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));
            _uiPlayerAfK.AddChild(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.Y), Vector2.One * 35, ImageDrawMode.Stretch, Anchor.CenterLeft, new Vector2(-18, -2)));
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
            _uiPlayerName = UserInterface.Active.AddEntity(new Label(string.Empty, Anchor.TopCenter, null, new Vector2(0, 30)) { FontOverride = Content.Load<SpriteFont>("fonts/player_label"), Text = "" });
            _uiPlayerTutorial = UserInterface.Active.AddEntity(new Label(string.Empty, Anchor.BottomLeft, null, new Vector2(80, 30)) { FontOverride = Content.Load<SpriteFont>("fonts/tutorial_label") });
            _uiPlayerTutorial.Tween("FillColor", Color.Lerp(Color.BlanchedAlmond, Color.Black, 0.7f), 0.7f).SetLoops(LoopType.PingPong, -1).SetEaseType(EaseType.QuadInOut).Start();

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
                _uiPlayerHUDs[_playerIndices[_playerIndices.Length - 1 - i]].Tween("Offset", new Vector2(5, 15 + ii * 75), 0.5f).SetEaseType(Nez.Tweens.EaseType.QuadInOut).Start();
            }
        }

        public void OpenAnger() =>
            //Display message cascade
            MessageBox.ShowMsgBox("[TRIGGERED]", "You get angry, because you suck at this game.", new MessageBox.MsgBoxOption("OK, I get it", () =>
            {
                MessageBox.ShowMsgBox("Anger Button", "You are granted a single Joker. Do you want to utilize it now?", new MessageBox.MsgBoxOption("Nah, I'm good", () => true), new MessageBox.MsgBoxOption("Yes, please!", () =>
                {
                    MessageBox.ShowInputBox("Anger Button", "How far do you want to move? (12 fields are the maximum and 1 field the minimum)", new MessageBox.InputBoxOption("Stop", x =>
                    {
                        MessageBox.ShowMsgBox("Looser", "Alright, then don't.", new MessageBox.MsgBoxOption("Bitch!", () => true));
                        return true;
                    }), new MessageBox.InputBoxOption("Let's go", x =>
                    {
                        //Check if value is valid
                        if (!int.TryParse(x, out int cnt) || cnt < 0 || cnt > 12)
                        {
                            MessageBox.ShowMsgBox("W  A  T  .", "Nah mate, no chance. Enter a proper value.", new MessageBox.MsgBoxOption("Darn", () => true));
                            return false;
                        }
                        //If it is, split up number and return back to the system
                        var lst = new List<int>() { System.Math.Min(cnt, 6) };
                        if (cnt > 6) lst.Add(cnt - 6);
                        _players[_activePlayer].DecideAfterDiceroll(lst);
                        _players[_activePlayer].AngerCount--;
                        return true;
                    }));
                    return true;
                }));
                return true;
            }));

        public void OpenSacrifice() =>
            //Display message cascade
            MessageBox.ShowMsgBox("Send that bastard to hell", "You can sacrifice one of your players to the holy BV gods. The further your player is, the higher is the chance to recieve a positive effect.", new MessageBox.MsgBoxOption("OK, I get it", () =>
            {
                MessageBox.ShowMsgBox("Anger Button", "You really want to sacrifice one of your precious players?", new MessageBox.MsgBoxOption("Nah, I'm good", () =>
                {
                    MessageBox.ShowMsgBox("Looser", "Alright, then don't.", new MessageBox.MsgBoxOption("Bitch!", () => true));
                    return true;
                }), new MessageBox.MsgBoxOption("Yes, please!", () =>
                {
                    var pl = _players[_activePlayer];
                    var possibleChars = pl.GetSacrificableFigures().ToList();
                    if (possibleChars.Count == 0)
                    {
                        MessageBox.ShowMsgBox("Pls plae gaem goodly!", "Nah, sorry mate. There's actually no sacrificable figure out right now!", new MessageBox.MsgBoxOption("Darn >:/", () => true));
                    }
                    else if (possibleChars.Count == 1)
                    {
                        God.Sacrifice(possibleChars[0]);
                    }
                    else
                    {
                        GameState = GameState.PieceSelect;
                        foreach (var item in pl.GetFigures()) item.CanBeSelected = possibleChars.Contains(item);
                        pl.AddComponent(new CharPicker(x => God.Sacrifice(x))); //Open the character picker to choose the traveling distance
                    }
                    return true;
                }));
                return true;
            }));

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
                        Camera.LookAt = new Vector3(-495, 3, -495);
                        Camera.OverridePosition = new Vector3(-470, 50, -470);
                        //Refresh UI elements
                        _uiPlayerTutorial.Text = "Roll the dice!";
                        _uiPlayerControls.Visible = false;
                        _uiPlayerReroll.Visible = true;
                        _diceNumbers.Clear(); //Clear dice queue
                        RollDice();
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
            for (int i = 0; i < (_thriceRoll == ThriceRollState.ABLE_TO ? 3 : 1); i++) Dice.Throw(this);
            _uiPlayerReroll.Visible = false;
        }

        public void AdvancePlayer()
        {
            //Switch to next player & adapt UI
            _activePlayer = ++_activePlayer % _board.PlayerCount;
            var pl = _players[_activePlayer];
            _uiPlayerControls.FillColor = CharConfig.GetStdColor(_activePlayer) * 0.95f;

            //Implement skipping
            if (pl.SkipRound)
            {
                pl.SkipRound = false;
                GameState = GameState.OtherAction;
                _uiPlayerTutorial.Text = "You gotta skip a round!";
                Core.Schedule(1f, x => AdvancePlayer());
                return;
            }

            _uiPlayerAnger.Enabled = pl.AngerCount > 0;
            _uiPlayerName.Text = pl.CharacterConfig.Name;
            _uiPlayerName.Tween("FillColor", _uiPlayerControls.FillColor, 0.5f).Start();
            _uiPlayerSacrifice.Enabled = pl.Sacrificable;
            _uiPlayerTutorial.Text = "Choose an action!";
            _thriceRoll = pl.CanRollThrice() ? ThriceRollState.ABLE_TO : ThriceRollState.UNABLE;
            GameState = GameState.ActionSelect;

            pl.Sacrificable = true;
            pl.CharacterSwitched();

            this.SendPublicTele("player_change", null);
            ReorderPlayerHUD();
        }
    }
}
