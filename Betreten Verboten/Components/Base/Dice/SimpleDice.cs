using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit.UI;
using Nez.GeonBit.UI.Entities;

namespace Betreten_Verboten.Components.Base.Dice
{
    public class SimpleDice : Component, IUpdatable
    {
        //Const
        private const int TICK_COUNT = 20;
        private const float TICK_SLEEP_TIME = 0.01f;
        private const float AFTER_SLEEP_TIME = 0.2f;
        private const float DICE_SIZE = 250f;
        private const float MARGIN = 50f;

        //Fields
        private Image _uiEntity;
        private Image _uiBG;
        private int _ticksLeft = 0;
        private float _tickTimer;
        private int _currentNr;

        public override void OnAddedToEntity()
        {
            _uiEntity = UserInterface.Active.AddEntity(new Image(Core.Content.LoadTexture("texture/dice_simple_eyes"), new Vector2(DICE_SIZE), ImageDrawMode.Stretch, Anchor.BottomRight, new Vector2(-DICE_SIZE, MARGIN)));
            _uiEntity.Background = _uiBG = new Image(Core.Content.LoadTexture("texture/dice_simple_border"), new Vector2(DICE_SIZE), ImageDrawMode.Stretch, Anchor.Center);
        }

        private bool _isVisible = false;
        public bool Visible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _uiEntity.Tween("Offset", _isVisible ? new Vector2(MARGIN) : new Vector2(-DICE_SIZE, MARGIN), 0.1f).SetEaseType(Nez.Tweens.EaseType.CubicInOut).Start();
                //Reset value if fading in
                if (_isVisible)
                {
                    _currentNr = -1;
                    _uiEntity.SourceRectangle = GetNrSourceRect();
                }
            }
        }

        public void SetColor(Color c) => _uiEntity.FillColor = _uiBG.FillColor = c;

        public void Throw()
        {
            if (_ticksLeft > 0) return;
            _ticksLeft = TICK_COUNT;
            _tickTimer = TICK_SLEEP_TIME;
        }

        public void Update()
        {
            if (_ticksLeft < 1) return; //Timer not active -> return

            if ((_tickTimer -= Time.DeltaTime) <= 0)
            {
                //Generate new number
                _currentNr = Random.Range(1, 7);
                _uiEntity.SourceRectangle = GetNrSourceRect();
                //Tick down timer
                _tickTimer = TICK_SLEEP_TIME;
                if (--_ticksLeft < 1) Core.Schedule(AFTER_SLEEP_TIME, x => _currentNr.SendPrivateObj("dice", "base", "dice_value_set"));
            }
        }

        /// <summary>
        /// Returns the source texture rectangle of the currently rolled number.
        /// </summary>
        private Rectangle GetNrSourceRect()
        {
            switch (_currentNr)
            {
                case 1:
                    return new Rectangle(0, 0, 260, 260);
                case 2:
                    return new Rectangle(260, 0, 260, 260);
                case 3:
                    return new Rectangle(520, 0, 260, 260);
                case 4:
                    return new Rectangle(0, 260, 260, 260);
                case 5:
                    return new Rectangle(260, 260, 260, 260);
                case 6:
                    return new Rectangle(520, 260, 260, 260);
                default:
                    return Rectangle.Empty;
            }
        }
    }
}
