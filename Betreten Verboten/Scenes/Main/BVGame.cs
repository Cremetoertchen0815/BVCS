using Betreten_Verboten.Components.Base;
using Betreten_Verboten.Components.Base.Boards.BV;
using Betreten_Verboten.Components.Base.Characters;
using Betreten_Verboten.Components.Base.Dice;
using Betreten_Verboten.Components.BV;
using Betreten_Verboten.Components.BV.Backgrounds;
using Betreten_Verboten.Components.BV.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.ECS;
using Nez.GeonBit.Materials;
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

        //Config
        public DiceType DiceType { get; set; } = DiceType.Simple;

        //Misc
        protected GeonDefaultRenderer _geonRenderer;
        private bool _gameFocussed = true;
        private int _activePlayer = -1;
        private BVPlayer[] _players;
        private BVBoard _board;
        private List<int> _diceNumbers = new List<int>();
        private GameState _gameState = GameState.OtherAction;
        private ThriceRollState _thriceRoll = ThriceRollState.UNABLE;

        //UI
        private bool _scoreTriggerOverride = false;
        private int[] _playerIndices;
        private bool _isScoreVisible = false;
        private Panel[] _uiPlayerHUDs;
        private SimpleDice _uiSimpleDice;
        private DynamicLabel[] _scoreLabels;
        private Label _uiPlayerName;
        private Label _uiPlayerTutorial;
        private Panel _uiPlayerControls;
        private Button _uiPlayerReroll;
        private Button _uiPlayerAnger;
        private Button _uiPlayerSacrifice;
        private Button _uiPlayerAfK;

        //Virtual Controls
        private VirtualButton _btnConfirmNRoll;
        private VirtualButton _btnAnger;
        private VirtualButton _btnSacrifice;
        private VirtualButton _btnAfK;
        private VirtualButton _btnScore;

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

            //Init virtual controls
            _btnConfirmNRoll = new VirtualButton(new VirtualButton.KeyboardKey(Keys.Space), new VirtualButton.GamePadButton(0, Buttons.A));
            _btnSacrifice = new VirtualButton(new VirtualButton.KeyboardKey(Keys.Q), new VirtualButton.GamePadButton(0, Buttons.B));
            _btnAnger = new VirtualButton(new VirtualButton.KeyboardKey(Keys.E), new VirtualButton.GamePadButton(0, Buttons.X));
            _btnAfK = new VirtualButton(new VirtualButton.KeyboardKey(Keys.F), new VirtualButton.GamePadButton(0, Buttons.Y));
            _btnScore = new VirtualButton(new VirtualButton.KeyboardKey(Keys.Tab));

            //Init UI
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
            Core.DebugRenderEnabled = true;
#endif

            this.TeleRegister();
        }
        public override void Unload() => this.TeleDeregister();

        public void MessageReceived(Telegram message)
        {
            switch (message.Head)
            {
                case "dice_value_set": //Dice finished rolling and reports back a certain number rolled
                    int nr = (int)message.Body;
                    _diceNumbers.Add(nr);

                    //Takes into account multiple physical dices getting rolled simultaneously.
                    if (_thriceRoll == ThriceRollState.ABLE_TO && DiceType == DiceType.Physics)
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

                    //Check if any further rerolls are necessary and act accordingly
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
                //Some game component wants to display a message/a piece of information to the player.
                case "show_action_result":
                case "show_tutorial":
                    _uiPlayerTutorial.Text = (string)message.Body ?? string.Empty;
                    break;
                //Some game component reports that the current player is done moving and the next player is due.
                case "char_move_done":
                case "advance_player":
                    if (IsGameOver(out int who)) FinishGame(who); else AdvancePlayer();
                    break;
                //Some game component requests a re-ordering of the scoreboard.
                case "resort_score":
                    ReorderPlayerHUD();
                    break;
            }
        }

        protected void InitEnvironment()
        {
            //Create backgrounds
            AddRenderer(new PsygroundRenderer(0));
            
            //Create table
            var table = CreateGeonEntity("table", new Vector3(0, -26, 0), NodeType.BoundingBoxCulling).AddComponent(new ModelRenderer(Content.LoadModel("mesh/table")));
            table.Node.Scale = Vector3.One * 0.2f;
            table.Node.RotationX = MathHelper.PiOver2;
            table.ShadowsEnabled = false;
            table.RenderingQueue = RenderingQueue.Terrain;
            table.SetMaterial(new NormalMapLitMaterial() { NormalTexture = Content.LoadTexture("mesh/wood_normal"), Texture = Content.LoadTexture("mesh/wood-2609093_960_720"), TextureEnabled = true, CastsShadows = false });

            //Create playing field
            _board = CreateGeonEntity("board", NodeType.Simple).AddComponent(new BVPlusBoard());
            _players = new BVPlayer[_board.PlayerCount];
            for (int i = 0; i < _board.PlayerCount; i++) _players[i] = CreateGeonEntity("player_" + i).AddComponent(new LocalPlayer(i));

            //Create Saucer
            CreateGeonEntity("saucer", NodeType.BoundingBoxCulling).AddComponent(new Saucer());
        }

        protected void InitUI()
        {
            //Add control panel
            _uiPlayerControls = UserInterface.Active.AddEntity(new Panel(new Vector2(250, 400), PanelSkin.Simple, Anchor.TopRight, new Vector2(15)) { FillColor = Color.Transparent });
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

            //Add player score hud
            _uiPlayerHUDs = new Panel[_board.PlayerCount];
            _scoreLabels = new DynamicLabel[_board.PlayerCount];
            var chrCfg = new CharConfig();
            for (int i = 0; i < _board.PlayerCount; i++)
            {
                int ii = i;
                chrCfg.SetStdColorScheme(i);
                _uiPlayerHUDs[i] = UserInterface.Active.AddEntity(new Panel(new Vector2(200, 70), PanelSkin.Default, Anchor.TopLeft, new Vector2(5, 15 + i * 75)) { FillColor = chrCfg.Color });
                _uiPlayerHUDs[i].AddChild(new Image(Graphics.Instance.DebugSprite.Texture2D, new Vector2(40), ImageDrawMode.Stretch, Anchor.CenterLeft));
                _uiPlayerHUDs[i].AddChild(new Label("Player " + i, Anchor.TopLeft, null, new Vector2(50, 0)));
                _scoreLabels[i] = _uiPlayerHUDs[i].AddChild(new DynamicLabel(() => _players[ii].Points.ToString(), Anchor.TopRight));
                _scoreLabels[i].Visible = false;

            }

            //Add simple dice(if mode matches)
            if (DiceType == DiceType.Simple) _uiSimpleDice = CreateEntity("dice").SetTag(Dice.ENTITY_TAG).AddComponent(new SimpleDice());

            //Set up rest of the HUD
            UserInterface.Active.AddEntity(new Image(GamepadIcons.Instance.GetIcon(GamepadIcons.GamepadButton.LT), new Vector2(50, 51), ImageDrawMode.Stretch, Anchor.Auto, new Vector2(80, 0)));
            _uiPlayerName = UserInterface.Active.AddEntity(new Label(string.Empty, Anchor.TopCenter, null, new Vector2(0, 30)) { FontOverride = Content.Load<SpriteFont>("fonts/player_label"), Text = "" });
            _uiPlayerTutorial = UserInterface.Active.AddEntity(new Label(string.Empty, Anchor.BottomLeft, null, new Vector2(80, 30)) { FontOverride = Content.Load<SpriteFont>("fonts/tutorial_label") });
            _uiPlayerTutorial.Tween("FillColor", Color.Lerp(Color.BlanchedAlmond, Color.Black, 0.7f), 0.7f).SetLoops(LoopType.PingPong, -1).SetEaseType(EaseType.QuadInOut).Start();

            //Implement score toggle
            _btnConfirmNRoll.ButtonPressed += () => 
            {
                if (!_gameFocussed) return;

                switch (GameState)
                {
                    case GameState.ActionSelect:
                        GameState = GameState.DiceRoll;
                        break;
                    case GameState.DiceRoll:
                        if (_uiPlayerReroll.Visible && _uiPlayerReroll.Enabled) RollDice();
                        break;
                }
            };
            _btnScore.ButtonReleased += () =>
            {
                if (_isScoreVisible && _gameFocussed)
                {
                    for (int i = 0; i < _board.PlayerCount; i++)
                    {
                        _scoreLabels[i].Visible = false;
                        _uiPlayerHUDs[i].Tween("Size", new Vector2(200, 70), 0.1f).SetCompletionHandler(x => _scoreTriggerOverride = false).Start();
                        _isScoreVisible = false;
                    }
                }
            };
            _btnScore.ButtonPressed += () =>
            {
                if (!_isScoreVisible)
                {
                    _scoreTriggerOverride = true;
                    for (int i = 0; i < _board.PlayerCount; i++)
                    {
                        int ii = i;
                        _uiPlayerHUDs[ii].Tween("Size", new Vector2(400, 70), 0.1f).SetCompletionHandler(x => _scoreLabels[ii].Visible = true).Start();
                        _isScoreVisible = true;
                    }
                }
            };
            _btnAnger.ButtonPressed += () => OpenAnger();
            _btnSacrifice.ButtonPressed += () => OpenSacrifice();
        }

        /// <summary>
        /// Forces a reorder of the scoreboard. Needs to be called when the score has been changed to accomodate the new ranking.
        /// </summary>
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

        public void OpenAnger()
        {
            if (!_gameFocussed || !_uiPlayerAnger.Enabled || GameState != GameState.ActionSelect) return;
            _gameFocussed = false;

            //Display message cascade
            MessageBox.ShowMsgBox("[TRIGGERED]", "You get angry, because you suck at this game.", new MessageBox.MsgBoxOption("OK, I get it", () =>
            {
                MessageBox.ShowMsgBox("Anger Button", "You are granted a single Joker. Do you want to utilize it now?", new MessageBox.MsgBoxOption("Nah, I'm good", () => ExitMsgCascade()), new MessageBox.MsgBoxOption("Yes, please!", () =>
                {
                    MessageBox.ShowInputBox("Anger Button", "How far do you want to move? (12 fields are the maximum and 1 field the minimum)", new MessageBox.InputBoxOption("Stop", x =>
                    {
                        MessageBox.ShowMsgBox("Looser", "Alright, then don't.", new MessageBox.MsgBoxOption("Bitch!", () => ExitMsgCascade()));
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
                        GameState = GameState.OtherAction;
                        return ExitMsgCascade();
                    }));
                    return true;
                }));
                return true;
            }));
        }

        public void OpenSacrifice()
        {
            if (!_gameFocussed || !_uiPlayerSacrifice.Enabled || GameState != GameState.ActionSelect) return;
            _gameFocussed = false;

            //Display message cascade
            MessageBox.ShowMsgBox("Send that bastard to hell", "You can sacrifice one of your players to the holy BV gods. The further your player is, the higher is the chance to recieve a positive effect.", new MessageBox.MsgBoxOption("OK, I get it", () =>
            {
                MessageBox.ShowMsgBox("Sacrifice Button", "You really want to sacrifice one of your precious players?", new MessageBox.MsgBoxOption("Nah, I'm good", () =>
                {
                    MessageBox.ShowMsgBox("Looser", "Alright, then don't.", new MessageBox.MsgBoxOption("Bitch!", () => ExitMsgCascade()));
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
                    GameState = GameState.OtherAction;
                    return ExitMsgCascade();
                }));
                return true;
            }));
        }

        /// <summary>
        /// Prepares a MsgBox/InputBox dialogue to be closed correctly.
        /// </summary>
        /// <returns>Returns always true.</returns>
        private bool ExitMsgCascade()
        {
            _gameFocussed = true; //Enable virtual buttons
            UserInterface.GamePadModeEnabled = false; //Disable GeonBit UI gamepad mode
            return true;
        }

        //Allows the flow of the game to be controlled by setting this property.
        public GameState GameState
        {
            get => _gameState;
            set
            {
                if (_gameState == value) return; //Ignore same non-value changing triggers

                //Switch for old state
                switch (_gameState)
                {
                    case GameState.DiceRoll:
                        if (DiceType == DiceType.Physics)
                            FindEntitiesWithTag(Dice.ENTITY_TAG).ForEach(x => x.Destroy());
                        else
                            _uiSimpleDice.Visible = false;
                        break;
                }

                //Switch for new state
                switch (_gameState = value)
                {
                    case GameState.DiceRoll:
                        if (DiceType == DiceType.Physics)
                        {
                            //Set camera position
                            Camera.LookAt = PhysicsDice.GetCamFocusOverride();
                            Camera.OverridePosition = PhysicsDice.GetCamPosOverride();
                        }
                        else
                        {
                            _uiSimpleDice.Visible = true;
                        }
                        //Refresh UI elements
                        _uiPlayerTutorial.Text = "Roll the dice!";
                        _uiPlayerControls.Visible = false;
                        _uiPlayerReroll.Visible = true;
                        _diceNumbers.Clear(); //Clear dice queue
                        RollDice();
                        break;
                    case GameState.Outro:
                        FindEntity("saucer").GetComponent<Saucer>().Tween("Rotator", 6f, 5f).SetEaseType(EaseType.CubicInOut).Start();
                        Camera.LookAt = new Vector3(0, 0, 0);
                        _uiPlayerTutorial.Text = "Game is over!";
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

        public override void Update()
        {
            //Update scoreboard size by the amount of trigger press on the player's gamepad.
            if (Input.GamePads[0].IsConnected() && !_scoreTriggerOverride)
            {
                var val = Input.GamePads[0].GetLeftTriggerRaw();
                for (int i = 0; i < _board.PlayerCount; i++)
                {
                    _scoreLabels[i].Visible = val > 1 - Mathf.Epsilon;
                    _uiPlayerHUDs[i].Size = new Vector2(200 + val * 200, 70);
                    _isScoreVisible = val > Mathf.Epsilon;
                }
            }
            base.Update();
        }

        /// <summary>
        /// Trigger the dice to roll(physical and simple).
        /// </summary>
        private void RollDice()
        {
            if (DiceType == DiceType.Physics)
                for (int i = 0; i < (_thriceRoll == ThriceRollState.ABLE_TO ? 3 : 1); i++) PhysicsDice.Throw(this);
            else
                _uiSimpleDice.Throw();

            _uiPlayerReroll.Visible = false;
        }

        /// <summary>
        /// Returns true if one of the players won the game.
        /// </summary>
        /// <param name="who">The player who has won the game.</param>
        public bool IsGameOver(out int who)
        {
            for (int i = 0; i < Board.PlayerCount; i++)
            {
                var figs = _players[i].GetFigures();
                var hasWon = true;
                for (int j = 0; j < Board.FigureCountPP; j++) if (figs[j].Position < Board.FieldCountTotal) hasWon = false;
                who = i;
                if (hasWon) return true;
            }
            who = -1;
            return false;
        }

        /// <summary>
        /// Prepares the game for its ending & plays ending animations for the characters.
        /// </summary>
        /// <param name="playerWon">The ID of the player who won the game.</param>
        public void FinishGame(int playerWon)
        {
            GameState = GameState.Outro;
            int action = Random.Range(0, 3);
            //Play outro animation for each player's characters
            for (int i = 0; i < _players.Length; i++)
            {
                //Ignore winner & buffer chars
                if (playerWon == i) continue;
                var fg = _players[i].GetFigures();
                for (int j = 0; j < Board.FigureCountPP; j++)
                {
                    //Play outro animation
                    switch (action)
                    {
                        case 0: //Fly upwards
                            fg[j].Node.Tween("PositionY", 50f, 3f).SetEaseType(EaseType.CubicIn).Start();
                            break;
                        case 1: //Tip over
                            fg[j].RigidBodyEnabled = true;
                            var angle = Random.NextAngle();
                            fg[j].RigidBody.AngularVelocity = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 15f;
                            break;
                        case 2: //Shrink
                            fg[j].Node.Tween("Scale", Vector3.Zero, 2f).SetEaseType(EaseType.CubicIn).Start();
                            fg[j].Node.Tween("PositionY", 0f, 2f).SetEaseType(EaseType.CubicIn).Start();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Is getting called when a player's round is over and a new player shall take over, while getting everything ready.
        /// </summary>
        public void AdvancePlayer()
        {
            //Switch to next player
            _activePlayer = ++_activePlayer % _board.PlayerCount;
            var pl = _players[_activePlayer];
            var plClor = CharConfig.GetStdColor(_activePlayer);

            //Implement skipping
            if (pl.SkipRound)
            {
                pl.SkipRound = false;
                GameState = GameState.OtherAction;
                _uiPlayerTutorial.Text = "You gotta skip a round!";
                Core.Schedule(1f, x => AdvancePlayer());
                return;
            }

            //Update UI
            _uiPlayerName.Text = pl.CharacterConfig.Name;
            _uiPlayerName.Tween("FillColor", plClor, 0.2f).Start();
            _uiSimpleDice?.SetColor(plClor);
            _uiPlayerSacrifice.Enabled = pl.Sacrificable;
            _uiPlayerAnger.Enabled = pl.AngerCount > 0;
            _uiPlayerTutorial.Text = "Choose an action!";

            //Prepare misc variables
            _thriceRoll = pl.CanRollThrice() ? ThriceRollState.ABLE_TO : ThriceRollState.UNABLE;
            GameState = GameState.ActionSelect;
            pl.Sacrificable = true;
            pl.CharacterSwitched();

            this.SendPublicTele("player_change", null); //Notify everyone
            ReorderPlayerHUD(); //Forces a re-ordering of the scoreboard
        }
    }
}
